using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
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
///     Default implementation for <see cref="ICommandService{ProfileDeletedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ProfileDeletedMessageService : BaseCommandService<ProfileDeletedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProfileDeletedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ProfileDeletedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public ProfileDeletedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ProfileDeletedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<ProfileDeletedMessage?> ModifyAsync(
        ProfileDeletedMessage? message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (message != null)
        {
            IProfile? repoGroup = await _readService.GetProfileAsync(message.Id, message.ProfileKind);

            if (repoGroup == null)
            {
                throw new InstanceNotFoundException(
                    $"Failed to modify {nameof(ProfileDeletedMessage)} "
                    + $"because no profile with id {message.Id} was found.");
            }

            // Set external ids for further process like validation in external services.
            message.ExternalIds = repoGroup.ExternalIds;
        }

        return await base.ModifyAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        ProfileDeletedMessage message,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        ICollection<ProfileIdent> children = await _readService.GetChildrenOfProfileAsync(
            message.Id,
            message.ProfileKind);

        ICollection<ProfileIdent> parents =
            await _readService.GetParentsOfProfileAsync(message.Id);

        ProfileDeletedEvent eventData =
            CreateEvent<ProfileDeletedEvent, ProfileIdentifierPayload>(
                message,
                correlationId,
                processId,
                initiator,
                p => p.Id);

        eventData.Children = children.ToArray();
        eventData.Parents = parents.ToArray();

        return Logger.ExitMethod(eventData);
    }
}
