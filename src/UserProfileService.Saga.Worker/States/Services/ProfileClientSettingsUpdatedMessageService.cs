using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Worker.Abstractions;

namespace UserProfileService.Saga.Worker.States.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{ProfileClientSettingsUpdatedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ProfileClientSettingsUpdatedMessageService : BaseCommandService<ProfileClientSettingsUpdatedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProfileClientSettingsUpdatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ProfileClientSettingsUpdatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public ProfileClientSettingsUpdatedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ProfileClientSettingsUpdatedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        ProfileClientSettingsUpdatedMessage message,
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

        JObject settings = await _readService.GetSettingsOfProfileAsync(
            message.Resource.Id,
            message.Resource.ProfileKind,
            message.Key);

        settings.Merge(
            message.Settings,
            new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

        var mergedPayload = new ClientSettingsSetPayload
        {
            Key = message.Key,
            Resource = message.Resource,
            Settings = settings
        };

        ProfileClientSettingsUpdatedEvent eventData =
            CreateEvent<ProfileClientSettingsUpdatedEvent, ClientSettingsSetPayload>(
                mergedPayload,
                correlationId,
                processId,
                initiator,
                m => m.Resource.Id);

        return Logger.ExitMethod(eventData);
    }
}
