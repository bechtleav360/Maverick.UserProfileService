namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{ProfileTagsAddedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ProfileTagsAddedMessageService : BaseCommandService<ProfileTagsAddedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProfileTagsAddedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ProfileTagsAddedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public ProfileTagsAddedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ProfileTagsAddedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<ProfileTagsAddedMessage> ModifyAsync(
        ProfileTagsAddedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        message.Tags ??= Array.Empty<TagAssignment>();

        ProfileTagsAddedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        ProfileTagsAddedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        ProfileTagsAddedEvent eventData =
            CreateEvent<ProfileTagsAddedEvent, TagsSetPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        IProfile profile = await _readService.GetProfileAsync(eventData.Payload.Id, ProfileKind.Unknown);

        eventData.ProfileKind = profile.Kind;

        return Logger.ExitMethod(eventData);
    }
}
