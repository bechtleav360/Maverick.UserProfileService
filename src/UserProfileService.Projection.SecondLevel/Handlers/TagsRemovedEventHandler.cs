using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="TagsRemoved" /> event.
/// </summary>
internal class TagsRemovedEventHandler : SecondLevelEventHandlerBase<TagsRemoved>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="TagsRemovedEventHandler" />.
    /// </summary>
    /// <param name="repository"> The repository to be used. </param>
    /// <param name="mapper"> </param>
    /// <param name="streamNameResolver"> The resolver that will convert from or to stream names. </param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger"> the logger to be used. </param>
    public TagsRemovedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ILogger<MemberRemovedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        TagsRemoved domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(domainEvent.ToLogString()));
        }

        if (string.IsNullOrWhiteSpace(domainEvent.Id))
        {
            throw new InvalidDomainEventException(
                "Could not remove tags to resource: Resource id is missing.",
                domainEvent);
        }

        if (domainEvent.ObjectType == ObjectType.Unknown)
        {
            throw new InvalidDomainEventException(
                "Could not remove tags to resource: Type of object is unknown.",
                domainEvent);
        }

        if (domainEvent.Tags == null || !domainEvent.Tags.Any())
        {
            throw new InvalidDomainEventException(
                "Could not remove tags to resource: Tags are null or empty.",
                domainEvent);
        }

        await ExecuteInsideTransactionAsync(
            async (repo, t, ct)
                =>
            {
                await repo.RemoveTagFromObjectAsync(
                    relatedEntityIdent.Id ?? domainEvent.Id,
                    domainEvent.Id,
                    domainEvent.ObjectType,
                    domainEvent.Tags,
                    t,
                    ct);

                // set new UpdateAt date depending on object type
                switch (domainEvent.ObjectType)
                {
                    case ObjectType.Profile:
                    case ObjectType.Group:
                    case ObjectType.User:
                    case ObjectType.Organization:

                        await UpdateProfileTimestampAsync(
                            domainEvent.Id,
                            domainEvent.MetaData.Timestamp,
                            repo,
                            t,
                            ct);

                        break;
                    case ObjectType.Role:

                        await UpdateRoleTimestampAsync(
                            domainEvent.Id,
                            domainEvent.MetaData.Timestamp,
                            repo,
                            t,
                            ct);

                        break;
                    case ObjectType.Function:

                        await UpdateFunctionTimestampAsync(
                            domainEvent.Id,
                            domainEvent.MetaData.Timestamp,
                            repo,
                            t,
                            ct);

                        break;
                }
            },
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
