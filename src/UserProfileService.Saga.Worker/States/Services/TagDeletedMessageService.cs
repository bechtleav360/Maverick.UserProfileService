using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Worker.Abstractions;

namespace UserProfileService.Saga.Worker.States.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{TagDeletedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class TagDeletedMessageService : BaseCommandService<TagDeletedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="TagDeletedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="TagDeletedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public TagDeletedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<TagDeletedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        TagDeletedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        TagDeletedEvent eventData =
            CreateEvent<TagDeletedEvent, IdentifierPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        Tag tag = await _readService.GetTagAsync(eventData.OldTag.Id);
        eventData.OldTag = tag;

        return Logger.ExitMethod(eventData);
    }
}
