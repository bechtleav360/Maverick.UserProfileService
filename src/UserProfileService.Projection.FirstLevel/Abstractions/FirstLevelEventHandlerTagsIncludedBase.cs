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
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using ObjectTypeResolved = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType;
using TagResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.Tag;
using TagAssignmentResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.TagAssignment;
using RangeConditionResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;
using GroupResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Group;
using OrganizationResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using MemberAddedResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.MemberAdded;
using MemberResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Projection.FirstLevel.Abstractions;

/// <summary>
///     Base class is used to create events, that also add
///     tags to the created entity. The class has an method that is used
///     to set tags to profiles, functions and roles.It also includes a method
///     to add members to a container. The method is needed by the created group and create organization
///     handler.
/// </summary>
/// <typeparam name="TEventType">The type has the type <see cref="IUserProfileServiceEvent" />.</typeparam>
public abstract class FirstLevelEventHandlerTagsIncludedBase<TEventType> : FirstLevelEventHandlerBase<TEventType>
    where TEventType : class, IUserProfileServiceEvent, IDomainEvent
{
    /// <summary>
    ///     Used to create event tuples for the first level projection.
    /// </summary>
    protected readonly IFirstLevelEventTupleCreator Creator;
    /// <summary>
    ///     The Mapper used to map several existing events in new event with the right order.
    /// </summary>
    protected readonly IMapper Mapper;
    /// <summary>
    ///     The saga service that generates the streams for the entities with the given objects.
    /// </summary>
    protected readonly ISagaService SagaService;

    /// <summary>
    ///     The constructor that is used to create an instance of <see cref="FirstLevelEventHandlerTagsIncludedBase{TEventType}" />.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> to be used.</param>
    /// <param name="repository">The repository that is used to perform methods on the first level projection.</param>
    /// <param name="sagaService">The saga service that generates the streams for the entities with the given objects.</param>
    /// <param name="mapper">The mapper is used to map objects from one type, to another.</param>
    /// <param name="creator">The creator creates the event in <see cref="EventTuple" />.</param>
    protected FirstLevelEventHandlerTagsIncludedBase(
        ILogger logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IMapper mapper,
        IFirstLevelEventTupleCreator creator) : base(
        logger,
        repository)
    {
        SagaService = sagaService;
        Mapper = mapper;
        Creator = creator;
    }

    /// <summary>
    ///     The method is used to create a right <see cref="WasAssignedToBase{TContainer}" />
    ///     related to the container type.
    /// </summary>
    /// <param name="assignToProfile">The parent the child was assigned to.</param>
    /// <param name="containerType">The container type of the parent.</param>
    /// <param name="conditions">A list of conditions the new assignment should be valid.</param>
    /// <param name="childId">The id of the child that should be assigned to the container.</param>
    /// <returns>A task that represent the asynchronous  operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the given container type could not be matched to a event.</exception>
    private IUserProfileServiceEvent ChooseWasAssignToEvent(
        IFirstLevelProjectionProfile assignToProfile,
        ContainerType containerType,
        IList<RangeCondition> conditions,
        string childId)
    {
        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The assignToProfile: {assignToProfile}, containerType: {containerType}, member: {member}, childId: {childId}",
                LogHelpers.Arguments(
                    assignToProfile.ToLogString(),
                    containerType.ToLogString(),
                    conditions.ToLogString(),
                    childId.ToLogString()));
        }

        return containerType switch
        {
            ContainerType.Group => new WasAssignedToGroup
            {
                Conditions =
                    conditions != null
                        ? Mapper.Map<RangeConditionResolved[]>(conditions)
                        : new[] { new RangeConditionResolved() },
                Target = Mapper.Map<GroupResolved>((FirstLevelProjectionGroup)assignToProfile),
                ProfileId = childId
            },
            ContainerType.Organization => new WasAssignedToOrganization
            {
                Conditions =
                    conditions != null
                        ? Mapper.Map<RangeConditionResolved[]>(conditions)
                        : new[] { new RangeConditionResolved() },
                Target = Mapper.Map<OrganizationResolved>((FirstLevelProjectionOrganization)assignToProfile),
                ProfileId = childId
            },
            _ => throw new ArgumentOutOfRangeException(
                $"The container type could not be mapped to a specific event. Incoming container type: {containerType}.")
        };
    }

    /// <summary>
    ///     The method is used to assign members to a container.
    ///     This method will be called for groups and organizations.
    /// </summary>
    /// <param name="transaction">The transaction that is used to call all database related methods.</param>
    /// <param name="containerToAssignTo">The profile to which the member have to be assigned.</param>
    /// <param name="membersToAdd">The member that should assigned to a container. </param>
    /// <param name="batchId">The id of the batch that the saga worker needs to created a batch.</param>
    /// <param name="eventObject">The original event that is needed for extra information.</param>
    /// <param name="altRelatedEntityId">Is used to set a debug information.</param>
    /// <param name="tagsAssignments">Tags assignment that should be assigned to the member, that are assigned to the profile.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task that represent the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">If the given assigned container is a user. </exception>
    protected async Task CreateProfileAssignmentsAsync(
        IDatabaseTransaction transaction,
        ObjectIdent containerToAssignTo,
        ConditionObjectIdent[] membersToAdd,
        Guid batchId,
        TEventType eventObject,
        string altRelatedEntityId,
        List<TagAssignment> tagsAssignments,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (containerToAssignTo == null || string.IsNullOrWhiteSpace(containerToAssignTo.Id))
        {
            throw new ArgumentNullException(nameof(containerToAssignTo));
        }

        if (membersToAdd == null)
        {
            throw new ArgumentNullException(nameof(membersToAdd));
        }

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(altRelatedEntityId))
        {
            throw new ArgumentNullException(nameof(altRelatedEntityId));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "BatchId:{batchId}, MemberAssignments: {tagAssignments}, eventObject: {eventObject}, containerTagsAssignedTo: {containerTagsAssignedTo} ",
                LogHelpers.Arguments(
                    batchId.ToLogString(),
                    membersToAdd.ToLogString(),
                    eventObject.ToLogString(),
                    containerToAssignTo.ToLogString()));
        }

        if (membersToAdd.Any())
        {
            Logger.LogInfoMessage(
                "There are members to assign, the member will be assigned to the entity: {entity}",
                containerToAssignTo.ToLogString().AsArgumentList());

            IFirstLevelProjectionProfile profileToAssignTo = await Repository.GetProfileAsync(
                containerToAssignTo.Id,
                transaction,
                cancellationToken);

            if (!(profileToAssignTo is IFirstLevelProjectionContainer assignToProfileContainer))
            {
                throw new InvalidOperationException("The assign type can not be a user.");
            }

            Logger.LogDebugMessage(
                "Members that should be assigned to the container: {assignMembers}.",
                membersToAdd.ToLogString().AsArgumentList());

            foreach (ConditionObjectIdent memberToAdd in membersToAdd)
            {
                // get the member to add
                IFirstLevelProjectionProfile memberProfileToAdd = await Repository.GetProfileAsync(
                    memberToAdd.Id,
                    transaction,
                    cancellationToken);

                await Repository.SetUpdatedAtAsync(
                    DateTime.Now,
                    new List<ObjectIdent>
                    {
                        memberProfileToAdd.ToObjectIdent()
                    },
                    transaction,
                    cancellationToken);

                Logger.LogTraceMessage(
                    "The child that should be assigned: {childInformation}",
                    memberProfileToAdd.ToObjectIdent().ToLogString().AsArgumentList());

                IList<FirstLevelRelationProfile> childElementsOfMember = await ExecuteSafelyAsync(
                    () => Repository.GetAllChildrenAsync(
                        new ObjectIdent(memberToAdd.Id, memberToAdd.Type),
                        transaction,
                        cancellationToken),
                    new List<FirstLevelRelationProfile>());

                List<IFirstLevelProjectionProfile> children =
                    childElementsOfMember.Select(pr => pr.Profile).ToList();

                // if tags are there they should be inherited to the member that are assigned
                // to the group or organization
                children = children.Append(memberProfileToAdd).ToList();

                await AddInheritableTagsToProfiles(
                    transaction,
                    containerToAssignTo,
                    batchId,
                    eventObject,
                    children,
                    tagsAssignments,
                    cancellationToken);

                Logger.LogInfoMessage(
                    "The child has other children that should be informed, that they were assigned to {container}: {childInformation}",
                    LogHelpers.Arguments(
                        containerToAssignTo.Type == ObjectType.Group ? "group" : "organization",
                        childElementsOfMember.Select(p => p.Profile.ToObjectIdent()).ToList()));

                Logger.LogInfoMessage(
                    "The children property createdAt will be updated due to an assignment.",
                    LogHelpers.Arguments());

                Logger.LogInfoMessage(
                    "An assignment will be created for the first level repository. From: {parent} , To: {child}.",
                    LogHelpers.Arguments(
                        assignToProfileContainer.ToObjectIdent().ToLogString(),
                        memberProfileToAdd.ToObjectIdent().ToLogString()));

                await Repository.CreateProfileAssignmentAsync(
                    assignToProfileContainer.Id,
                    assignToProfileContainer.ContainerType,
                    memberToAdd.Id,
                    memberToAdd.Conditions,
                    transaction,
                    cancellationToken);

                List<EventTuple> memberWereAssignToContainer = children
                    .Select(
                        childProfile =>
                            Creator.CreateEvent(
                                childProfile.ToObjectIdent(),
                                ChooseWasAssignToEvent(
                                    profileToAssignTo,
                                    assignToProfileContainer
                                        .ContainerType,
                                    memberToAdd.Conditions,
                                    memberToAdd.Id),
                                eventObject))
                    .ToList();

                Logger.LogInfoMessage(
                    "For the assignments from:{parent} , to: {child} has been created {countTuple} events.",
                    LogHelpers.Arguments(
                        assignToProfileContainer.ToObjectIdent().ToLogString(),
                        memberProfileToAdd.ToObjectIdent().ToLogString(),
                        memberWereAssignToContainer.Count));

                if (Logger.IsEnabledForTrace())
                {
                    Logger.LogTraceMessage(
                        "For the parentId: {parentId} and the childId: {childId} and their children following '{wasAssignedEvent}' have been created: {eventTuple}",
                        LogHelpers.Arguments(
                            containerToAssignTo.ToLogString(),
                            memberProfileToAdd.ToObjectIdent().ToLogString(),
                            assignToProfileContainer.ContainerType == ContainerType.Group
                                ? nameof(WasAssignedToGroup)
                                : nameof(WasAssignedToOrganization),
                            memberWereAssignToContainer.ToLogString()));
                }

                // only the member added to the group or organization
                EventTuple memberAddedToContainer = Creator.CreateEvent(
                    assignToProfileContainer.ToObjectIdent(),
                    new MemberAddedResolved
                    {
                        Member = Mapper.Map<MemberResolved>(memberProfileToAdd),
                        ParentId = assignToProfileContainer.Id,
                        ParentType = assignToProfileContainer.ContainerType
                    },
                    eventObject);

                if (Logger.IsEnabledForTrace())
                {
                    Logger.LogTraceMessage(
                        "For the parentId: {parentId} and the childId: {childId}  has been created following memberAddedEvent: {eventTuple}",
                        LogHelpers.Arguments(
                            containerToAssignTo.ToLogString(),
                            memberProfileToAdd.ToObjectIdent().ToLogString(),
                            memberAddedToContainer.ToLogString()));
                }

                await SagaService.AddEventsAsync(
                    batchId,
                    memberWereAssignToContainer.Append(memberAddedToContainer),
                    cancellationToken);
            }
        }

        Logger.ExitMethod();
    }

    /// <summary>
    ///     This method is used to generate tags assignment to a given entity. Mostly
    ///     users, groups, organizations, roles and functions.
    /// </summary>
    /// <param name="transaction">The transaction that is used to call all database related methods.</param>
    /// <param name="batchId">The id of the batch that the saga worker needs to created a batch.</param>
    /// <param name="tagAssignments">The tag assignments that have to be created.</param>
    /// <param name="tagAddedRepository">The Repo</param>
    /// <param name="eventObject">The original object that is needed for some extra information.</param>
    /// <param name="containerTagsAssignedTo">The object ident the tags are added to.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns> A task that represent the asynchronous  operation.</returns>
    protected async Task CreateTagsAddedEvent(
        IDatabaseTransaction transaction,
        Guid batchId,
        List<TagAssignment> tagAssignments,
        Func<IFirstLevelProjectionRepository, FirstLevelProjectionTagAssignment, Task>
            tagAddedRepository,
        TEventType eventObject,
        ObjectIdent containerTagsAssignedTo,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (tagAssignments == null)
        {
            throw new ArgumentNullException(nameof(tagAssignments));
        }

        if (tagAddedRepository == null)
        {
            throw new ArgumentNullException(nameof(tagAddedRepository));
        }

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (containerTagsAssignedTo == null
            || string.IsNullOrWhiteSpace(containerTagsAssignedTo.Id))
        {
            throw new ArgumentNullException(nameof(containerTagsAssignedTo));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "BatchId:{batchId}, tagAssignments: {tagAssignments}, eventObject: {eventObject}, containerTagsAssignedTo: {containerTagsAssignedTo} ",
                LogHelpers.Arguments(
                    batchId.ToLogString(),
                    tagAssignments.ToLogString(),
                    eventObject.ToLogString(),
                    containerTagsAssignedTo.ToLogString()));
        }

        if (tagAssignments.Any())
        {
            Logger.LogInfoMessage(
                "Tags assignments are not empty, so tags will be added to: {entity}",
                containerTagsAssignedTo.ToLogString().AsArgumentList());

            var tagAssignmentsEvents =
                new List<TagAssignmentResolved>();

            foreach (TagAssignment tagAssignment in tagAssignments)
            {
                FirstLevelProjectionTag tag = null;

                await ExecuteSafelyAsync(
                    async () =>
                    {
                        Logger.LogInfoMessage(
                            "Trying to find a tag with the id: {tagId}",
                            tagAssignment.TagId.AsArgumentList());

                        tag = await Repository.GetTagAsync(
                            tagAssignment.TagId,
                            transaction,
                            cancellationToken);

                        Logger.LogInfoMessage(
                            "The tag with the id: {tagId} could be found.",
                            tagAssignment.TagId.AsArgumentList());
                    },
                    async () =>
                    {
                        Logger.LogWarnMessage(
                            "The tag with the id :{tagId} could not be found. The even handler will be canceled.",
                            tagAssignment.TagId.AsArgumentList());

                        await SagaService.AbortBatchAsync(batchId, cancellationToken);
                    });

                tagAssignmentsEvents.Add(
                    new TagAssignmentResolved
                    {
                        TagDetails = Mapper.Map<TagResolved>(tag),
                        IsInheritable = tagAssignment.IsInheritable
                    });

                var tagAssignmentProjection = new FirstLevelProjectionTagAssignment
                {
                    TagId = tag.Id,
                    IsInheritable = tagAssignment.IsInheritable
                };

                await tagAddedRepository.Invoke(Repository, tagAssignmentProjection);
            }

            if (tagAssignmentsEvents.Any())
            {
                var tagAddEvent = new TagsAdded
                {
                    Id = containerTagsAssignedTo.Id,
                    ObjectType = Mapper.Map<ObjectTypeResolved>(containerTagsAssignedTo.Type),
                    Tags = Mapper
                        .Map<TagAssignmentResolved
                            []>(tagAssignmentsEvents)
                };

                Logger.LogInfoMessage(
                    "A tags added event has been created with the tagEvents: {tagAddEvents} for the entity: {entity}.",
                    LogHelpers.Arguments(tagAddEvent.ToLogString(), containerTagsAssignedTo.ToLogString()));

                await SagaService.AddEventsAsync(
                    batchId,
                    Creator.CreateEvents(
                            new ObjectIdent(
                                containerTagsAssignedTo.Id,
                                containerTagsAssignedTo.Type),
                            new List<IUserProfileServiceEvent>
                            {
                                tagAddEvent
                            },
                            eventObject)
                        .ToArray(),
                    cancellationToken);
            }
        }

        Logger.ExitMethod();
    }

    /// <summary>
    ///     Recalculated the clientSettings and creates a list of <see cref="EventTuple" />. It also includes the
    ///     <see cref="ClientSettingsInvalidated" />
    ///     event, so that only the new recalculated clientSettings key are valid.
    /// </summary>
    /// <param name="profile">The profile as <see cref="ObjectIdent" /> where the client setting has to be recalculated. </param>
    /// <param name="transaction">The transaction that is used to call all database related methods.</param>
    /// <param name="eventObject">The original object that is needed for some extra information.</param>
    /// <param name="clientSettingsKey">The client settings key from the client settings itself.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns> A task that represent the asynchronous  operation that wraps a list of <see cref="EventTuple" /> </returns>
    protected async Task<IEnumerable<EventTuple>> RecalculateClientSettingsAsync(
        ObjectIdent profile,
        IDatabaseTransaction transaction,
        TEventType eventObject,
        string clientSettingsKey,
        CancellationToken cancellationToken)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(clientSettingsKey))
        {
            throw new ArgumentException(nameof(clientSettingsKey));
        }

        IList<FirstLevelProjectionsClientSetting> clientSettingFromProfile =
            await Repository.GetCalculatedClientSettingsAsync(
                profile.Id,
                transaction,
                cancellationToken);

        List<IUserProfileServiceEvent> newCalculatedClientSettings =
            clientSettingFromProfile.GetClientSettingsCalculatedEvents(profile.Id);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Recalculated for id: {profileId}, the clientSettings: {clientSettings}",
                LogHelpers.Arguments(
                    profile.Id.ToLogString(),
                    newCalculatedClientSettings.ToLogString()));
        }

        EventTuple validClientSettingTupleEvent = Creator.CreateEvent(
            profile,
            new ClientSettingsInvalidated
            {
                Keys = newCalculatedClientSettings
                    .Select(cls => ((ClientSettingsCalculated)cls).Key)
                    .ToArray(),
                ProfileId = profile.Id
            },
            eventObject);

        // only the new calculated key has to be send
        newCalculatedClientSettings = newCalculatedClientSettings
            .Where(
                cls => ((ClientSettingsCalculated)cls).Key.Equals(
                    clientSettingsKey,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        Logger.LogInfoMessage(
            "Current valid client setting keys: {clientSettingKeys} for id {profileId}",
            LogHelpers.Arguments(
                newCalculatedClientSettings
                    .Select(cls => ((ClientSettingsCalculated)cls).Key)
                    .ToArray()
                    .ToLogString(),
                profile.Id.ToLogString()));

        IEnumerable<EventTuple> newCalculatedClientSettingsTuple = Creator.CreateEvents(
            profile,
            newCalculatedClientSettings,
            eventObject);

        IEnumerable<EventTuple> calculatedClientSettingsTuple = newCalculatedClientSettingsTuple as EventTuple[]
            ?? newCalculatedClientSettingsTuple.ToArray();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Created event for the second level projection: {tupleEvents}",
                calculatedClientSettingsTuple.Append(validClientSettingTupleEvent)
                    .ToLogString()
                    .AsArgumentList());
        }

        return calculatedClientSettingsTuple.Append(validClientSettingTupleEvent);
    }

    /// <summary>
    ///     Recalculates the client settings of the children
    ///     from a profile.
    /// </summary>
    /// <param name="profileCheckForChildren">
    ///     The profile that should be checked for children. And when children are there the
    ///     client settings must be recalculated.
    /// </param>
    /// <param name="batchSagaId">The id of the batch that the saga worker needs to created a batch.</param>
    /// <param name="transaction">The transaction that is used to call all database related methods.</param>
    /// <param name="eventObject">The original object that is needed for some extra information.</param>
    /// <param name="clientSettingsKey">The client settings key from the client settings itself.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns> A task that represent the asynchronous operation.</returns>
    protected async Task RecalculatedClientSettingsForChildren(
        ObjectIdent profileCheckForChildren,
        IDatabaseTransaction transaction,
        Guid batchSagaId,
        TEventType eventObject,
        string clientSettingsKey,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        // Only if the profile is a organization or a group, client settings will be 
        // inherited.
        if (profileCheckForChildren.Type == ObjectType.Organization
            || profileCheckForChildren.Type == ObjectType.Group)
        {
            Logger.LogInfoMessage(
                "The profile is {profile}. It has to be checked, if the client settings has to be recalculated for the children. ",
                LogHelpers.Arguments(
                    profileCheckForChildren.Type == ObjectType.Organization ? "an organization" : "a group"));

            IList<FirstLevelRelationProfile> children = await ExecuteSafelyAsync(
                async () => await Repository.GetAllChildrenAsync(
                    profileCheckForChildren,
                    transaction,
                    cancellationToken),
                new List<FirstLevelRelationProfile>());

            foreach (IFirstLevelProjectionProfile child in children.Select(pr => pr.Profile).ToList())
            {
                IEnumerable<EventTuple> clientSettingChildTuple = await RecalculateClientSettingsAsync(
                    child.ToObjectIdent(),
                    transaction,
                    eventObject,
                    clientSettingsKey,
                    cancellationToken);

                await SagaService.AddEventsAsync(batchSagaId, clientSettingChildTuple, cancellationToken);
            }
        }
    }

    /// <summary>
    ///     Sets a new client setting to the given profile, recalculated the clientSettings and
    ///     creates a list of <see cref="EventTuple" />. It also includes the <see cref="ClientSettingsInvalidated" />
    ///     event, so that only the new recalculated clientSettings key are valid.
    /// </summary>
    /// <param name="profile">The profile as <see cref="ObjectIdent" /> where the client setting has to be recalculated. </param>
    /// <param name="transaction">The transaction that is used to call all database related methods.</param>
    /// <param name="eventObject">The original object that is needed for some extra information.</param>
    /// <param name="clientSettingKey">The client settings key tha should be set.</param>
    /// <param name="clientSettingValue">The client settings value that should be set.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns> A task that represent the asynchronous  operation that wraps a list of <see cref="EventTuple" /> </returns>
    protected async Task<IEnumerable<EventTuple>> SetClientSettingsAndRecalculateTupleAsync(
        ObjectIdent profile,
        IDatabaseTransaction transaction,
        TEventType eventObject,
        string clientSettingKey,
        string clientSettingValue,
        CancellationToken cancellationToken)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(clientSettingKey))
        {
            throw new ArgumentException(nameof(clientSettingKey));
        }

        if (string.IsNullOrWhiteSpace(clientSettingValue))
        {
            throw new ArgumentException(nameof(clientSettingValue));
        }

        await Repository.SetClientSettingsAsync(
            profile.Id,
            clientSettingValue,
            clientSettingKey,
            transaction,
            cancellationToken);

        IEnumerable<EventTuple> newEvents = await RecalculateClientSettingsAsync(
            profile,
            transaction,
            eventObject,
            clientSettingKey,
            cancellationToken);

        return Enumerable.Empty<EventTuple>()
            .Append(
                Creator.CreateEvent(
                    profile,
                    new ProfileClientSettingsSet
                    {
                        Key = clientSettingKey,
                        ProfileId = profile.Id,
                        ClientSettings = clientSettingValue
                    },
                    eventObject))
            .Concat(newEvents);
    }

    /// <summary>
    ///     Unsets a new client settings key to the given profile, recalculated the clientSettings and
    ///     creates a list of <see cref="EventTuple" />. It also includes the <see cref="ClientSettingsInvalidated" />
    ///     event, so that only the new recalculated clientSettings key are valid.
    /// </summary>
    /// <param name="profile">The profile as <see cref="ObjectIdent" /> where the client setting has to be recalculated. </param>
    /// <param name="transaction">The transaction that is used to call all database related methods.</param>
    /// <param name="eventObject">The original object that is needed for some extra information.</param>
    /// <param name="clientSettingKey">The client settings key tha should be unset.</param>
    /// <param name="altRelatedEntityId">Is used to set a debug information.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns> A task that represent the asynchronous operation that wraps a list of <see cref="EventTuple" /> </returns>
    protected async Task<IEnumerable<EventTuple>> UnsetClientSettingsAndRecalculateTupleAsync(
        ObjectIdent profile,
        IDatabaseTransaction transaction,
        TEventType eventObject,
        string clientSettingKey,
        string altRelatedEntityId,
        CancellationToken cancellationToken)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(clientSettingKey))
        {
            throw new ArgumentException(nameof(clientSettingKey));
        }

        if (string.IsNullOrWhiteSpace(altRelatedEntityId))
        {
            throw new ArgumentException(nameof(altRelatedEntityId));
        }

        await Repository.UnsetClientSettingsAsync(
            profile.Id,
            clientSettingKey,
            transaction,
            cancellationToken);

        IEnumerable<EventTuple> newEvents = await RecalculateClientSettingsAsync(
            profile,
            transaction,
            eventObject,
            clientSettingKey,
            cancellationToken);

        return Enumerable.Empty<EventTuple>()
            .Append(
                Creator.CreateEvent(
                    profile,
                    new ProfileClientSettingsUnset
                    {
                        Key = clientSettingKey,
                        ProfileId = profile.Id
                    },
                    eventObject))
            .Concat(newEvents);
    }

    /// <summary>
    ///     This method is used to filter the list for inheritable tags and add them to the children elements
    ///     (see<paramref name="profiles"></paramref>).
    /// </summary>
    /// <param name="transaction">The transaction that is used to call all database related methods.</param>
    /// <param name="containerTagsAssignedTo">Id of the profile that was assigned the tags originally.</param>
    /// <param name="batchId">The id of the batch that the saga worker needs to created a batch.</param>
    /// <param name="eventObject">The original event that is needed for extra information.</param>
    /// <param name="profiles">Profiles to which the inheritable tags are added.</param>
    /// <param name="tagAssignments">The tags that are added to the profiles. Will be filtered by inheritable.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task that represent the asynchronous operation.</returns>
    public async Task AddInheritableTagsToProfiles(
        IDatabaseTransaction transaction,
        ObjectIdent containerTagsAssignedTo,
        Guid batchId,
        TEventType eventObject,
        ICollection<IFirstLevelProjectionProfile> profiles,
        ICollection<TagAssignment> tagAssignments,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        List<TagAssignment> inheritableTags = tagAssignments
            .Where(t => t.IsInheritable)
            .ToList();

        if (!inheritableTags.Any())
        {
            Logger.LogDebugMessage(
                "No inheritable tags to add to child elements of member with id {id} and type {type}.",
                LogHelpers.Arguments(containerTagsAssignedTo.Id, containerTagsAssignedTo.Type));

            return;
        }

        Logger.LogDebugMessage(
            "Found {count} inheritable to add to child elements of member with id {id} and type {type}.",
            LogHelpers.Arguments(inheritableTags.Count, containerTagsAssignedTo.Id, containerTagsAssignedTo.Type));

        foreach (IFirstLevelProjectionProfile profileToAssignTags in profiles)

        {
            Logger.LogDebugMessage(
                "The Member: {member} will be assigned to the following tags: {tagAssignments}",
                LogHelpers.Arguments(
                    profileToAssignTags.ToObjectIdent().ToLogString(),
                    inheritableTags.ToLogString()));

            await CreateTagsAddedEvent(
                transaction,
                batchId,
                inheritableTags,
                async (repo, tag) =>
                    await repo.AddTagToProfileAsync(
                        tag,
                        profileToAssignTags.Id,
                        transaction,
                        cancellationToken),
                eventObject,
                profileToAssignTags.ToObjectIdent(),
                cancellationToken);
        }

        Logger.ExitMethod();
    }
}
