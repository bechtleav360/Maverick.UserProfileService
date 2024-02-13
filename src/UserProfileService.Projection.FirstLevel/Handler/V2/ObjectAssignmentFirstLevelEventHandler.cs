using System;
using System.Collections.Generic;
using System.Linq;
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
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using ResolvedRangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;
using ResolvedInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using ResolvedMember = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using InitiatorResolved = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ResolvedProfileKind = Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="ObjectAssignmentEvent" />.
/// </summary>
internal class ObjectAssignmentFirstLevelEventHandler : FirstLevelEventHandlerBase<ObjectAssignmentEvent>
{
    private Guid _BatchSagaId;
    private readonly IFirstLevelEventTupleCreator _Creator;
    private ObjectAssignmentEvent _EventObject;
    private readonly IMapper _Mapper;
    private readonly HashSet<ObjectIdent> _ProfilesToRecalculateClientSettings;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="ObjectAssignmentFirstLevelEventHandler" />.
    /// </summary>
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
    /// <param name="logger">
    ///     The logger factory that is used to create a logger. The logger logs message for debugging
    ///     and control reasons.
    /// </param>
    /// <param name="firstLevelProjectionRepository">
    ///     The read service is used to read from the internal query storage to get all information to
    ///     generate all needed stream events.
    /// </param>
    /// <param name="sagaService">
    ///     The saga service is used to write all created <see cref="IUserProfileServiceEvent" /> to the
    ///     write stream.
    /// </param>
    /// <param name="creator">The creator is used to create <inheritdoc cref="EventTuple" /> from the given parameter.</param>
    public ObjectAssignmentFirstLevelEventHandler(
        IMapper mapper,
        ILogger<ObjectAssignmentFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository firstLevelProjectionRepository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator) : base(logger, firstLevelProjectionRepository)
    {
        _Mapper = mapper;
        _SagaService = sagaService;
        _Creator = creator;
        _ProfilesToRecalculateClientSettings = new HashSet<ObjectIdent>();
    }

    // use only for testing purposes
    public ObjectAssignmentFirstLevelEventHandler(
        IMapper mapper,
        ILogger<ObjectAssignmentFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository firstLevelProjectionRepository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        Guid batchSagaId,
        ObjectAssignmentEvent eventEvent
    ) : base(logger, firstLevelProjectionRepository)
    {
        _Mapper = mapper;
        _SagaService = sagaService;
        _Creator = creator;
        _ProfilesToRecalculateClientSettings = new HashSet<ObjectIdent>();
        _BatchSagaId = batchSagaId;
        _EventObject = eventEvent;
    }

    private async Task ResolveTargetTypesAsync(
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken,
        ConditionObjectIdent[] identCollection)
    {
        foreach (ConditionObjectIdent ident in identCollection)
        {
            if (ident.Type != ObjectType.Profile)
            {
                continue;
            }

            IFirstLevelProjectionProfile targetProfile = await Repository.GetProfileAsync(
                ident.Id,
                databaseTransaction,
                cancellationToken);

            ident.Type = targetProfile.Kind.ToObjectType();
        }
    }

    /// <summary>
    ///     This method is needed to assign a container to a profile.
    ///     Thereby the container type can only be a role or a function.
    ///     So there is no need to set client setting or tag assignments.
    ///     They won't be inherited. The assignments can be:
    ///     function --> group
    ///     function --> user
    ///     role --> group
    ///     role --> user
    /// </summary>
    /// <param name="profileAssignments">
    ///     The assignments grouped by id and the assignments. Basically it is a list of
    ///     assignments for a container. In this case the container is a role or function.
    /// </param>
    /// <param name="container">The container to which the profiles has to be assigned.</param>
    /// <param name="databaseTransaction">The transaction that is used to call all database related methods.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellationToken requests.</param>
    /// <returns>A task that represent the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">If an arguments is null.</exception>
    /// <exception cref="NotSupportedException">If the container to which has to be to assigned is not a profile.</exception>
    internal async Task AssignmentsToContainerTypeAsync(
        IGrouping<string, Assignment> profileAssignments,
        IFirstLevelProjectionContainer container,
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken
    )
    {
        Logger.EnterMethod();

        if (profileAssignments == null)
        {
            throw new ArgumentNullException(nameof(profileAssignments));
        }

        if (databaseTransaction == null)
        {
            throw new ArgumentNullException(nameof(databaseTransaction));
        }

        if (container is IFirstLevelProjectionProfile)
        {
            throw new NotSupportedException($"Use the method {nameof(AssignmentsToProfileTypeAsync)} instead.");
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The profile assignment: {profileAssignment}, the container: {container}",
                LogHelpers.Arguments(profileAssignments.ToLogString(), container.ToLogString()));
        }

