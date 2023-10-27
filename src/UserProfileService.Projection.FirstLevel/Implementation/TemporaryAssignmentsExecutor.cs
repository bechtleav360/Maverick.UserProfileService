using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using ObjectTypeAPI = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Projection.FirstLevel.Implementation;

internal class TemporaryAssignmentsExecutor : ITemporaryAssignmentsExecutor
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly ILogger _Logger;
    private readonly IMapper _Mapper;
    private readonly IFirstLevelProjectionRepository _Repository;
    private readonly ISagaService _SagaService;

    public TemporaryAssignmentsExecutor(
        ILogger<TemporaryAssignmentsExecutor> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper)
    {
        _Logger = logger;
        _Repository = repository;
        _SagaService = sagaService;
        _Creator = creator;
        _Mapper = mapper;
    }

    private async Task TryExecuteBatchAsync(
        TemporaryAssignmentExtended assignment,
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        try
        {
            await _SagaService.ExecuteBatchAsync(assignment.BatchId, cancellationToken);

            assignment.Assignment.UpdateState();
            assignment.Assignment.NotificationStatus = UpdateNotificationState(assignment);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _Logger.LogWarnMessage(
                e,
                "Error occurred during executing saga service batch. Temporary assignments in batch (id = {batchId}) could not been executed.",
                LogHelpers.Arguments(assignment.BatchId));

            assignment.Assignment.LastErrorMessage = e.Message;
            assignment.Assignment.State = TemporaryAssignmentState.ErrorOccurred;
        }

        _Logger.ExitMethod();
    }

    private async IAsyncEnumerable<TemporaryAssignmentExtended> TryPushEventsAsync(
        IList<FirstLevelProjectionTemporaryAssignment> temporaryAssignments,
        IDatabaseTransaction transaction,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        foreach (FirstLevelProjectionTemporaryAssignment assignment in temporaryAssignments)
        {
            if (assignment == null)
            {
                _Logger.LogWarnMessage(
                    "Assignment object should not be null.",
                    LogHelpers.Arguments());

                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var assignmentExtended = new TemporaryAssignmentExtended(assignment);

            try
            {
                IList<FirstLevelRelationProfile> children =
                    await _Repository.GetAllChildrenAsync(
                        new ObjectIdent(
                            assignment.TargetId,
                            assignment.TargetType),
                        transaction,
                        cancellationToken);

                assignmentExtended.BatchId = await _SagaService.CreateBatchAsync(
                    cancellationToken,
                    await CreateEventsAsync(
                            assignment,
                            children.Select(pr => pr.Profile).ToList(),
                            cancellationToken)
                        // in this case all events will be the same, the stream name is the only relevant identifier
                        .Distinct(new StreamNameEventTupleComparer())
                        .ToArrayAsync(cancellationToken));

                List<EventTuple> clientEventTuples = await CreateClientSettingsEventsAsync(
                        assignment,
                        children.Select(pr => pr.Profile).ToList(),
                        transaction,
                        cancellationToken)
                    .ToListAsync(cancellationToken);

                if (clientEventTuples.Any())
                {
                    await _SagaService.AddEventsAsync(
                        assignmentExtended.BatchId,
                        clientEventTuples,
                        cancellationToken);
                }

                _Logger.LogDebugMessage(
                    "Batch created: Id = {batchId}",
                    LogHelpers.Arguments(assignmentExtended.BatchId));
            }
            catch (OperationCanceledException)
            {
                _Logger.LogDebugMessage(
                    "Operation aborted.",
                    LogHelpers.Arguments());

                yield break;
            }
            catch (Exception e)
            {
                assignmentExtended.Assignment.LastErrorMessage = e.Message;
                assignmentExtended.Assignment.State = TemporaryAssignmentState.ErrorOccurred;

                _Logger.LogWarnMessage(
                    e,
                    "Error occurred during creating saga service batch. Temporary assignment (id = {temporaryAssignmentId}) could not been processed.",
                    LogHelpers.Arguments(assignment.Id));

                continue;
            }

            yield return assignmentExtended;
        }

        _Logger.ExitMethod();
    }

    private async IAsyncEnumerable<EventTuple> CreateEventsAsync(
        FirstLevelProjectionTemporaryAssignment assignment,
        IList<IFirstLevelProjectionProfile> children,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        var assignmentConditionTriggered = new AssignmentConditionTriggered
        {
            ProfileId = assignment.ProfileId,
            TargetId = assignment.TargetId,
            TargetObjectType =
                _Mapper.Map<ObjectType>(assignment.TargetType),
            IsActive = assignment.State == TemporaryAssignmentState.Active
                || assignment.State
                == TemporaryAssignmentState.ActiveWithExpiration
        };

        yield return _Creator.CreateEvent(
            new ObjectIdent(
                assignment.ProfileId,
                assignment.ProfileType),
            assignmentConditionTriggered);

        cancellationToken.ThrowIfCancellationRequested();

        yield return _Creator.CreateEvent(
            new ObjectIdent(
                assignment.TargetId,
                assignment.TargetType),
            assignmentConditionTriggered);

        cancellationToken.ThrowIfCancellationRequested();

        foreach (IFirstLevelProjectionProfile child in children)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return _Creator.CreateEvent(
                child.ToObjectIdent(),
                assignmentConditionTriggered);
        }

        _Logger.ExitMethod();
    }

    private async IAsyncEnumerable<EventTuple> CreateClientSettingsEventsAsync(
        FirstLevelProjectionTemporaryAssignment assignment,
        IList<IFirstLevelProjectionProfile> children,
        IDatabaseTransaction transaction,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Add Client-Settings when the target is a group
        // or an organization.
        if (assignment.TargetType == ObjectTypeAPI.Group
            || assignment.TargetType == ObjectTypeAPI.Organization)
        {
            IList<FirstLevelProjectionsClientSetting> calculatedClientSettingsList =
                await _Repository.GetCalculatedClientSettingsAsync(
                    assignment.TargetId,
                    transaction,
                    cancellationToken);

            if (calculatedClientSettingsList.Any())
            {
                List<ObjectIdent> childrenForClientSettings = children.Select(child => child.ToObjectIdent())
                    .Append(
                        new ObjectIdent(
                            assignment.ProfileId,
                            assignment.ProfileType))
                    .ToList();

                List<EventTuple> clientSettingsEvents = await CreateClientSettingsEventTuplesAsync(
                    childrenForClientSettings,
                    transaction,
                    cancellationToken);

                foreach (EventTuple clientSettingsEvent in clientSettingsEvents)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    yield return clientSettingsEvent;
                }
            }
        }
    }

    private async Task<List<EventTuple>> CreateClientSettingsEventTuplesAsync(
        IList<ObjectIdent> children,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        if (children == null)
        {
            throw new ArgumentNullException(nameof(children));
        }

        var clientSettingsEventTuple = new List<EventTuple>();

        foreach (ObjectIdent child in children)
        {
            IList<FirstLevelProjectionsClientSetting> clientSettingsChild =
                await _Repository.GetCalculatedClientSettingsAsync(child.Id, transaction, cancellationToken);

            List<IUserProfileServiceEvent> activeClientSettings =
                clientSettingsChild.GetClientSettingsCalculatedEvents(child.Id);

            IEnumerable<EventTuple> activeClientSettingTupleEvents = _Creator.CreateEvents(
                child,
                activeClientSettings);

            // adds a new event so that only the new keys from the client settings
            // are valid
            EventTuple validClientSettingTupleEvent = _Creator.CreateEvent(
                child,
                new ClientSettingsInvalidated
                {
                    Keys = activeClientSettings
                        .Select(cls => ((ClientSettingsCalculated)cls).Key)
                        .ToArray(),
                    ProfileId = child.Id
                });

            clientSettingsEventTuple.AddRange(activeClientSettingTupleEvents.Append(validClientSettingTupleEvent));
        }

        return _Logger.ExitMethod(clientSettingsEventTuple);
    }

    // somehow a fire-and-forget - just to be on the safe side
    private Task TryAbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            return _Repository.AbortTransactionAsync(transaction, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _Logger.LogDebugMessage(
                "Operation aborted.",
                LogHelpers.Arguments());
        }
        catch (Exception e)
        {
            // Maybe an error occurred before calling this method
            // then aborting the transaction won't work.
            _Logger.LogDebugMessage(
                e,
                "Could not abort transaction, because of an occurred error. {errorMessage}",
                LogHelpers.Arguments(e.Message));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Updates the notification state while comparing the old with the new <see cref="TemporaryAssignmentState" />.
    /// </summary>
    /// <param name="entity">Includes the old an new state for the <see cref="TemporaryAssignmentState" />.</param>
    /// <returns>The updated <see cref="TemporaryAssignmentState" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the old <see cref="TemporaryAssignmentState" /> is not met.</exception>
    private NotificationStatus UpdateNotificationState(TemporaryAssignmentExtended entity)
    {
        return entity.OldTemporaryState switch
        {
            // Here the assignment is not processed and should change the state
            // from notProcessed --> activeWithExpiration
            // Also the NotificationStatus changes from NotSent --> ActivationSent
            TemporaryAssignmentState.NotProcessed when entity.Assignment.State
                == TemporaryAssignmentState.ActiveWithExpiration
                && !entity.Assignment.NotificationStatus.HasFlag(NotificationStatus.ActivationSent) =>
                NotificationStatus.ActivationSent,

            // Here the assignment is not active and should change the state from
            // activeWithExpiration --> inactive
            // The NotificationStatus changes from NotSent --> BothSend
            TemporaryAssignmentState.ActiveWithExpiration when
                entity.Assignment.State == TemporaryAssignmentState.Inactive
                && !entity.Assignment.NotificationStatus.HasFlag(NotificationStatus.DeactivationSent) =>
                entity.Assignment.NotificationStatus |= NotificationStatus.DeactivationSent,

            // Here the assignment is not active and should change the state from
            // notProcessed --> active
            // The NotificationStatus changes from NotSent --> ActivationSent (This notification state won't change,
            // because the assignment will last forever)
            TemporaryAssignmentState.NotProcessed when
                entity.Assignment.State == TemporaryAssignmentState.Active
                && !entity.Assignment.NotificationStatus.HasFlag(NotificationStatus.ActivationSent) => entity
                    .Assignment.NotificationStatus = NotificationStatus.ActivationSent,

            // if no condition is met, throw an exceptions with further information.
            _ => throw new ArgumentOutOfRangeException(
                $"Temporary old state: {entity.OldTemporaryState}. The notification state: {entity.Assignment.NotificationStatus}. The combination of these states are not be possible!.")
        };
    }

    /// <inheritdoc />
    public async Task CheckTemporaryAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        IDatabaseTransaction transaction = await _Repository.StartTransactionAsync(cancellationToken);

        try
        {
            IList<FirstLevelProjectionTemporaryAssignment> temp = await _Repository.GetTemporaryAssignmentsAsync(
                transaction,
                cancellationToken);

            if (temp == null || temp.Count == 0)
            {
                _Logger.ExitMethod();

                return;
            }

            _Logger.LogInfoMessage(
                "Found temporary assignments that have to be activated/deactivated: {temporaryAssignmentsAmount}",
                LogHelpers.Arguments(temp.Count));

            List<TemporaryAssignmentExtended> extendedAssignments =
                await TryPushEventsAsync(temp, transaction, cancellationToken)
                    .ToListAsync(cancellationToken);

            _Logger.LogDebugMessage(
                "Amount of {sagaServiceBatchAmount} saga service batches created.",
                LogHelpers.Arguments(
                    extendedAssignments.Count(a => string.IsNullOrEmpty(a.Assignment.LastErrorMessage))));

            cancellationToken.ThrowIfCancellationRequested();

            foreach (TemporaryAssignmentExtended extendedAssignment in extendedAssignments)
            {
                await TryExecuteBatchAsync(extendedAssignment, cancellationToken);
            }

            _Logger.LogTraceMessage(
                "Executed all batches by saga service",
                LogHelpers.Arguments());

            await _Repository.UpdateTemporaryAssignmentStatesAsync(
                temp,
                transaction,
                cancellationToken);

            _Logger.LogTraceMessage(
                "Saved modified state of temporary assignments",
                LogHelpers.Arguments());

            cancellationToken.ThrowIfCancellationRequested();

            await _Repository.CommitTransactionAsync(transaction, cancellationToken);

            _Logger.LogInfoMessage(
                "Relevant temporary assignments processed.",
                LogHelpers.Arguments());
        }
        catch (OperationCanceledException)
        {
            _Logger.LogInfoMessage(
                "Operation aborted.",
                LogHelpers.Arguments());

            // the original cancellation token will already be cancelled
            // that's why a new one should be passed to the method (either CancellationToken.None
            // or one that will cancelled after a short time period)
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            await TryAbortTransactionAsync(
                transaction,
                cts.Token);
        }
        catch (Exception)
        {
            await TryAbortTransactionAsync(transaction, cancellationToken);

            throw;
        }

        _Logger.ExitMethod();
    }

    private class StreamNameEventTupleComparer : IEqualityComparer<EventTuple>
    {
        public bool Equals(EventTuple x, EventTuple y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.TargetStream == y.TargetStream;
        }

        public int GetHashCode(EventTuple obj)
        {
            return obj.TargetStream != null ? obj.TargetStream.GetHashCode() : 0;
        }
    }

    /// <summary>
    ///     This extended class is used to store additional information for the
    ///     <see cref="FirstLevelProjectionTemporaryAssignment" />s. The extended class
    ///     store the batchId and the old <see cref="TemporaryAssignmentState" />.
    /// </summary>
    private class TemporaryAssignmentExtended
    {
        internal FirstLevelProjectionTemporaryAssignment Assignment { get; }

        internal Guid BatchId { get; set; }

        internal TemporaryAssignmentState OldTemporaryState { get; }

        /// <summary>
        ///     Create an instance <see cref="TemporaryAssignmentExtended" />.
        /// </summary>
        /// <param name="entity">The entity that should be stored in the <see cref="TemporaryAssignmentExtended" />.</param>
        public TemporaryAssignmentExtended(FirstLevelProjectionTemporaryAssignment entity)
        {
            Assignment = entity;
            OldTemporaryState = entity.State;
        }
    }
}
