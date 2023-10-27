using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="EntityDeleted" /> event.
/// </summary>
internal class EntityDeletedEventHandler : SecondLevelEventHandlerBase<EntityDeleted>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="EntityDeletedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper"></param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">the logger to be used.</param>
    public EntityDeletedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ILogger<ContainerDeletedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        EntityDeleted domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        string entityId = domainEvent.Id;
        ISecondLevelProjectionProfile profileToDelete = null;

        if (relatedEntityIdent.Type == ObjectType.User)
        {
            profileToDelete =
                await Repository.GetProfileAsync(entityId, cancellationToken: cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentNullException(nameof(entityId));
        }

        switch (relatedEntityIdent.Type)
        {
            case ObjectType.Profile:
            case ObjectType.User:
            case ObjectType.Group:
            case ObjectType.Organization:

                Logger.LogInfoMessage(
                    "Deleting profile (Id = {profileId})",
                    LogHelpers.Arguments(entityId));

                await ExecuteInsideTransactionAsync(
                    (repo, t, ct)
                        => repo.DeleteProfileAsync(
                            entityId,
                            t,
                            ct),
                    eventHeader,
                    cancellationToken);

                break;
            case ObjectType.Role:

                Logger.LogInfoMessage(
                    "Deleting role (Id = {roleId})",
                    LogHelpers.Arguments(entityId));

                await ExecuteInsideTransactionAsync(
                    (repo, t, ct)
                        => repo.DeleteRoleAsync(
                            entityId,
                            t,
                            ct),
                    eventHeader,
                    cancellationToken);

                break;

            case ObjectType.Function:

                Logger.LogInfoMessage(
                    "Deleting function (Id = {functionId})",
                    LogHelpers.Arguments(entityId));

                await ExecuteInsideTransactionAsync(
                    (repo, t, ct)
                        => repo.DeleteFunctionAsync(
                            entityId,
                            t,
                            ct),
                    eventHeader,
                    cancellationToken);

                break;
            case ObjectType.Tag:

                Logger.LogInfoMessage(
                    "Deleting tag (Id = {tagId})",
                    LogHelpers.Arguments(entityId));

                await ExecuteInsideTransactionAsync(
                    (repo, t, ct)
                        => repo.RemoveTagAsync(
                            entityId,
                            t,
                            ct),
                    eventHeader,
                    cancellationToken);

                break;
            case ObjectType.Unknown:
            default:
                throw new NotSupportedException(
                    $"The type '{relatedEntityIdent.Type}' is not supported by this method.");
        }
        Logger.ExitMethod();
    }
}
