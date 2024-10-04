using System.Diagnostics;
using System.Net;
using AutoMapper;
using MassTransit;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UserProfileService.Abstractions;
using UserProfileService.Api.Common.Abstractions;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;
using UserProfileService.Common.V2.TicketStore.Models;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Extensions;
using UserProfileService.Saga.Events.Extensions;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Validation.Abstractions;
using FunctionCreatedPayloadV3 = UserProfileService.Events.Payloads.V3.FunctionCreatedPayload;
using UserCreatedPayloadV3 = UserProfileService.Events.Payloads.V3.UserCreatedPayload;
using ValidationResult = UserProfileService.Validation.Abstractions.ValidationResult;

namespace UserProfileService.Utilities;

/// <summary>
///     Implementation of the OperationHandler for the API
/// </summary>
public class ApiOperationHandler : IOperationHandler, IVolatileDataOperationHandler
{
    protected readonly ILogger<ApiOperationHandler> _logger;
    protected readonly IMapper _mapper;
    protected readonly IBus _messageBus;
    protected readonly IPayloadValidationService _payloadValidator;
    protected readonly ITicketStore _ticketStore;
    protected readonly IUserContextStore _userContext;

    public ApiOperationHandler(
        ITicketStore ticketStore,
        IBus messageBus,
        IMapper mapper,
        IUserContextStore userContext,
        IPayloadValidationService payloadValidator,
        ILogger<ApiOperationHandler> logger)
    {
        _ticketStore = ticketStore;
        _messageBus = messageBus;
        _mapper = mapper;
        _userContext = userContext;
        _payloadValidator = payloadValidator;
        _logger = logger;
    }

    /// <summary>
    ///     Publishes the given message and creates a ticket for it.
    ///     It handles all errors that may occur.
    ///     1. Unable to write to ticket-store
    ///     2. Unable to publish message
    ///     There should be no message without an ticket. If the publishing fails, the ticket will be set to failed.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <typeparam name="TPayload">The type of the inherited payload of the message.</typeparam>
    /// <param name="payload">The payload to send.</param>
    /// <param name="operation">The type of the operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> which can be used in oder to abort the request.</param>
    /// <param name="objectIds">The related objects.</param>
    /// <param name="additionalQueryParameter">An additional query string to filter for information.</param>
    /// <returns>The id of the created ticket.</returns>
    protected async Task<string> PublishMessageWithTicket<TMessage, TPayload>(
        TMessage payload,
        string operation,
        CancellationToken cancellationToken = default,
        string additionalQueryParameter = null,
        params string[] objectIds) where TMessage : TPayload
    {
        _logger.EnterMethod();

        var ticketGuid = Guid.NewGuid();
        var ticketId = ticketGuid.ToString("D");
        DateTime startTime = DateTime.UtcNow;
        CommandInitiator initiator = GetInitiator();

        try
        {
            ValidateMessage<TMessage, TPayload>(payload);

            await _ticketStore.AddOrUpdateEntryAsync(
                new UserProfileOperationTicket(ticketId, objectIds, operation)
                {
                    CorrelationId = Activity.Current?.Id,
                    Started = startTime,
                    Initiator = initiator.Id,
                    AdditionalQueryParameter = additionalQueryParameter
                },
                cancellationToken);

            await _messageBus.Publish(
                new StartCollectingMessage
                {
                    ExternalProcessId = ticketId,
                    CollectItemsAccount = 1,
                    CollectingId = ticketGuid,
                    Dispatch = new StatusDispatch(1)
                },
                cancellationToken);

            await _messageBus.Publish(
                payload.ToCommand(ticketId, ticketGuid, initiator),
                cancellationToken);
        }
        catch (Exception e)
        {
            // update ticket when the message could not be sent
            await _ticketStore.AddOrUpdateEntryAsync(
                new UserProfileOperationTicket(ticketId, objectIds, operation)
                {
                    CorrelationId = Activity.Current?.Id,
                    Status = TicketStatus.Failure,
                    Finished = DateTime.UtcNow,
                    Started = startTime,
                    Initiator = initiator.Id,
                    Details = new ProblemDetails
                    {
                        Title = "Unable to forward the request for validation",
                        Detail = $"Unable to forward the request for validation. {e.Message}",
                        Status = (int)HttpStatusCode.InternalServerError,
                        Extensions =
                        {
                            { "Exception", e.GetType().Name }
                        }
                    }
                },
                cancellationToken);

            throw;
        }

        return _logger.ExitMethod<string>(ticketId);
    }

