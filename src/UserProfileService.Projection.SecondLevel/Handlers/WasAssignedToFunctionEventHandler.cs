using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="WasAssignedToFunction" /> event.
/// </summary>
internal class WasAssignedToFunctionEventHandler : SecondLevelEventHandlerBase<WasAssignedToFunction>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="WasAssignedToFunctionEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">The mapper is used to map objects from one type, to another.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public WasAssignedToFunctionEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        ILogger<WasAssignedToFunctionEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        WasAssignedToFunction domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}, relatedEntityIdent: {relatedEntityIdent}",
                LogHelpers.Arguments(domainEvent.ToLogString(), relatedEntityIdent.ToLogString()));
        }

        await ExecuteInsideTransactionAsync(
            async (repo, t, ct) =>
            {
                var functionAssignedTo = Mapper.Map<SecondLevelProjectionFunction>(domainEvent.Target);

                if (Logger.IsEnabledForTrace())
                {
                    Logger.LogTraceMessage(
                        "The mapped Second-Level-Projection-Function: {mappedFunction}",
                        functionAssignedTo.ToLogString().AsArgumentList());
                }

                await repo.AddMemberOfAsync(
                    relatedEntityIdent.Id,
                    domainEvent.ProfileId,
                    domainEvent.Conditions,
                    functionAssignedTo,
                    t,
                    ct);

                await UpdateProfileTimestampAsync(
                    domainEvent.ProfileId,
                    domainEvent.MetaData.Timestamp,
                    repo,
                    t,
                    ct);
            },
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
