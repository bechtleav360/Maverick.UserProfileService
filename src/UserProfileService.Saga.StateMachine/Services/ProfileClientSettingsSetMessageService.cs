﻿using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{ProfileClientSettingsSetMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ProfileClientSettingsSetMessageService : BaseCommandService<ProfileClientSettingsSetMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProfileClientSettingsSetMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ProfileClientSettingsSetMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public ProfileClientSettingsSetMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ProfileClientSettingsSetMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        ProfileClientSettingsSetMessage message,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        IProfile? profile = await _readService.GetProfileAsync(
            message.Resource.Id,
            ProfileKind.Unknown);

        if (profile == null)
        {
            throw new InstanceNotFoundException(
                $"Failed to create {nameof(ProfileClientSettingsSetEvent)} "
                + $"because no profile with id {message.Resource.Id} was found.");
        }

        message.Resource.ProfileKind = profile.Kind;

        ProfileClientSettingsSetEvent eventData =
            CreateEvent<ProfileClientSettingsSetEvent, ClientSettingsSetPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Resource.Id);

        return Logger.ExitMethod(eventData);
    }
}
