namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{ProfileClientSettingsDeletedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ProfileClientSettingsDeletedMessageService : BaseCommandService<ProfileClientSettingsDeletedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProfileClientSettingsDeletedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ProfileClientSettingsDeletedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public ProfileClientSettingsDeletedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ProfileClientSettingsDeletedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        ProfileClientSettingsDeletedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        IProfile profile = await _readService.GetProfileAsync(
            message.Resource.Id,
            ProfileKind.Unknown);

        message.Resource.ProfileKind = profile.Kind;

        ProfileClientSettingsDeletedEvent eventData =
            CreateEvent<ProfileClientSettingsDeletedEvent, ClientSettingsDeletedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Resource.Id);

        return Logger.ExitMethod(eventData);
    }
}
