using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using AggregateEnums = Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="ProfileTagsRemovedEvent" />.
/// </summary>
internal class ProfileTagsRemovedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<ProfileTagsRemovedEvent>
{
    /// <summary>
    ///     Creates an instance of the object <see cref="ProfileTagsRemovedFirstLevelEventHandler" />.
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
    public ProfileTagsRemovedFirstLevelEventHandler(
        ILogger<ProfileTagsRemovedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(
        logger,
        repository,
        sagaService,
        mapper,
        creator)
    {
    }

    private async Task CreateTagsRemovedEventAsync(
        Guid batchId,
        IDomainEvent eventObject,
        ObjectIdent profileToRemoveTagsFrom,
        IList<FirstLevelProjectionTagAssignment> tagAssignmentsToRemove,
        string altRelatedEntityId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        var tagsRemovedEvent = new TagsRemoved
        {
            Id = profileToRemoveTagsFrom.Id,
            Tags = tagAssignmentsToRemove.Select(t => t.TagId).ToArray(),
            ObjectType = Mapper.Map<AggregateEnums.ObjectType>(profileToRemoveTagsFrom.Type)
        };

        Logger.LogInfoMessage(
            "A tags added event has been created with the tagEvents: {tagAddEvents} for the entity: {entity}.",
            LogHelpers.Arguments(tagsRemovedEvent.ToLogString(), profileToRemoveTagsFrom.ToLogString()));

        await SagaService.AddEventsAsync(
            batchId,
            Creator.CreateEvents(
                    profileToRemoveTagsFrom,
                    new List<IUserProfileServiceEvent>
                    {
                        tagsRemovedEvent
                    },
                    eventObject)
                .ToArray(),
            cancellationToken);

        Logger.ExitMethod();
    }

    protected override async Task HandleInternalAsync(
        ProfileTagsRemovedEvent eventObject,
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

        // Includes null check for payload.
        if (string.IsNullOrWhiteSpace(eventObject.Payload?.ResourceId))
        {
            throw new ArgumentException(
                "The profile id taken from the payload of the event can not be null or empty",
                nameof(eventObject));
        }

        if (eventObject.Payload.Tags == null || !eventObject.Payload.Tags.Any())
        {
            throw new ArgumentException(
                "The tag assignments taken from the payload of the event can not be null or empty",
                nameof(eventObject));
        }

        IFirstLevelProjectionProfile profileToRemoveTagsFrom = await Repository.GetProfileAsync(
            eventObject.Payload.ResourceId,
            transaction,
            cancellationToken);

        Logger.LogInfoMessage(
            "Start handling event to remove tags from object: {object}.",
            LogHelpers.Arguments(profileToRemoveTagsFrom.ToObjectIdent().ToLogString()));

        Guid batchId = await SagaService.CreateBatchAsync(cancellationToken);

        Logger.LogDebugMessage(
            "Created batch with id {batchId} for event with id {eventId}",
            LogHelpers.Arguments(batchId, eventObject.EventId));

        string[] tagsToRemove = eventObject.Payload.Tags;

        IList<FirstLevelProjectionTagAssignment> tagAssignmentsToRemove =
            await Repository.GetTagsAssignmentsFromProfileAsync(
                tagsToRemove,
                eventObject.Payload.ResourceId,
                transaction,
                cancellationToken);

        if (tagAssignmentsToRemove.Count != tagsToRemove.Length)
        {
            IEnumerable<string> missingTags =
                tagsToRemove.Where(t => tagAssignmentsToRemove.All(ta => ta.TagId != t));

            Logger.LogWarnMessage(
                "There is a difference between the tags that are stored in the database and those that should be deleted. The following tags are not present in the database: {ids}",
                LogHelpers.Arguments(missingTags.ToLogString()));
        }

        if (!tagAssignmentsToRemove.Any())
        {
            throw new Exception(
                $"No tags to be deleted are currently assigned to the profile with id {profileToRemoveTagsFrom.Id} and type {profileToRemoveTagsFrom.ToObjectIdent().Type}.");
        }

        foreach (FirstLevelProjectionTagAssignment tagAssignment in tagAssignmentsToRemove)
        {
            await ExecuteSafelyAsync(
                async () =>
                {
                    await Repository.RemoveTagFromProfileAsync(
                        tagAssignment.TagId,
                        profileToRemoveTagsFrom.Id,
                        transaction,
                        cancellationToken);
                },
                () =>
                {
                    Logger.LogWarnMessage(
                        "Tag {tagId} could not be removed from profile with id {id}",
                        LogHelpers.Arguments(tagAssignment.TagId, profileToRemoveTagsFrom.Id));

                    return Task.CompletedTask;
                });
        }

        await CreateTagsRemovedEventAsync(
            batchId,
            eventObject,
            profileToRemoveTagsFrom.ToObjectIdent(),
            tagAssignmentsToRemove,
            profileToRemoveTagsFrom.Id,
            cancellationToken);

        if (tagAssignmentsToRemove.Any(t => t.IsInheritable))
        {
            Logger.LogDebugMessage(
                "Found inheritable tags to remove from child elements. Try to get all child elements of profile with id {id}.",
                LogHelpers.Arguments(profileToRemoveTagsFrom.Id));

            IList<FirstLevelRelationProfile> childElementsOfMember = await ExecuteSafelyAsync(
                () => Repository.GetAllChildrenAsync(
                    profileToRemoveTagsFrom.ToObjectIdent(),
                    transaction,
                    cancellationToken),
                new List<FirstLevelRelationProfile>());

            if (childElementsOfMember.Any())
            {
                Logger.LogDebugMessage(
                    "Found {count} child elements for profile with id {id}",
                    LogHelpers.Arguments(childElementsOfMember.Count, profileToRemoveTagsFrom.Id));

                if (Logger.IsEnabledForTrace())
                {
                    string childElementIdsAsStr = string.Join(
                        " , ",
                        childElementsOfMember.Select(t => t.Profile.Id));

                    Logger.LogTraceMessage(
                        "Found {count} child elements for profile with id {id}. Ids: {children}",
                        LogHelpers.Arguments(
                            childElementsOfMember.Count,
                            profileToRemoveTagsFrom.Id,
                            childElementIdsAsStr));
                }

                IList<FirstLevelProjectionTagAssignment> inheritableTagsToRemove =
                    tagAssignmentsToRemove.Where(t => t.IsInheritable).ToList();

                foreach (IFirstLevelProjectionProfile profileToAssignTags in childElementsOfMember
                             .Select(pm => pm.Profile)
                             .ToList())

                {
                    Logger.LogDebugMessage(
                        "The Member: {member} will be unassigned to the following tags: {tagAssignments}",
                        LogHelpers.Arguments(
                            profileToAssignTags.ToObjectIdent().ToLogString(),
                            inheritableTagsToRemove.ToLogString()));

                    await CreateTagsRemovedEventAsync(
                        batchId,
                        eventObject,
                        new ObjectIdent(profileToAssignTags.Id, profileToAssignTags.Kind.ToObjectType()),
                        inheritableTagsToRemove,
                        profileToRemoveTagsFrom.Id,
                        cancellationToken);
                }
            }
            else
            {
                Logger.LogDebugMessage(
                    "No child elements found to remove tags from. Original profile with id {id}.",
                    LogHelpers.Arguments(profileToRemoveTagsFrom.Id));
            }
        }
        else
        {
            Logger.LogDebugMessage(
                "No inheritable tags found to remove from child elements.",
                LogHelpers.Arguments());
        }

        await SagaService.ExecuteBatchAsync(batchId, cancellationToken);

        Logger.LogInfoMessage(
            "Finished handling event to remove tags from object {object}",
            LogHelpers.Arguments(profileToRemoveTagsFrom.ToObjectIdent().ToLogString()));

        Logger.ExitMethod();
    }
}
