using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{OrganizationCreatedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class OrganizationCreatedMessageService : BaseCommandService<OrganizationCreatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="OrganizationCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="OrganizationCreatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public OrganizationCreatedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<OrganizationCreatedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override Task<OrganizationCreatedMessage> ModifyAsync(
        OrganizationCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        message.Id = Guid.NewGuid().ToString();
        message.Members ??= Array.Empty<ConditionObjectIdent>();
        message.Tags ??= Array.Empty<TagAssignment>();
        message.ExternalIds ??= new List<ExternalIdentifier>();
        message.ExternalIds = message.ExternalIds.Where(ei => ei != null).ToList();
        message.Members.AddDefaultConditions();

        Task<OrganizationCreatedMessage> result = Task.FromResult(message);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        OrganizationCreatedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        OrganizationCreatedEvent eventData =
            CreateEvent<OrganizationCreatedEvent, OrganizationCreatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(Task.FromResult<IUserProfileServiceEvent>(eventData));
    }
}
