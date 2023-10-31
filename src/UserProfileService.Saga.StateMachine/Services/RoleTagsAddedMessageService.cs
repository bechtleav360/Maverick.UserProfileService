namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{RoleTagsAddedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class RoleTagsAddedMessageService : BaseCommandService<RoleTagsAddedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="RoleTagsAddedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="RoleTagsAddedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public RoleTagsAddedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<RoleTagsAddedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<RoleTagsAddedMessage> ModifyAsync(
        RoleTagsAddedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        message.Tags ??= Array.Empty<TagAssignment>();

        RoleTagsAddedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        RoleTagsAddedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        RoleTagsAddedEvent eventData =
            CreateEvent<RoleTagsAddedEvent, TagsSetPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        Task<IUserProfileServiceEvent> result = Task.FromResult<IUserProfileServiceEvent>(eventData);

        return Logger.ExitMethod(result);
    }
}
