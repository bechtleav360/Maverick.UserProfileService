using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Worker.Abstractions;

namespace UserProfileService.Saga.Worker.States.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{RoleTagsRemovedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class RoleTagsRemovedMessageService : BaseCommandService<RoleTagsRemovedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="RoleTagsRemovedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="RoleTagsRemovedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public RoleTagsRemovedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<RoleTagsRemovedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<RoleTagsRemovedMessage> ModifyAsync(
        RoleTagsRemovedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        message.Tags ??= Array.Empty<string>();

        RoleTagsRemovedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        RoleTagsRemovedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        RoleTagsRemovedEvent eventData =
            CreateEvent<RoleTagsRemovedEvent, TagsRemovedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.ResourceId);

        Task<IUserProfileServiceEvent> result = Task.FromResult<IUserProfileServiceEvent>(eventData);

        return Logger.ExitMethod(result);
    }
}
