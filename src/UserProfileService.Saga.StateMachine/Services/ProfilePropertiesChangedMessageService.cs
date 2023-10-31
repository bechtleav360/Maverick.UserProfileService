﻿namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{ProfilePropertiesChangedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ProfilePropertiesChangedMessageService : BaseCommandService<ProfilePropertiesChangedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProfilePropertiesChangedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ProfilePropertiesChangedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public ProfilePropertiesChangedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ProfilePropertiesChangedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<ProfilePropertiesChangedMessage> ModifyAsync(
        ProfilePropertiesChangedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        ValidationExtension.RemoveEnumerableNullValues<IList<ExternalIdentifier>, ExternalIdentifier>(
            message.Properties,
            nameof(IProfile.ExternalIds),
            true);

        ProfilePropertiesChangedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        ProfilePropertiesChangedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        ProfilePropertiesChangedEvent eventData =
            CreateEvent<ProfilePropertiesChangedEvent, PropertiesUpdatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        IProfile profile = await _readService.GetProfileAsync(eventData.Payload.Id, ProfileKind.Unknown);

        eventData.OldProfile = profile;
        eventData.ProfileKind = profile.Kind;

        return Logger.ExitMethod(eventData);
    }
}
