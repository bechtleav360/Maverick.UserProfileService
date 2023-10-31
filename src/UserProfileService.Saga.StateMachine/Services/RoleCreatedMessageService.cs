namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{RoleCreatedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class RoleCreatedMessageService : BaseCommandService<RoleCreatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="RoleCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="RoleCreatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public RoleCreatedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<RoleCreatedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<RoleCreatedMessage> ModifyAsync(
        RoleCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        message.Id = Guid.NewGuid().ToString();

        message.Permissions ??= new List<string>();
        message.DeniedPermissions ??= new List<string>();
        message.Tags ??= Array.Empty<TagAssignment>();
        message.ExternalIds = message.ExternalIds.Where(ei => ei != null).ToList();

        // Remove all null or empty permissions
        message.Permissions =
            message.Permissions.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

        message.DeniedPermissions =
            message.DeniedPermissions.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

        RoleCreatedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        RoleCreatedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        RoleCreatedEvent eventData =
            CreateEvent<RoleCreatedEvent, RoleCreatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(Task.FromResult<IUserProfileServiceEvent>(eventData));
    }
}
