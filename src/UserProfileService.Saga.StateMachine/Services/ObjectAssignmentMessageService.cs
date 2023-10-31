namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{ObjectAssignmentMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ObjectAssignmentMessageService : BaseCommandService<ObjectAssignmentMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="ObjectAssignmentMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ObjectAssignmentMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public ObjectAssignmentMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ObjectAssignmentMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<ObjectAssignmentMessage> ModifyAsync(
        ObjectAssignmentMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        message.Added ??= Array.Empty<ConditionObjectIdent>();
        message.Removed ??= Array.Empty<ConditionObjectIdent>();
        message.Added.AddDefaultConditions();
        message.Removed.AddDefaultConditions();

        ObjectAssignmentMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        ObjectAssignmentMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        ObjectAssignmentEvent eventData =
            CreateEvent<ObjectAssignmentEvent, AssignmentPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Resource.Id);

        return Logger.ExitMethod(Task.FromResult<IUserProfileServiceEvent>(eventData));
    }
}
