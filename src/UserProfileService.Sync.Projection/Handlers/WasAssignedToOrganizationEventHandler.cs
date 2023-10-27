using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

internal class WasAssignedToOrganizationEventHandler : SyncBaseEventHandler<WasAssignedToOrganization>
{
    private readonly IStreamNameResolver _streamNameResolver;

    /// <summary>
    ///     Create a new instance of <see cref="UserCreatedEventHandler" />
    /// </summary>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    /// <param name="profileService">An instance of <see cref="IProfileService" /> used to handle user operations.</param>
    /// <param name="streamNameResolver">
    ///     Object used to resolve object information through the stream name
    ///     <see cref="IStreamNameResolver" />
    /// </param>
    public WasAssignedToOrganizationEventHandler(
        ILogger<WasAssignedToOrganizationEventHandler> logger,
        IProfileService profileService,
        IStreamNameResolver streamNameResolver) : base(logger, profileService)
    {
        _streamNameResolver = streamNameResolver;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        WasAssignedToOrganization eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (eventHeader == null)
        {
            throw new ArgumentNullException(nameof(eventHeader));
        }

        if (string.IsNullOrWhiteSpace(eventObject.ProfileId))
        {
            throw new ArgumentException("Profile id should not be null or whitespace", nameof(eventObject.ProfileId));
        }

        if (string.IsNullOrWhiteSpace(eventObject.Target.Id))
        {
            throw new ArgumentNullException(nameof(eventObject.Target.Id));
        }

        Logger.LogInfoMessage(
            "Assigning profile with id: {profileId} to organization with the id: {orgaId} in the sync database",
            LogHelpers.Arguments(eventObject.ProfileId, eventObject.Target.Id));

        ObjectIdent relatedObjectIdent =
            _streamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId);

        if (relatedObjectIdent.Id == eventObject.ProfileId
            && relatedObjectIdent.Type == ObjectType.Organization)
        {
            try
            {
                string parentId = eventObject.Target.Id;

                var organization = await ProfileService.GetProfileAsync<OrganizationSync>(
                    eventObject.ProfileId,
                    cancellationToken);

                var objectRelation = new ObjectRelation(
                    AssignmentType.ParentsToChild,
                    new KeyProperties(
                        eventObject.Target.ExternalIds.FirstOrDefault()?.Id,
                        eventObject.Target.ExternalIds.FirstOrDefault()?.Source),
                    parentId,
                    ObjectType.Organization,
                    eventObject.Conditions?.ToList());

                organization.AddObjectRelation(objectRelation);

                await ProfileService.UpdateProfileAsync(organization, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "Error happened by updating organization with the id: {orgaId} in the sync database",
                    LogHelpers.Arguments(eventObject.ProfileId));

                throw;
            }
            finally
            {
                Logger.ExitMethod();
            }
        }
    }
}
