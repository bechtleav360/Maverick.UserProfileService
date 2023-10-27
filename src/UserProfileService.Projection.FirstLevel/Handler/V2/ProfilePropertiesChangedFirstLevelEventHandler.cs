using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using PropertiesChangedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.PropertiesChanged;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="ProfilePropertiesChangedEvent" />.
/// </summary>
internal class ProfilePropertiesChangedFirstLevelEventHandler : FirstLevelEventHandlerBase<ProfilePropertiesChangedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly IMapper _Mapper;
    private readonly IPropertiesChangedRelatedEventsResolver _PropertiesChangedRelatedEventsResolver;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="ProfilePropertiesChangedFirstLevelEventHandler" />.
    /// </summary>
    /// <param name="logger">
    ///     The logger factory that is used to create a logger. The logger logs message for debugging
    ///     and control reasons.
    /// </param>
    /// <param name="repository">
    ///     The read service is used to read from the internal query storage to get all information to
    ///     generate all needed stream events.
    /// </param>
    /// <param name="sagaService">
    ///     The saga service is used to write all created <see cref="IUserProfileServiceEvent" /> to the
    ///     write stream.
    /// </param>
    /// <param name="creator">The creator is used to create <inheritdoc cref="EventTuple" /> from the given parameter.</param>
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
    /// <param name="propertiesChangedResolver"></param>
    public ProfilePropertiesChangedFirstLevelEventHandler(
        ILogger<ProfilePropertiesChangedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper,
        IPropertiesChangedRelatedEventsResolver propertiesChangedResolver) :
        base(logger, repository)
    {
        _SagaService = sagaService;
        _Creator = creator;
        _Mapper = mapper;
        _PropertiesChangedRelatedEventsResolver = propertiesChangedResolver;
    }

    protected override async Task HandleInternalAsync(
        ProfilePropertiesChangedEvent eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "@event: {event}.",
                LogHelpers.Arguments(eventObject.ToLogString()));
        }

        IFirstLevelProjectionProfile profile = await Repository.GetProfileAsync(
            eventObject.Payload.Id,
            transaction,
            cancellationToken);

        var changedProfile = profile.ToObjectIdent();

        // own properties are changed
        var propertiesChangedEventDirectMember = _Mapper.Map<PropertiesChangedResolvedEvent>(eventObject);
        propertiesChangedEventDirectMember.RelatedContext = PropertiesChangedContext.Self;

        Guid batchSagaId = await _SagaService.CreateBatchAsync(
            cancellationToken,
            _Creator.CreateEvents(
                    changedProfile,
                    new List<IUserProfileServiceEvent>
                    {
                        propertiesChangedEventDirectMember
                    },
                    eventObject)
                .ToArray());

        IList<FirstLevelRelationProfile> relatedProfiles = await ExecuteSafelyAsync(
            async () =>
                await Repository.GetAllChildrenAsync(profile.ToObjectIdent(), transaction, cancellationToken),
            new List<FirstLevelRelationProfile>());

        IList<IFirstLevelProjectionContainer> parents = await ExecuteSafelyAsync(
            async () => await Repository.GetParentsAsync(
                profile.Id,
                transaction,
                cancellationToken),
            new List<IFirstLevelProjectionContainer>());

        if (parents.Any() && eventObject.Payload.MembersHasToBeUpdated())
        {
            Logger.LogInfoMessage(
                "Found {parentsCount} related parents",
                parents.Count.ToLogString().AsArgumentList());

            List<EventTuple> eventTupleParentsEvents = parents.Select(
                    container =>
                        _PropertiesChangedRelatedEventsResolver
                            .CreateRelatedMemberEvent(
                                changedProfile,
                                container.ToObjectIdent(),
                                PropertiesChangedRelation.MemberOf,
                                eventObject))
                .ToList();

            await _SagaService.AddEventsAsync(batchSagaId, eventTupleParentsEvents, cancellationToken);
        }

        if (profile.Kind == ProfileKind.Organization)
        {
            relatedProfiles = relatedProfiles.Where(rel => rel.Profile.Kind == ProfileKind.Organization)
                .ToList();
        }

        if (relatedProfiles.Any() && eventObject.Payload.MembersHasToBeUpdated())
        {
            Logger.LogDebugMessage(
                "Found {relatedProfilesCount} related profiles.",
                LogHelpers.Arguments(relatedProfiles.Count));

            List<EventTuple> eventTupleChildEvents = relatedProfiles.Select(
                    relatedProfile =>
                        relatedProfile.Relation
                        == FirstLevelMemberRelation
                            .IndirectMember
                            ? _PropertiesChangedRelatedEventsResolver
                                .CreateRelatedMemberEvent(
                                    changedProfile,
                                    relatedProfile.Profile
                                        .ToObjectIdent(),
                                    PropertiesChangedRelation
                                        .IndirectMember,
                                    eventObject)
                            : _PropertiesChangedRelatedEventsResolver
                                .CreateRelatedMemberEvent(
                                    changedProfile,
                                    relatedProfile.Profile
                                        .ToObjectIdent(),
                                    PropertiesChangedRelation
                                        .Member,
                                    eventObject))
                .ToList();

            await _SagaService.AddEventsAsync(batchSagaId, eventTupleChildEvents, cancellationToken);
        }

        if (profile.Kind == ProfileKind.Organization)
        {
            var eventTuplesFunctions = new List<EventTuple>();

            ICollection<FirstLevelProjectionFunction> firstLevelFunctions = await ExecuteSafelyAsync(
                () =>
                {
                    Task<ICollection<FirstLevelProjectionFunction>> result =
                        Repository.GetFunctionsOfOrganizationAsync(
                            profile.Id,
                            transaction,
                            cancellationToken);

                    return result;
                },
                new List<FirstLevelProjectionFunction>());

            foreach (FirstLevelProjectionFunction function in firstLevelFunctions)
            {
                FunctionPropertiesChangedEvent functionPropertiesChangedEvent =
                    eventObject.Payload.CreateFunctionEventRelatedToPropertiesPayload(eventObject);

                eventTuplesFunctions.AddRange(
                    await _PropertiesChangedRelatedEventsResolver.CreateFunctionPropertiesChangedEventsAsync(
                        function.Id,
                        functionPropertiesChangedEvent,
                        PropertiesChangedContext.Organization,
                        transaction,
                        cancellationToken));
            }

            if (eventTuplesFunctions.Any())
            {
                await _SagaService.AddEventsAsync(batchSagaId, eventTuplesFunctions, cancellationToken);
            }
        }

        profile.UpdateProfileWithPayload(eventObject.Payload, eventObject, Logger);

        await Repository.UpdateProfileAsync(profile, transaction, cancellationToken);
        await _SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        Logger.LogInfoMessage(
            "Finished handling event to update the profile with id {id} and type {type}",
            LogHelpers.Arguments(profile.Id, eventObject.ProfileKind.ToLogString()));

        Logger.ExitMethod();
    }
}