        Logger.LogInfoMessage("Starting to generating container assignments.", LogHelpers.Arguments());

        List<Tuple<Assignment, ObjectIdent>> profileAssignment =
            (await Task.WhenAll(
                profileAssignments.Select(ass => ExtractAssignment(ass, databaseTransaction, cancellationToken))))
            .SelectMany(t => t)
            .ToList();

        List<EventTuple> wasAssignedEventTuples = profileAssignment.Select(
                t => _Creator.CreateEvent(
                    t.Item2,
                    GenerateWasAssignedToEvent(
                        new
                            FirstLevelProjectionTreeEdgeRelation
                            {
                                Conditions = t.Item1.Conditions,
                                Parent = container,
                                Child = new ObjectIdent(
                                    t.Item1.ProfileId,
                                    ObjectType.Profile)
                            }),
                    _EventObject))
            .ToList();

        await _SagaService.AddEventsAsync(
            _BatchSagaId,
            wasAssignedEventTuples,
            cancellationToken);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The created was-assigned-event tuple: {wasAssignedEventTuples}",
                wasAssignedEventTuples.ToLogString().AsArgumentList());
        }

        IList<IUserProfileServiceEvent> memberCreatedAssignments = await CreateMemberAssignmentsAsync(
            profileAssignments,
            container.ToObjectIdent(),
            databaseTransaction,
            cancellationToken);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The created member assign-events as tuple: {memberAssignmentTuple},",
                wasAssignedEventTuples.ToLogString().AsArgumentList());
        }

        await _SagaService.AddEventsAsync(
            _BatchSagaId,
            _Creator.CreateEvents(
                container.ToObjectIdent(),
                memberCreatedAssignments,
                _EventObject),
            cancellationToken);

        Logger.ExitMethod();
    }

    /// <summary>
    ///     This method is used to assign profiles to each other:
    ///     group --> group
    ///     user ---> group
    ///     organization --> organization
    /// </summary>
    /// <param name="profileAssignments">
    ///     The assignments grouped by id and the assignments. Basically it is a list of
    ///     assignments for a container.
    /// </param>
    /// <param name="databaseTransaction">The transaction that is used to call all database related methods.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellationToken requests.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If the assignments are null.</exception>
    /// <exception cref="NotSupportedException">When the parent type is not a container.</exception>
    /// <returns>A task that represent the asynchronous operation.</returns>
    internal async Task AssignmentsToProfileTypeAsync(
        IGrouping<string, Assignment> profileAssignments,
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (profileAssignments == null)
        {
            throw new ArgumentNullException(nameof(profileAssignments));
        }

        if (databaseTransaction == null)
        {
            throw new ArgumentNullException(nameof(databaseTransaction));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Profile assignments: {profileAssignments}",
                profileAssignments.ToLogString().AsArgumentList());
        }

        string parentId = profileAssignments.Key;

        Logger.LogInfoMessage(
            "The profile assignments will be added to the profile: {parentId}",
            parentId.ToLogString().AsArgumentList());

        IFirstLevelProjectionProfile parent = await Repository.GetProfileAsync(
            parentId,
            databaseTransaction,
            cancellationToken);

        if (!(parent is IFirstLevelProjectionContainer))
        {
            throw new NotSupportedException("Parent should be a container type, method will be canceled.");
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The parent with the id {parentId} was found: {parentProfile}",
                LogHelpers.Arguments(parentId.ToLogString(), parent.ToLogString()));
        }

        Logger.LogInfoMessage(
            "The parent with the id was found in the repository.",
            parentId.ToLogString().AsArgumentList());

        // Get all children of the assignments an created a list of assignments to object idents.
        List<Tuple<Assignment, ObjectIdent>> profileAssignment =
            (await Task.WhenAll(
                profileAssignments.Select(ass => ExtractAssignment(ass, databaseTransaction, cancellationToken))))
            .SelectMany(t => t)
            .ToList();

        IList<FirstLevelProjectionsClientSetting> clientSetting =
            await Repository.GetCalculatedClientSettingsAsync(
                parentId,
                databaseTransaction,
                cancellationToken);

        if (clientSetting.Any())
        {
            Logger.LogInfoMessage(
                "The parent profile with the id {id} has client settings, so the assigned members client settings have to be recalculated: {clientSettings}",
                LogHelpers.Arguments(
                    parentId.ToLogString(),
                    profileAssignment.Select(ass => ass.Item2).ToLogString()));

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "The client setting for the parent profile: {clientSetting} ",
                    clientSetting.ToString().AsArgumentList());
            }

            _ProfilesToRecalculateClientSettings.UnionWith(profileAssignment.Select(ass => ass.Item2));
        }

        // get only the ids of the parent + child to assignments.
        List<string> profileIds = profileAssignment.Select(tass => tass.Item2.Id).Distinct().ToList();

        // get the difference, so that we know which event should be generated for the children.
        IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> differences =
            Repository.GetDifferenceInParentsTreesAsync(
                parentId,
                profileIds,
                databaseTransaction,
                cancellationToken);

        // iterate through the differences.
        await foreach (FirstLevelProjectionParentsTreeDifferenceResult difference in differences.WithCancellation(
                           cancellationToken))
        {
            List<Tuple<Assignment, ObjectIdent>> ownProfileAssignments = profileAssignment
                .Where(
                    p => p.Item2.Id
                        == difference.ReferenceProfileId)
                .ToList();

            List<IUserProfileServiceEvent> resolvedTagsAddedAssignment =
                GenerateTagAddedEvents(difference).ToList();

            Logger.LogDebugMessage(
                "The were {tagEventTupleCount} tag event tuples created",
                resolvedTagsAddedAssignment.Count.ToLogString().AsArgumentList());

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Generated tag event tuple {resolvedTagsAddedAssignment}",
                    resolvedTagsAddedAssignment.ToLogString().AsArgumentList());
            }

            List<IUserProfileServiceEvent> resolvedWasAssignedEvent =
                difference.MissingRelations.Select(GenerateWasAssignedToEvent).ToList();

            Logger.LogDebugMessage(
                "The were {tagEventTupleCount} was-assign-event-tuples created",
                resolvedWasAssignedEvent.Count.ToLogString().AsArgumentList());

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Generated tag event tuple {resolvedTagsAddedAssignment}",
                    resolvedWasAssignedEvent.ToLogString().AsArgumentList());
            }

            List<IUserProfileServiceEvent> treeAdditionalAssignments = ownProfileAssignments
                .Select(
                    misRel =>
                        new
                            FirstLevelProjectionTreeEdgeRelation
                            {
                                Child = new ObjectIdent(
                                    misRel.Item1.ProfileId,
                                    ObjectType.Profile),
                                Conditions =
                                    misRel.Item1.Conditions,
                                Parent = difference.Profile,
                                ParentTags =
                                    difference.ProfileTags
                            })
                .Select(GenerateWasAssignedToEvent)
                .ToList();

            Logger.LogDebugMessage(
                "There were {numberAssignments} was-assignments created.",
                treeAdditionalAssignments.Count.ToLogString().AsArgumentList());

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Generated assignments events: {assignments}",
                    treeAdditionalAssignments.ToLogString().AsArgumentList());
            }

            List<EventTuple> eventTupleWasAssignments = resolvedWasAssignedEvent
                .Union(treeAdditionalAssignments)
                .Select(
                    upsEvents =>
                        _Creator.CreateEvent(
                            ownProfileAssignments
                                .First()
                                .Item2,
                            upsEvents,
                            _EventObject))
                .ToList();

            Logger.LogDebugMessage(
                "The were {countEventTuple} event tuple created.",
                eventTupleWasAssignments.Count.ToLogString().AsArgumentList());

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "The assignments as event tuple {eventTupleWasAssignments}",
                    eventTupleWasAssignments.ToLogString().AsArgumentList());
            }

            List<EventTuple> eventTupleTagsAdded = resolvedTagsAddedAssignment.Select(
                    tagAdded =>
                        _Creator.CreateEvent(
                            ownProfileAssignments.First().Item2,
                            tagAdded,
                            _EventObject))
                .ToList();

            Logger.LogDebugMessage(
                "There were {numberEventTupleTagsAdded} tag tuple events created",
                eventTupleTagsAdded.Count.ToLogString().AsArgumentList());

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "The following tag event Tuple: {eventTupleTagsAdded}",
                    eventTupleTagsAdded.ToLogString().AsArgumentList());
            }

            await _SagaService.AddEventsAsync(
                _BatchSagaId,
                eventTupleWasAssignments.Concat(eventTupleTagsAdded),
                cancellationToken);
        }

        IList<IUserProfileServiceEvent> memberAddedEvents = await CreateMemberAssignmentsAsync(
            profileAssignments,
            parent.ToObjectIdent(),
            databaseTransaction,
            cancellationToken);

        Logger.LogDebugMessage(
            "There were {numberAddedEvents} member added events created.",
            memberAddedEvents.Count.ToLogString().AsArgumentList());

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The following member events were created: {memberAddedEvents}.",
                memberAddedEvents.ToLogString().AsArgumentList());
        }

        await _SagaService.AddEventsAsync(
            _BatchSagaId,
            _Creator.CreateEvents(
                parent.ToObjectIdent(),
                memberAddedEvents,
                _EventObject),
            cancellationToken);
    }

    /// <summary>
    ///     Creates all client settings for the profiles where the client settings really have to be recalculated.
    /// </summary>
    /// <param name="databaseTransaction">The transaction that is used to call all database related methods.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellationToken requests.</param>
    /// <returns>A task that represent the asynchronous operation.</returns>
    internal async Task CreateAllClientSettingRecalculationAsync(
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        foreach (ObjectIdent profilesToRecalculateClientSetting in _ProfilesToRecalculateClientSettings)
        {
            IList<FirstLevelProjectionsClientSetting> allClientSettings =
                await Repository.GetCalculatedClientSettingsAsync(
                    profilesToRecalculateClientSetting.Id,
                    databaseTransaction,
                    cancellationToken);

            List<IUserProfileServiceEvent> validClientSetting =
                allClientSettings.GetClientSettingsCalculatedEvents(profilesToRecalculateClientSetting.Id);

            IEnumerable<EventTuple> validClientSettingTupleEvents = _Creator.CreateEvents(
                profilesToRecalculateClientSetting,
                validClientSetting,
                _EventObject);

            // adds a new event so that only the new keys from the client settings
            // are valid
            EventTuple validClientSettingTupleEvent = _Creator.CreateEvent(
                profilesToRecalculateClientSetting,
                new ClientSettingsInvalidated
                {
                    Keys = validClientSetting
                        .Select(cls => ((ClientSettingsCalculated)cls).Key)
                        .ToArray(),
                    ProfileId = profilesToRecalculateClientSetting.Id
                },
                _EventObject);

            await _SagaService.AddEventsAsync(
                _BatchSagaId,
                validClientSettingTupleEvents.Append(validClientSettingTupleEvent),
                cancellationToken);
        }

        Logger.ExitMethod();
    }

    /// <summary>
    ///     The method creates a list of <see cref="MemberAdded" /> events, that are basically
    ///     <see cref="IUserProfileServiceEvent" />.
    /// </summary>
    /// <param name="assignments">The assignments for which the <see cref="MemberAdded" /> event have to be created.</param>
    /// <param name="container">The container for which the should be created.</param>
    /// <param name="databaseTransaction">The transaction that is used to call all database related methods.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellationToken requests.</param>
    /// <returns>A task that represent the asynchronous operation. It wraps a list of <see cref="IUserProfileServiceEvent" />s.</returns>
    internal async Task<IList<IUserProfileServiceEvent>> CreateMemberAssignmentsAsync(
        IEnumerable<Assignment> assignments,
        ObjectIdent container,
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        var memberAddedEvents = new List<IUserProfileServiceEvent>();

        foreach (Assignment assignment in assignments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IFirstLevelProjectionProfile firstLevelProfileMember =
                await Repository.GetProfileAsync(
                    assignment.ProfileId,
                    databaseTransaction,
                    cancellationToken);

            var memberAddedEvent = new MemberAdded
            {
                ParentId = container.Id,
                Member = _Mapper.Map<ResolvedMember>(firstLevelProfileMember),
                ParentType = assignment.TargetType.ToContainerType()
            };

            memberAddedEvent.Member.Conditions = _Mapper.Map<ResolvedRangeCondition[]>(assignment.Conditions);
            memberAddedEvents.Add(memberAddedEvent);

            await Repository.CreateProfileAssignmentAsync(
                assignment.TargetId,
                assignment.TargetType.ToContainerType(),
                assignment.ProfileId,
                assignment.Conditions,
                databaseTransaction,
                cancellationToken);
        }

        return Logger.ExitMethod(memberAddedEvents);
    }

    /// <summary>
    ///     Returns the extracted assignments in a special list which contained tuples that have assignments with their
    ///     correspondent <see cref="ObjectIdent" />.
    /// </summary>
    /// <param name="assignment">The assignments from that the list of assignment should extracted in special form.</param>
    /// <param name="databaseTransaction">The transaction that is used to call all database related methods.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellationToken requests.</param>
    /// <returns>A task that represent the asynchronous operation. It wraps a list of tuples.</returns>
    internal async Task<List<Tuple<Assignment, ObjectIdent>>> ExtractAssignment(
        Assignment assignment,
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IFirstLevelProjectionProfile profile = await Repository.GetProfileAsync(
            assignment.ProfileId,
            databaseTransaction,
            cancellationToken);

        IList<FirstLevelRelationProfile> children = await ExecuteSafelyAsync(
            () => Repository.GetAllChildrenAsync(
                profile.ToObjectIdent(),
                databaseTransaction,
                cancellationToken),
            new List<FirstLevelRelationProfile>());

        return Logger.ExitMethod(
            children.Select(child => new Tuple<Assignment, ObjectIdent>(assignment, child.Profile.ToObjectIdent()))
                .Append(new Tuple<Assignment, ObjectIdent>(assignment, profile.ToObjectIdent()))
                .ToList());
    }

    /// <summary>
    ///     Generate <see cref="TagsAdded" /> events out of the <see cref="FirstLevelProjectionParentsTreeDifferenceResult" />.
    /// </summary>
    /// <param name="firstLevelParentsResult">The result of of a method, what should be changed.</param>
    /// <returns>A list of <see cref="IUserProfileServiceEvent" /></returns>
    internal IEnumerable<IUserProfileServiceEvent> GenerateTagAddedEvents(
        FirstLevelProjectionParentsTreeDifferenceResult firstLevelParentsResult)
    {
        return firstLevelParentsResult.MissingRelations.Select(
                misRel => new TagsAdded
                {
                    Id = misRel.Parent.Id,
                    ObjectType = misRel.Parent.ContainerType
                        .ToObjectTypeResolved(),
                    Tags = misRel.ParentTags.Where(p => p.IsInheritable)
                        .ToArray()
                })
            .Append(
                new TagsAdded
                {
                    Id = firstLevelParentsResult.Profile.Id,
                    ObjectType = firstLevelParentsResult.Profile.ContainerType
                        .ToObjectTypeResolved(),
                    Tags = firstLevelParentsResult.ProfileTags.Where(p => p.IsInheritable)
                        .ToArray()
                })
            .Where(@event => @event.Tags.Any())
            .ToList();
    }

    /// <summary>
    ///     Creates out of a <see cref="FirstLevelProjectionTreeEdgeRelation" /> the suitable "was-assign-event".
    /// </summary>
    /// <param name="relationEdge"></param>
    /// <returns>A "was-assigned-event" that is basically a <see cref="IUserProfileServiceEvent" />.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="relationEdge" /> is null.</exception>
    /// <exception cref="NotSupportedException">If the container type could not mapped to a container type.</exception>
    internal IUserProfileServiceEvent GenerateWasAssignedToEvent(FirstLevelProjectionTreeEdgeRelation relationEdge)
    {
        Logger.EnterMethod();

        if (relationEdge == null)
        {
            throw new ArgumentNullException(nameof(relationEdge));
        }

        if (relationEdge.Child == null)
        {
            throw new ArgumentException("The child of the relationEdge is null.", nameof(relationEdge));
        }

        if (relationEdge.Parent == null)
        {
            throw new ArgumentException("The parent of the relationEdge is null.", nameof(relationEdge));
        }

        Logger.ExitMethod();

        return relationEdge.Parent.ContainerType switch
        {
            ContainerType.Group => _Mapper.Map<WasAssignedToGroup>(relationEdge),
            ContainerType.Organization => _Mapper.Map<WasAssignedToOrganization>(relationEdge),
            ContainerType.Function => _Mapper.Map<WasAssignedToFunction>(relationEdge),
            ContainerType.Role => _Mapper.Map<WasAssignedToRole>(relationEdge),
            _ => throw new NotSupportedException(
                $"The object type is could not be mapped to a suitable type. ObjectType: {relationEdge.Parent.ContainerType}")
        };
    }

    /// <summary>
    ///     The method creates a list of <see cref="MemberRemoved" /> events, that are basically
    ///     <see cref="IUserProfileServiceEvent" />.
    /// </summary>
    /// <param name="assignments">The assignments for which the <see cref="MemberRemoved" /> event have to be created.</param>
    /// <param name="container">The container for which the should be created.</param>
    /// <param name="databaseTransaction">The transaction that is used to call all database related methods.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellationToken requests.</param>
    /// <returns>A task that represent the asynchronous operation. It wraps a list of <see cref="IUserProfileServiceEvent" />s.</returns>
    internal async Task<IList<IUserProfileServiceEvent>> UnassignMemberFromContainerAsync(
        IEnumerable<Assignment> assignments,
        ObjectIdent container,
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken)
    {
        var memberAddedEvents = new List<IUserProfileServiceEvent>();

        foreach (Assignment assignment in assignments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IFirstLevelProjectionProfile firstLevelProfileMember =
                await Repository.GetProfileAsync(
                    assignment.ProfileId,
                    databaseTransaction,
                    cancellationToken);

            var memberAddedEvent = new MemberRemoved
            {
                MemberKind = _Mapper.Map<ResolvedProfileKind>(firstLevelProfileMember.Kind),
                MemberId = assignment.ProfileId,
                ParentType = assignment.TargetType.ToContainerType(),
                ParentId = assignment.TargetId,
                Conditions = _Mapper.Map<ResolvedRangeCondition[]>(assignment.Conditions)
            };

            memberAddedEvents.Add(memberAddedEvent);

            await Repository.DeleteProfileAssignmentAsync(
                assignment.TargetId,
                assignment.TargetType.ToContainerType(),
                assignment.ProfileId,
                assignment.Conditions,
                databaseTransaction,
                cancellationToken);
        }

        return memberAddedEvents;
    }

    /// <summary>
    ///     The method creates a list of <see cref="WasUnassignedFrom" /> events, that are basically
    ///     <see cref="IUserProfileServiceEvent" />.
    /// </summary>
    /// <param name="unassignments">The unassignments for which the <see cref="WasUnassignedFrom" /> event have to be created.</param>
    /// <param name="databaseTransaction">The transaction that is used to call all database related methods.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellationToken requests.</param>
    /// <returns>A task that represent the asynchronous operation. It wraps a list of <see cref="IUserProfileServiceEvent" />s.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal async Task UnassignmentsFromContainerAsync(
        IGrouping<string, Assignment> unassignments,
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken)
    {
        if (unassignments == null)
        {
            throw new ArgumentNullException(nameof(unassignments));
        }

        if (databaseTransaction == null)
        {
            throw new ArgumentNullException(nameof(databaseTransaction));
        }

        if (_EventObject == null)
        {
            throw new ArgumentNullException(nameof(_EventObject));
        }

        var parentToUnassign = new ObjectIdent(unassignments.Key, unassignments.First().TargetType);

        List<Tuple<Assignment, ObjectIdent>> unassignmentsObjectIdents =
            (await Task.WhenAll(
                unassignments.Select(ass => ExtractAssignment(ass, databaseTransaction, cancellationToken))))
            .SelectMany(t => t)
            .ToList();

        List<EventTuple> wasUnassignedFromTuple = unassignmentsObjectIdents.Select(
                objectTup => _Creator.CreateEvent(
                    objectTup.Item2,
                    new WasUnassignedFrom
                    {
                        ChildId = objectTup.Item1
                            .ProfileId,
                        ParentId = objectTup.Item1
                            .TargetId,
                        Conditions =
                            _Mapper
                                .Map<
                                    ResolvedRangeCondition
                                    []>(
                                    objectTup.Item1
                                        .Conditions),
                        ParentType =
                            objectTup.Item1
                                .TargetType
                                .ToContainerType()
                    },
                    _EventObject))
            .ToList();

        IList<IUserProfileServiceEvent> unassignmentsEvents = await UnassignMemberFromContainerAsync(
            unassignments,
            new ObjectIdent(unassignments.Key, unassignments.First().TargetType),
            databaseTransaction,
            cancellationToken);

        IEnumerable<EventTuple> unassignMemberFromTuples =
            _Creator.CreateEvents(parentToUnassign, unassignmentsEvents, _EventObject);

        await _SagaService.AddEventsAsync(
            _BatchSagaId,
            unassignMemberFromTuples.Union(wasUnassignedFromTuple),
            cancellationToken);

        // is the object an profile from whom to unassign? When yes client setting
        // maybe has to be computed new.
        if (unassignments.First().TargetType.IsProfileType())
        {
            IList<FirstLevelProjectionsClientSetting> clientSettings =
                await Repository.GetCalculatedClientSettingsAsync(
                    parentToUnassign.Id,
                    databaseTransaction,
                    cancellationToken);

            // calculate new client-setting for unassigned profiles, because the parent
            // has client settings inherited
            if (clientSettings.Any())
            {
                _ProfilesToRecalculateClientSettings.UnionWith(unassignmentsObjectIdents.Select(item => item.Item2));
            }
        }
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        ObjectAssignmentEvent eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction databaseTransaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (databaseTransaction == null)
        {
            throw new ArgumentNullException(nameof(databaseTransaction));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "@event: {event}, streamedEventHeader: {streamEvent}",
                LogHelpers.Arguments(eventObject.ToLogString(), streamEvent.ToLogString()));
        }

        // if no assignments are there, we have nothing to do.
        // The event seems to be invalid.
        if (!eventObject.Payload.Added.Any() && !eventObject.Payload.Removed.Any())
        {
            // Throw an exception, if saga worker validates the empty assignments in the event 
            return;
        }

        Logger.LogDebugMessage("Resolving profile types of targets to be added", LogHelpers.Arguments());

        await ResolveTargetTypesAsync(
            databaseTransaction,
            cancellationToken,
            eventObject.Payload.Added);

        // set the properties, because they are used in several methods
        _EventObject = eventObject;
        _BatchSagaId = await _SagaService.CreateBatchAsync(cancellationToken);

        if (eventObject.Payload.Added.Any())
        {
            Assignment[] assignments = eventObject.Payload.Added.GetAssignments(
                eventObject.Payload.Resource,
                eventObject.Payload.Type,
                Logger);

            // Set the UpdateAt-Property for all assignments + the date for the assignees
            List<ObjectIdent> assignmentsToBeUpdated =
                eventObject.Payload.Added.Select(ass => new ObjectIdent(ass.Id, ass.Type))
                    .Append(eventObject.Payload.Resource)
                    .ToList();

            await Repository.SetUpdatedAtAsync(
                eventObject.Timestamp,
                assignmentsToBeUpdated,
                databaseTransaction,
                cancellationToken);

            IEnumerable<IGrouping<string, Assignment>> targetIds = assignments.GroupBy(ass => ass.TargetId);

            foreach (IGrouping<string, Assignment> targetId in targetIds)
            {
                switch (targetId.First().TargetType)
                {
                    case ObjectType.Group:
                    case ObjectType.Organization:
                    case ObjectType.Profile:
                        await AssignmentsToProfileTypeAsync(
                            targetId,
                            databaseTransaction,
                            cancellationToken);

                        break;
                    case ObjectType.Function:
                        FirstLevelProjectionFunction function = await Repository.GetFunctionAsync(
                            targetId.Key,
                            databaseTransaction,
                            cancellationToken);

                        await AssignmentsToContainerTypeAsync(
                            targetId,
                            function,
                            databaseTransaction,
                            cancellationToken);

                        break;

                    case ObjectType.Role:
                        FirstLevelProjectionRole role = await Repository.GetRoleAsync(
                            targetId.Key,
                            databaseTransaction,
                            cancellationToken);

                        await AssignmentsToContainerTypeAsync(
                            targetId,
                            role,
                            databaseTransaction,
                            cancellationToken);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"The object type could not be mapped to supported type to assingn profiles. Object-type:{targetId.First().TargetType}.");
                }
            }
        }

        if (eventObject.Payload.Removed.Any())
        {
            Logger.LogDebugMessage("Resolving profile types of targets to be removed", LogHelpers.Arguments());

            await ResolveTargetTypesAsync(
                databaseTransaction,
                cancellationToken,
                eventObject.Payload.Removed);

            // Set the UpdateAt-Property for all assignments
            List<ObjectIdent> assignmentsToBeUpdated =
                eventObject.Payload.Removed.Select(ass => new ObjectIdent(ass.Id, ass.Type)).ToList();

            await Repository.SetUpdatedAtAsync(
                eventObject.Timestamp,
                assignmentsToBeUpdated,
                databaseTransaction,
                cancellationToken);

            Assignment[] assignments = eventObject.Payload.Removed.GetAssignments(
                eventObject.Payload.Resource,
                eventObject.Payload.Type,
                Logger);

            IEnumerable<IGrouping<string, Assignment>> targetIds = assignments.GroupBy(ass => ass.TargetId);

            foreach (IGrouping<string, Assignment> targetId in targetIds)
            {
                await UnassignmentsFromContainerAsync(
                    targetId,
                    databaseTransaction,
                    cancellationToken);
            }
        }

        // Do we have to recalculate any client settings?
        if (_ProfilesToRecalculateClientSettings.Any())
        {
            Logger.LogInfoMessage(
                "The client settings have to be recalculated for the following profiles: {profiles}",
                _ProfilesToRecalculateClientSettings.Select(x => x.Id).ToList().ToLogString().AsArgumentList());

            await CreateAllClientSettingRecalculationAsync(databaseTransaction, cancellationToken);
        }

        await _SagaService.ExecuteBatchAsync(_BatchSagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
