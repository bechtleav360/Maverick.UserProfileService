namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{FunctionTagsRemovedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class FunctionTagsRemovedMessageService : BaseCommandService<FunctionTagsRemovedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="FunctionTagsRemovedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="FunctionTagsRemovedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public FunctionTagsRemovedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<FunctionTagsRemovedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<FunctionTagsRemovedMessage> ModifyAsync(
        FunctionTagsRemovedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        message.Tags ??= Array.Empty<string>();

        FunctionTagsRemovedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        FunctionTagsRemovedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        FunctionTagsRemovedEvent eventData =
            CreateEvent<FunctionTagsRemovedEvent, TagsRemovedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.ResourceId);

        Task<IUserProfileServiceEvent> result = Task.FromResult<IUserProfileServiceEvent>(eventData);

        return Logger.ExitMethod(result);
    }
}