    private void ValidateMessage<TMessage, TPayload>(TMessage message) where TMessage : TPayload
    {
        ValidationResult result = _payloadValidator.ValidateObject<TPayload>(message);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    private CommandInitiator GetInitiator()
    {
        _logger.EnterMethod();
        
        string userId = _userContext.GetIdOfCurrentUser();

        return _logger.ExitMethod(
            !string.IsNullOrWhiteSpace(userId)
                ? new CommandInitiator(userId, CommandInitiatorType.User)
                : new CommandInitiator(string.Empty));
    }

    /// <inheritdoc />
    public async Task<string> CreateUserProfileAsync(
        CreateUserRequest user,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        UserCreatedMessage payload = _mapper.Map<CreateUserRequest, UserCreatedMessage>(user);

        payload.Id =
            Guid.NewGuid()
                .ToString(); // The validator needs the id, because the validator is used in another place.

        string ticketId = await PublishMessageWithTicket<UserCreatedMessage, UserCreatedPayloadV3>(
            payload,
            WellKnownTicketOperations.CreateUserProfile,
            cancellationToken);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateGroupProfileAsync(
        CreateGroupRequest group,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        GroupCreatedMessage payload = _mapper.Map<CreateGroupRequest, GroupCreatedMessage>(group);

        payload.Id =
            Guid.NewGuid()
                .ToString(); // The validator needs the id, because the validator is used in another place.

        string ticketId = await PublishMessageWithTicket<GroupCreatedMessage, GroupCreatedPayload>(
            payload,
            WellKnownTicketOperations.CreateGroupProfile,
            cancellationToken);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateOrganizationProfileAsync(
        CreateOrganizationRequest organization,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        OrganizationCreatedMessage payload =
            _mapper.Map<CreateOrganizationRequest, OrganizationCreatedMessage>(organization);

        payload.Id =
            Guid.NewGuid()
                .ToString(); // The validator needs the id, because the validator is used in another place.

        string ticketId = await PublishMessageWithTicket<OrganizationCreatedMessage, OrganizationCreatedPayload>(
            payload,
            WellKnownTicketOperations.CreateOrganizationProfile,
            cancellationToken);

        return _logger.ExitMethod<string>(ticketId);
    }

    public async Task<string> CreateTagAsync(
        CreateTagRequest tag,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        TagCreatedMessage payload = _mapper.Map<CreateTagRequest, TagCreatedMessage>(tag);

        payload.Id =
            Guid.NewGuid()
                .ToString(); // The validator needs the id, because the validator is used in another place.

        string ticketId = await PublishMessageWithTicket<TagCreatedMessage, TagCreatedPayload>(
            payload,
            WellKnownTicketOperations.CreateTag,
            cancellationToken);

        return _logger.ExitMethod<string>(ticketId);
    }

    public async Task<string> UpdateUserProfileAsync(
        string profileId,
        UserModifiableProperties profile,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ProfilePropertiesChangedMessage, PropertiesUpdatedPayload>(
            new ProfilePropertiesChangedMessage
            {
                Properties = profile.ToPropertiesChangeDictionary(),
                Id = profileId,
                ProfileKind = ProfileKind.User
            },
            WellKnownTicketOperations.UpdateUserProfile,
            cancellationToken,
            null,
            profileId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateGroupProfileAsync(
        string profileId,
        GroupModifiableProperties profile,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ProfilePropertiesChangedMessage, PropertiesUpdatedPayload>(
            new ProfilePropertiesChangedMessage
            {
                Properties = profile.ToPropertiesChangeDictionary(),
                Id = profileId,
                ProfileKind = ProfileKind.Group
            },
            WellKnownTicketOperations.UpdateGroupProfile,
            cancellationToken,
            null,
            profileId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateOrganizationProfileAsync(
        string profileId,
        OrganizationModifiableProperties profile,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ProfilePropertiesChangedMessage, PropertiesUpdatedPayload>(
            new ProfilePropertiesChangedMessage
            {
                Properties = profile.ToPropertiesChangeDictionary(),
                Id = profileId,
                ProfileKind = ProfileKind.Organization
            },
            WellKnownTicketOperations.UpdateOrganizationProfile,
            cancellationToken,
            null,
            profileId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteGroupAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ProfileDeletedMessage, ProfileIdentifierPayload>(
            new ProfileDeletedMessage
            {
                Id = id,
                ProfileKind = ProfileKind.Group
            },
            WellKnownTicketOperations.DeleteGroup,
            cancellationToken,
            null,
            id);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteOrganizationAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ProfileDeletedMessage, ProfileIdentifierPayload>(
            new ProfileDeletedMessage
            {
                Id = id,
                ProfileKind = ProfileKind.Organization
            },
            WellKnownTicketOperations.DeleteOrganization,
            cancellationToken,
            null,
            id);

        return _logger.ExitMethod<string>(ticketId);
    }

    public async Task<string> DeleteTagAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<TagDeletedMessage, IdentifierPayload>(
            new TagDeletedMessage
            {
                Id = id
            },
            WellKnownTicketOperations.DeleteTag,
            cancellationToken,
            null,
            id);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteUserAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ProfileDeletedMessage, ProfileIdentifierPayload>(
            new ProfileDeletedMessage
            {
                Id = id,
                ProfileKind = ProfileKind.User
            },
            WellKnownTicketOperations.DeleteUser,
            cancellationToken,
            null,
            id);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> AddProfileToRoleAsync(
        string profileId,
        ProfileKind profileKind,
        string roleId,
        RangeCondition[] conditions,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(roleId, ObjectType.Role),
                Type = AssignmentType.Unknown,
                Added = new[] { new ConditionObjectIdent(profileId, profileKind.ToObjectType(), conditions) }
            },
            profileKind == ProfileKind.User
                ? WellKnownTicketOperations.AddUserToRole
                : WellKnownTicketOperations.AddContainerProfileToRole,
            cancellationToken,
            null,
            roleId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateProfileToRoleAssignmentsAsync(
        string roleId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(roleId, ObjectType.Role),
                Type = AssignmentType.Unknown,
                Added = assignments.Added
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray(),
                Removed = assignments.Removed
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray()
            },
            WellKnownTicketOperations.UpdateProfilesOfRole,
            cancellationToken,
            null,
            roleId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> AddProfileToFunctionAsync(
        string profileId,
        ProfileKind profileKind,
        string functionId,
        RangeCondition[] conditions,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(functionId, ObjectType.Function),
                Type = AssignmentType.Unknown,
                Added = new[] { new ConditionObjectIdent(profileId, profileKind.ToObjectType(), conditions) }
            },
            WellKnownTicketOperations.UpdateProfilesOfFunction,
            cancellationToken,
            null,
            functionId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateProfileToFunctionAssignmentsAsync(
        string functionId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(functionId, ObjectType.Function),
                Type = AssignmentType.Unknown,
                Added = assignments.Added
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray(),
                Removed = assignments.Removed
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray()
            },
            WellKnownTicketOperations.UpdateProfilesOfFunction,
            cancellationToken,
            null,
            functionId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> RemoveProfileFromRoleAsync(
        string profileId,
        ProfileKind profileKind,
        string roleId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(roleId, ObjectType.Role),
                Type = AssignmentType.Unknown,
                Removed = new[]
                {
                    new ConditionObjectIdent
                    {
                        Id = profileId,
                        Type = profileKind.ToObjectType()
                    }
                }
            },
            profileKind == ProfileKind.User
                ? WellKnownTicketOperations.RemoveUserFromRole
                : WellKnownTicketOperations.RemoveContainerProfileFromRole,
            cancellationToken,
            null,
            roleId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> RemoveProfileFromFunctionAsync(
        string profileId,
        ProfileKind profileKind,
        string functionId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(functionId, ObjectType.Function),
                Type = AssignmentType.Unknown,
                Removed = new[]
                {
                    new ConditionObjectIdent
                    {
                        Id = profileId,
                        Type = profileKind.ToObjectType()
                    }
                }
            },
            profileKind == ProfileKind.User
                ? WellKnownTicketOperations.RemoveUserFromFunction
                : WellKnownTicketOperations.RemoveContainerProfileFromFunction,
            cancellationToken,
            null,
            functionId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> AssignProfilesToContainerProfileAsync(
        ProfileIdent[] profiles,
        ProfileIdent containerProfile,
        RangeCondition[] conditions,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(containerProfile.Id, containerProfile.ProfileKind.ToObjectType()),
                Type = AssignmentType.ChildrenToParent,
                Added = profiles
                    .Select(t => new ConditionObjectIdent(t.Id, t.ProfileKind.ToObjectType(), conditions))
                    .ToArray()
            },
            containerProfile.ProfileKind == ProfileKind.Organization
                ? WellKnownTicketOperations.AssignProfilesToOrganizationProfile
                : WellKnownTicketOperations.AssignProfilesToGroupProfile,
            cancellationToken,
            null,
            containerProfile.Id);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateProfilesToContainerProfileAssignmentsAsync(
        ProfileIdent containerProfile,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(containerProfile.Id, containerProfile.ProfileKind.ToObjectType()),
                Type = AssignmentType.ChildrenToParent,
                Added = assignments.Added
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray(),
                Removed = assignments.Removed
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray()
            },
            containerProfile.ProfileKind == ProfileKind.Organization
                ? WellKnownTicketOperations.UpdateProfilesToOrganizationProfileAssignments
                : WellKnownTicketOperations.UpdateProfilesToGroupProfileAssignments,
            cancellationToken,
            null,
            containerProfile.Id);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> RemoveProfileAssignmentsFromContainerProfileAsync(
        ProfileIdent[] profileIds,
        ProfileIdent profile,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(profile.Id, profile.ProfileKind.ToObjectType()),
                Type = AssignmentType.ChildrenToParent,
                Removed = profileIds.Select(t => new ConditionObjectIdent(t.Id, t.ProfileKind.ToObjectType()))
                    .ToArray()
            },
            profile.ProfileKind == ProfileKind.Organization
                ? WellKnownTicketOperations.AssignProfilesToOrganizationProfile
                : WellKnownTicketOperations.AssignProfilesToGroupProfile,
            cancellationToken,
            null,
            profile.Id);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateRoleToUserAssignmentsAsync(
        string userId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(userId, ObjectType.User),
                Type = AssignmentType.Unknown,
                Added = assignments.Added.Select(a => new ConditionObjectIdent(a.Id, ObjectType.Role, a.Conditions))
                    .ToArray(),
                Removed = assignments.Removed
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Role, a.Conditions))
                    .ToArray()
            },
            WellKnownTicketOperations.UpdateRolesOfUser,
            cancellationToken,
            null,
            userId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateFunctionToUserAssignmentsAsync(
        string userId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(userId, ObjectType.User),
                Type = AssignmentType.Unknown,
                Added = assignments.Added
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Function, a.Conditions))
                    .ToArray(),
                Removed = assignments.Removed
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Function, a.Conditions))
                    .ToArray()
            },
            WellKnownTicketOperations.UpdateFunctionsOfUser,
            cancellationToken,
            null,
            userId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateContainerProfileToUserAssignmentsAsync(
        string userId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<ObjectAssignmentMessage, AssignmentPayload>(
            new ObjectAssignmentMessage
            {
                Resource = new ObjectIdent(userId, ObjectType.User),
                Type = AssignmentType.ParentsToChild,
                Added = assignments.Added
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray(),
                Removed = assignments.Removed
                    .Select(a => new ConditionObjectIdent(a.Id, ObjectType.Profile, a.Conditions))
                    .ToArray()
            },
            WellKnownTicketOperations.UpdateContainerProfilesOfUser,
            cancellationToken,
            null,
            userId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateRoleAsync(
        CreateRoleRequest role,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        RoleCreatedMessage payload = _mapper.Map<CreateRoleRequest, RoleCreatedMessage>(role);

        payload.Id =
            Guid.NewGuid()
                .ToString(); // The validator needs the id, because the validator is used in another place

        string ticketId = await PublishMessageWithTicket<RoleCreatedMessage, RoleCreatedPayload>(
            payload,
            WellKnownTicketOperations.CreateRole,
            cancellationToken);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateRoleAsync(
        string roleId,
        RoleModifiableProperties roleToUpdate,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<RolePropertiesChangedMessage, PropertiesUpdatedPayload>(
            new RolePropertiesChangedMessage
            {
                Properties = roleToUpdate.ToPropertiesChangeDictionary(),
                Id = roleId
            },
            WellKnownTicketOperations.UpdateRole,
            cancellationToken,
            null,
            roleId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteRoleAsync(
        string roleId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<RoleDeletedMessage, IdentifierPayload>(
            new RoleDeletedMessage
            {
                Id = roleId
            },
            WellKnownTicketOperations.DeleteRole,
            cancellationToken,
            null,
            roleId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateFunctionAsync(
        CreateFunctionRequest function,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        FunctionCreatedMessage payload = _mapper.Map<CreateFunctionRequest, FunctionCreatedMessage>(function);

        payload.Id =
            Guid.NewGuid()
                .ToString(); // The validator needs the id, because the validator is used in another place.

        string ticketId = await PublishMessageWithTicket<FunctionCreatedMessage, FunctionCreatedPayloadV3>(
            payload,
            WellKnownTicketOperations.CreateFunction,
            cancellationToken);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteFunctionAsync(
        string functionId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<FunctionDeletedMessage, IdentifierPayload>(
            new FunctionDeletedMessage
            {
                Id = functionId
            },
            WellKnownTicketOperations.DeleteFunction,
            cancellationToken,
            null,
            functionId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateProfileTagsAsync(
        string profileId,
        ProfileKind profileKind,
        TagAssignment[] tags,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string operation = profileKind switch
        {
            ProfileKind.Group => WellKnownTicketOperations.CreateGroupTags,
            ProfileKind.User => WellKnownTicketOperations.CreateUserTags,
            ProfileKind.Organization => WellKnownTicketOperations.CreateOrganizationTags,
            ProfileKind.Unknown => throw new ArgumentException("Unsupported profile kind: Unknown", nameof(profileKind)),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, "Unsupported profile kind")
        };

        string ticketId = await PublishMessageWithTicket<ProfileTagsAddedMessage, TagsSetPayload>(
            new ProfileTagsAddedMessage
            {
                ProfileKind = profileKind,
                Id = profileId,
                Tags = tags
            },
            operation,
            cancellationToken,
            null,
            profileId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateRoleTagsAsync(
        string roleId,
        TagAssignment[] tags,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<RoleTagsAddedMessage, TagsSetPayload>(
            new RoleTagsAddedMessage
            {
                Id = roleId,
                Tags = tags
            },
            WellKnownTicketOperations.CreateRoleTags,
            cancellationToken,
            null,
            roleId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateFunctionTagsAsync(
        string functionId,
        TagAssignment[] tags,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<FunctionTagsAddedMessage, TagsSetPayload>(
            new FunctionTagsAddedMessage
            {
                Id = functionId,
                Tags = tags
            },
            WellKnownTicketOperations.CreateFunctionTags,
            cancellationToken,
            null,
            functionId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteTagsFromProfile(
        string profileId,
        ProfileKind profileKind,
        string[] tagIds,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string operation = profileKind switch
        {
            ProfileKind.Group => WellKnownTicketOperations.DeleteTagsFromGroup,
            ProfileKind.User => WellKnownTicketOperations.DeleteTagsFromUser,
            ProfileKind.Organization => WellKnownTicketOperations.DeleteTagsFromOrganization,
            ProfileKind.Unknown => throw new ArgumentException("Unsupported profile kind: Unknown", nameof(profileKind)),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };

        string ticketId = await PublishMessageWithTicket<ProfileTagsRemovedMessage, TagsRemovedPayload>(
            new ProfileTagsRemovedMessage
            {
                ResourceId = profileId,
                ProfileKind = profileKind,
                Tags = tagIds
            },
            operation,
            cancellationToken,
            null,
            profileId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteTagsFromRole(
        string roleId,
        string[] tagIds,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<RoleTagsRemovedMessage, TagsRemovedPayload>(
            new RoleTagsRemovedMessage
            {
                ResourceId = roleId,
                Tags = tagIds
            },
            WellKnownTicketOperations.DeleteTagsFromRole,
            cancellationToken,
            null,
            roleId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteTagsFromFunction(
        string functionId,
        string[] tagIds,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId = await PublishMessageWithTicket<FunctionTagsRemovedMessage, TagsRemovedPayload>(
            new FunctionTagsRemovedMessage
            {
                ResourceId = functionId,
                Tags = tagIds
            },
            WellKnownTicketOperations.DeleteTagsFromFunction,
            cancellationToken,
            null,
            functionId);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> SetProfileConfiguration(
        string profileId,
        ProfileKind profileKind,
        string configKey,
        JObject configurationObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string operation = profileKind switch
        {
            ProfileKind.Group => WellKnownTicketOperations.SetConfigForGroup,
            ProfileKind.User => WellKnownTicketOperations.SetConfigForUser,
            ProfileKind.Organization => WellKnownTicketOperations.SetConfigForOrganization,
            ProfileKind.Unknown => throw new ArgumentException("Unsupported profile kind: Unknown", nameof(profileKind)),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };

        string ticketId = await PublishMessageWithTicket<ProfileClientSettingsSetMessage, ClientSettingsSetPayload>(
            new ProfileClientSettingsSetMessage
            {
                Resource = new ProfileIdent(profileId, profileKind),
                Key = configKey,
                Settings = configurationObject
            },
            operation,
            cancellationToken,
            null,
            profileId,
            configKey);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> SetProfilesConfiguration(
        string configKey,
        BatchConfigSettingsRequest configRequest,
        ProfileKind profileKind,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string ticketId =
            await PublishMessageWithTicket<ProfileClientSettingsSetBatchMessage, ClientSettingsSetBatchPayload>(
                new ProfileClientSettingsSetBatchMessage
                {
                    Resources = configRequest.ProfileIds.Select(p => new ProfileIdent(p, profileKind)).ToArray(),
                    Key = configKey,
                    Settings = JObject.Parse(configRequest.Config)
                },
                WellKnownTicketOperations.SetConfigForProfiles,
                cancellationToken,
                null,
                configKey);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateProfileConfiguration(
        string profileId,
        ProfileKind profileKind,
        string configKey,
        JsonPatchDocument configurationObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string operation = profileKind switch
        {
            ProfileKind.Group => WellKnownTicketOperations.UpdateConfigForGroup,
            ProfileKind.User => WellKnownTicketOperations.UpdateConfigForUser,
            ProfileKind.Organization => WellKnownTicketOperations.UpdateConfigForOrganization,
            ProfileKind.Unknown => throw new ArgumentException("Unsupported profile kind: Unknown", nameof(profileKind)),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };

        string ticketId =
            await PublishMessageWithTicket<ProfileClientSettingsUpdatedMessage, ClientSettingsUpdatedPayload>(
                new ProfileClientSettingsUpdatedMessage
                {
                    Resource = new ProfileIdent(profileId, profileKind),
                    Key = configKey,
                    Settings = configurationObject
                },
                operation,
                cancellationToken,
                null,
                profileId,
                configKey);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> RemoveProfileConfiguration(
        string profileId,
        ProfileKind profileKind,
        string configKey,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string operation = profileKind switch
        {
            ProfileKind.Group => WellKnownTicketOperations.RemoveConfigFromGroup,
            ProfileKind.User => WellKnownTicketOperations.RemoveConfigFromUser,
            ProfileKind.Organization => WellKnownTicketOperations.RemoveConfigFromOrganization,
            ProfileKind.Unknown => throw new ArgumentException("Unsupported profile kind: Unknown", nameof(profileKind)),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };

        string ticketId =
            await PublishMessageWithTicket<ProfileClientSettingsDeletedMessage, ClientSettingsDeletedPayload>(
                new ProfileClientSettingsDeletedMessage
                {
                    Resource = new ProfileIdent(profileId, profileKind),
                    Key = configKey
                },
                operation,
                cancellationToken,
                null,
                profileId,
                configKey);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> CreateUserSettingAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingObjects,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        var payload =
            new UserSettingsSectionCreatedMessage
            {
                // The validator needs the id, because the validator is used in another place.
                Id = Guid.NewGuid().ToString(),
                SectionName = userSettingsSectionName,
                ValuesAsJsonString = userSettingObjects,
                UserId = userId
            };

        string ticketId =
            await PublishMessageWithTicket<UserSettingsSectionCreatedMessage, UserSettingSectionCreatedPayload>(
                payload,
                WellKnownTicketOperations.CreateUserSettingSection,
                cancellationToken,
                null,
                userId,
                userSettingsSectionName);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> UpdateUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingsId,
        string userSettingsObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        var payload =
            new UserSettingObjectUpdatedMessage
            {
                // The validator needs the id, because the validator is used in another place.
                Id = Guid.NewGuid().ToString(),
                SectionName = userSettingsSectionName,
                SettingObjectId = userSettingsId,
                ValuesAsJsonString = userSettingsObject,
                UserId = userId
            };

        string ticketId =
            await PublishMessageWithTicket<UserSettingObjectUpdatedMessage, UserSettingObjectUpdatedPayload>(
                payload,
                WellKnownTicketOperations.UpdateUserSettingObject,
                cancellationToken,
                null,
                userId,
                userSettingsSectionName);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteSettingsSectionForUserAsync(
        string userId,
        string userSettingSectionName,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        var payload =
            new UserSettingSectionDeletedMessage
            {
                // The validator needs the id, because the validator is used in another place.
                Id = Guid.NewGuid().ToString(),
                SectionName = userSettingSectionName,
                UserId = userId
            };

        string ticketId =
            await PublishMessageWithTicket<UserSettingSectionDeletedMessage, UserSettingSectionDeletedPayload>(
                payload,
                WellKnownTicketOperations.DeleteUserSettingSection,
                cancellationToken,
                null,
                userId,
                userSettingSectionName);

        return _logger.ExitMethod<string>(ticketId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteUserSettingsAsync(
        string userId,
        string userSettingSectionName,
        string userSettingsId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        var payload =
            new UserSettingObjectDeletedMessage
            {
                // The validator needs the id, because the validator is used in another place.
                Id = Guid.NewGuid().ToString(),
                SectionName = userSettingSectionName,
                UserId = userId,
                SettingObjectId = userSettingsId
            };

        string ticketId =
            await PublishMessageWithTicket<UserSettingObjectDeletedMessage, UserSettingObjectDeletedPayload>(
                payload,
                WellKnownTicketOperations.DeleteUserSettingObject,
                cancellationToken,
                null,
                userId,
                userSettingSectionName);

        return _logger.ExitMethod<string>(ticketId);
    }
}
