using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.Validation.Abstractions;
using UserProfileService.Validation.Abstractions.Configuration;
using FunctionCreatedPayloadV3 = UserProfileService.Events.Payloads.V3.FunctionCreatedPayload;
using UserCreatedPayloadV3 = UserProfileService.Events.Payloads.V3.UserCreatedPayload;

namespace UserProfileService.Saga.Validation;

internal class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private readonly IPayloadValidationService _payloadValidationService;
    private readonly IRepoValidationService _repoValidationService;
    private readonly ValidationConfiguration _validationConfiguration;
    private readonly IVolatileRepoValidationService _volatileRepoValidationService;

    public ValidationService(
        ILogger<ValidationService> logger,
        IPayloadValidationService payloadValidationService,
        IRepoValidationService repoValidationService,
        IOptions<ValidationConfiguration> validationOptions,
        IVolatileRepoValidationService volatileRepoValidationService)
    {
        _logger = logger;
        _payloadValidationService = payloadValidationService;
        _repoValidationService = repoValidationService;
        _volatileRepoValidationService = volatileRepoValidationService;
        _validationConfiguration = validationOptions.Value;
    }

    private async Task ValidateMessageAsync(
        FunctionCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidationResult validationResult;

        ValidateObject<FunctionCreatedPayloadV3>(message);

        if (!_validationConfiguration.Internal.Function.DuplicateAllowed)
        {
            validationResult =
                await _repoValidationService.ValidateDuplicateFunctionAsync(
                    message.RoleId,
                    message.OrganizationId,
                    cancellationToken);

            validationResult.CheckAndThrowException();
        }

        validationResult =
            await _repoValidationService.ValidateRoleExistsAsync(
                message.RoleId,
                nameof(message.RoleId),
                cancellationToken);

        validationResult.CheckAndThrowException();

        validationResult = await _repoValidationService.ValidateOrganizationExistsAsync(
            message.OrganizationId,
            nameof(message.OrganizationId),
            cancellationToken);

        validationResult.CheckAndThrowException();

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        FunctionDeletedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<IdentifierPayload>(message);

        ValidationResult validationResult =
            await _repoValidationService.ValidateFunctionExistsAsync(
                message.Id,
                nameof(message.Id),
                cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        FunctionTagsAddedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<TagsSetPayload>(message);

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        ValidationResult validationResult =
            await _repoValidationService.ValidateFunctionExistsAsync(
                message.Id,
                nameof(message.Id),
                cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        FunctionTagsRemovedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<TagsRemovedPayload>(message);

        ValidationResult validationResult =
            await _repoValidationService.ValidateFunctionExistsAsync(
                message.ResourceId,
                nameof(message.ResourceId),
                cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        GroupCreatedMessage message,
        Initiator initiator = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        GroupCreatedPayload payload = message;

        ValidateObject(payload);

        // allow system to skip custom validation
        if (initiator?.Type != InitiatorType.System)
        {
            string groupNameRegex = _validationConfiguration.Internal.Group.Name.Regex;

            if (!string.IsNullOrWhiteSpace(groupNameRegex))
            {
                ValidationResult validationResult = Validator.Group.ValidateNames(
                    payload.Name,
                    payload.DisplayName,
                    groupNameRegex,
                    nameof(payload.Name),
                    nameof(payload.DisplayName));

                validationResult.CheckAndThrowException();
            }

            if (!_validationConfiguration.Internal.Group.Name.Duplicate
                && (!string.IsNullOrWhiteSpace(payload.Name) || !string.IsNullOrWhiteSpace(payload.DisplayName)))
            {
                ValidationResult validationResult = await _repoValidationService.ValidateGroupExistsAsync(
                    payload.Name,
                    payload.DisplayName,
                    null,
                    _validationConfiguration.Internal.Group.Name.IgnoreCase,
                    nameof(payload.Name),
                    nameof(payload.DisplayName),
                    cancellationToken);

                validationResult.CheckAndThrowException();
            }
        }

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        // Check whether the given profiles are valid and
        // whether the corresponding profiles are also available in the database. 
        ValidationResult objectsValidationResult =
            await _repoValidationService.ValidateObjectsExistAsync(
                payload.Members.ToArray<IObjectIdent>(),
                nameof(payload.Members),
                cancellationToken);

        objectsValidationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        ObjectAssignmentMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<AssignmentPayload>(message);

        ValidationResult validationResult = await _repoValidationService.ValidateObjectExistsAsync(
            message.Resource,
            nameof(message.Resource),
            cancellationToken);

        validationResult.CheckAndThrowException();

        // Check whether the given profiles are valid and
        // whether the corresponding profiles are also available in the database. 
        ValidationResult addedObjectsValidationResult =
            await _repoValidationService.ValidateObjectsExistAsync(
                message.Added.ToArray<IObjectIdent>(),
                nameof(message.Added),
                cancellationToken);

        ValidationResult removedObjectsValidationResult =
            await _repoValidationService.ValidateObjectsExistAsync(
                message.Removed.ToArray<IObjectIdent>(),
                nameof(message.Removed),
                cancellationToken);

        if (!addedObjectsValidationResult.IsValid || !removedObjectsValidationResult.IsValid)
        {
            List<ValidationAttribute> conceitedErrors = addedObjectsValidationResult.Errors
                .Concat(removedObjectsValidationResult.Errors)
                .ToList();

            throw new ValidationException(conceitedErrors);
        }

        // Validate the type of assignment between source and target objects.
        ValidationResult assignmentValidationResult =
            ValidateAssignment(nameof(message.Added), message, x => x.Added);

        assignmentValidationResult.CheckAndThrowException();

        ValidationResult containerProfileResult =
            await _repoValidationService.ValidateContainerProfileAssignmentGraphAsync(
                message.Resource,
                message.Added,
                nameof(message.Added),
                cancellationToken);

        containerProfileResult.CheckAndThrowException();

        // Validate the type of assignment between source and target objects.
        assignmentValidationResult =
            ValidateAssignment(nameof(message.Removed), message, x => x.Removed);

        assignmentValidationResult.CheckAndThrowException();

        // TODO: Check organization assignment business logic.

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        OrganizationCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<OrganizationCreatedPayload>(message);

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        // Check whether the given profiles are valid and
        // whether the corresponding profiles are also available in the database. 
        ValidationResult objectsValidationResult =
            await _repoValidationService.ValidateObjectsExistAsync(
                message.Members.ToList<IObjectIdent>(),
                nameof(message.Members),
                cancellationToken);

        objectsValidationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        ProfileClientSettingsDeletedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<ClientSettingsDeletedPayload>(message);

        ValidationResult<JObject> validationResult =
            await _repoValidationService.ValidateClientSettingsExistsAsync(
                message.Resource,
                message.Key,
                nameof(message.Key),
                cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        ProfileClientSettingsSetBatchMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<ClientSettingsSetBatchPayload>(message);

        // Check whether the given profiles are valid and
        // whether the corresponding profiles are also available in the database. 

        List<IObjectIdent> resources = message.Resources.Select(r => r.ToObjectIdent()).ToList<IObjectIdent>();

        ValidationResult profilesValidationResult =
            await _repoValidationService.ValidateObjectsExistAsync(
                resources,
                nameof(message.Resources),
                cancellationToken);

        profilesValidationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        ProfileClientSettingsSetMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<ClientSettingsSetPayload>(message);

        ValidationResult validationResult = await _repoValidationService.ValidateObjectExistsAsync(
            message.Resource.ToObjectIdent(),
            nameof(message.Resource),
            cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        ProfileClientSettingsUpdatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<ClientSettingsUpdatedPayload>(message);

        ValidationResult<JObject> validationResult =
            await _repoValidationService.ValidateClientSettingsExistsAsync(
                message.Resource,
                message.Key,
                nameof(message.Key),
                cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }
    
    private async Task ValidateMessageAsync(
        ProfilePropertiesChangedMessage message,
        Initiator initiator = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidationResult<IProfile> validationResultObjectExists =
            await _repoValidationService.ValidateProfileExistsAsync(
                new ProfileIdent(message.Id, message.ProfileKind),
                nameof(message.Id),
                cancellationToken);

        validationResultObjectExists.CheckAndThrowException();

        // If the profile kind is undefined, the profile kind is overwritten with the one from the database.
        message.ProfileKind = validationResultObjectExists.Facade.Kind;

        ValidationResult validationResult = message.ProfileKind switch
        {
            ProfileKind.Group => _payloadValidationService
                .ValidateUpdateObjectProperties<GroupBasic>(message),

            ProfileKind.User => _payloadValidationService
                .ValidateUpdateObjectProperties<UserBasic>(message),
            ProfileKind.Organization => _payloadValidationService
                .ValidateUpdateObjectProperties<OrganizationBasic>(message),
            _ => throw new ArgumentOutOfRangeException(nameof(message.ProfileKind))
        };

        validationResult.CheckAndThrowException();

        if (initiator?.Type != InitiatorType.System)
        {
            if (CheckToRunEmailDuplicateCheck(message, out string email))
            {
                validationResult =
                    await _repoValidationService.ValidateUserEmailExistsAsync(
                        email,
                        message.Id,
                        nameof(UserBasic.Email),
                        cancellationToken);

                validationResult.CheckAndThrowException();
            }

            string groupNameRegex = _validationConfiguration.Internal.Group.Name.Regex;

            if (message.ProfileKind == ProfileKind.Group && !string.IsNullOrWhiteSpace(groupNameRegex))
            {
                string groupName = message.Properties.TryGetValue(nameof(GroupBasic.Name), out object property)
                    ? (string)property
                    : null;

                string groupDisplayName = message.Properties.TryGetValue(nameof(GroupBasic.DisplayName), out object messageProperty)
                    ? (string)messageProperty
                    : null;

                validationResult = Validator.Group.ValidateNames(
                    groupName,
                    groupDisplayName,
                    groupDisplayName,
                    nameof(GroupBasic.Name),
                    nameof(GroupBasic.DisplayName));

                validationResult.CheckAndThrowException();
            }

            if (CheckToRunNameDuplicateCheck(message, out string name, out string displayName))
            {
                validationResult = await _repoValidationService.ValidateGroupExistsAsync(
                    name ?? displayName,
                    displayName ?? name,
                    message.Id,
                    _validationConfiguration.Internal.Group.Name.IgnoreCase,
                    nameof(GroupBasic.Name),
                    nameof(GroupBasic.DisplayName),
                    cancellationToken);

                validationResult.CheckAndThrowException();
            }
        }

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        ProfileTagsAddedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<TagsSetPayload>(message);

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        ValidationResult<IProfile> validationResultObjectExists =
            await _repoValidationService.ValidateProfileExistsAsync(
                new ProfileIdent(message.Id, message.ProfileKind),
                nameof(message.Id),
                cancellationToken);

        validationResultObjectExists.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        ProfileTagsRemovedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<TagsRemovedPayload>(message);

        ValidationResult<IProfile> validationResultObjectExists =
            await _repoValidationService.ValidateProfileExistsAsync(
                new ProfileIdent(message.ResourceId, message.ProfileKind),
                nameof(message.ResourceId),
                cancellationToken);

        validationResultObjectExists.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        RoleCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<RoleCreatedPayload>(message);

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        RoleDeletedMessage message,
        Initiator initiator = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<IdentifierPayload>(message);

        ValidationResult<RoleBasic> validationResultRole =
            await _repoValidationService.ValidateRoleExistsAsync(message.Id, nameof(message.Id), cancellationToken);

        validationResultRole.CheckAndThrowException();

        RoleBasic repoRole = validationResultRole.Facade;

        ValidationResult validationResultSystem = Validator.Profile.ValidateOperationAllowed(
            repoRole.IsSystem,
            initiator?.Type,
            nameof(RoleBasic.IsSystem));

        validationResultSystem.CheckAndThrowException();

        // Check if assignment with a target/source function exists.

        ValidationResult validationResult =
            await _repoValidationService.ValidateRoleAssignmentsAsync(
                message.Id,
                cancellationToken: cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        RolePropertiesChangedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidationResult validationResultProperties =
            _payloadValidationService.ValidateUpdateObjectProperties<RoleBasic>(message);

        validationResultProperties.CheckAndThrowException();

        ValidationResult<RoleBasic> validationResult =
            await _repoValidationService.ValidateRoleExistsAsync(message.Id, nameof(message.Id), cancellationToken);

        validationResult.CheckAndThrowException();

        RoleBasic role = validationResult.Facade;

        // The following region checks the permissions as well as denied permissions.
        // It checks if the permissions override each other and
        // if the new permissions conflict with already saved permissions in the database. 

        #region Check Permissions

        bool permissionsSet = message.Properties.TryGetValue(
                nameof(RoleBasic.Permissions),
                out object permissionsObject)
            && permissionsObject != null;

        IList<string> permissions =
            permissionsSet
                ? permissionsObject.TryConvertObject<IList<string>>() ?? new List<string>()
                : role.Permissions;

        bool deniedPermissionsSet = message.Properties.TryGetValue(
                nameof(RoleBasic.DeniedPermissions),
                out object deniedPermissionsObject)
            && deniedPermissionsObject != null;

        IList<string> deniedPermissions =
            deniedPermissionsSet
                ? deniedPermissionsObject.TryConvertObject<IList<string>>() ?? new List<string>()
                : role.DeniedPermissions;

        // Remove all null or empty permissions
        permissions = permissions.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        deniedPermissions = deniedPermissions.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

        // Check if any denied permission is granted.
        List<string> invalidPermissions = deniedPermissions.Where(dp => permissions.Contains(dp)).ToList();

        if (invalidPermissions.Any())
        {
            var addInfos = new Dictionary<string, object>
            {
                { "Permissions", invalidPermissions }
            };

            ValidationAttribute validationResultDenied = deniedPermissionsSet
                ? new ValidationAttribute(
                    nameof(RoleBasic.DeniedPermissions),
                    "The same permissions are already granted to the role directly.",
                    addInfos)
                : new ValidationAttribute(
                    nameof(RoleBasic.Permissions),
                    "The same permissions are already denied or rejected to the role directly.",
                    addInfos);

            throw new ValidationException(validationResultDenied);
        }

        if (permissionsSet)
        {
            message.Properties[nameof(RoleBasic.Permissions)] = permissions;
        }

        if (deniedPermissionsSet)
        {
            message.Properties[nameof(RoleBasic.DeniedPermissions)] = deniedPermissions;
        }

        #endregion

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        RoleTagsAddedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<TagsSetPayload>(message);

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        ValidationResult<RoleBasic> validationResult =
            await _repoValidationService.ValidateRoleExistsAsync(message.Id, nameof(message.Id), cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        RoleTagsRemovedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<TagsRemovedPayload>(message);

        ValidationResult<RoleBasic> validationResult =
            await _repoValidationService.ValidateRoleExistsAsync(
                message.ResourceId,
                nameof(message.ResourceId),
                cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        TagCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        await Task.Yield();

        cancellationToken.ThrowIfCancellationRequested();
        
        ValidateObject<TagCreatedPayload>(message);

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        TagDeletedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<IdentifierPayload>(message);

        ValidationResult<Tag> validationResult =
            await _repoValidationService.ValidateTagExistsAsync(message.Id, nameof(message.Id), cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        UserCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<UserCreatedPayloadV3>(message);

        await ValidateTagAssignmentsAsync(message.Tags, nameof(message.Tags), cancellationToken);

        // Set email to lower case.
        if (!_validationConfiguration.Internal.User.DuplicateEmailAllowed
            && !string.IsNullOrWhiteSpace(message.Email))
        {
            ValidationResult validationResult = await _repoValidationService.ValidateUserEmailExistsAsync(
                message.Email,
                null,
                nameof(message.Email),
                cancellationToken);

            validationResult.CheckAndThrowException();
        }

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        UserSettingsSectionCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<UserSettingSectionCreatedPayload>(message);

        ValidationResult validationResult = await _volatileRepoValidationService.ValidateProfileExistsAsync(
            message.UserId,
            cancellationToken: cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        UserSettingObjectUpdatedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<UserSettingObjectUpdatedPayload>(message);

        ValidationResult validationResultUserExists = await _volatileRepoValidationService.ValidateProfileExistsAsync(
            message.UserId,
            cancellationToken: cancellationToken);

        validationResultUserExists.CheckAndThrowException();

        ValidationResult validationResultUserSettingsExists =
            await _volatileRepoValidationService.ValidateUserSettingObjectExistsAsync(
                message.UserId,
                message.SectionName,
                message.SettingObjectId,
                cancellationToken);

        validationResultUserSettingsExists.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        UserSettingObjectDeletedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<UserSettingObjectDeletedPayload>(message);

        ValidationResult validationResultUserExists = await _volatileRepoValidationService.ValidateProfileExistsAsync(
            message.UserId,
            cancellationToken: cancellationToken);

        validationResultUserExists.CheckAndThrowException();

        ValidationResult validationResultUserSettingsExists =
            await _volatileRepoValidationService.ValidateUserSettingObjectExistsAsync(
                message.UserId,
                message.SectionName,
                message.SettingObjectId,
                cancellationToken);

        validationResultUserSettingsExists.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateMessageAsync(
        UserSettingSectionDeletedMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ValidateObject<UserSettingSectionDeletedPayload>(message);

        ValidationResult validationResultUserExists = await _volatileRepoValidationService.ValidateProfileExistsAsync(
            message.UserId,
            cancellationToken: cancellationToken);

        validationResultUserExists.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private async Task ValidateTagAssignmentsAsync(
        TagAssignment[] tags,
        string member,
        CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        if (!tags.Any())
        {
            return;
        }

        List<string> tagIds = tags.Select(t => t.TagId).ToList();

        ValidationResult validationResult =
            await _repoValidationService.ValidateTagsExistAsync(tagIds, member, cancellationToken);

        validationResult.CheckAndThrowException();

        _logger.ExitMethod();
    }

    private bool CheckToRunEmailDuplicateCheck(ProfilePropertiesChangedMessage message, out string email)
    {
        _logger.EnterMethod();

        email = string.Empty;

        if (message.ProfileKind != ProfileKind.User)
        {
            return _logger.ExitMethod(false);
        }

        if (_validationConfiguration.Internal.User.DuplicateEmailAllowed)
        {
            return _logger.ExitMethod(false);
        }

        bool emailPropertyExist =
            message.Properties.TryGetValue(nameof(UserBasic.Email), out object emailObject);

        if (!emailPropertyExist || !(emailObject is string emailStr))
        {
            return _logger.ExitMethod(false);
        }

        email = emailStr.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            return _logger.ExitMethod(false);
        }

        message.Properties[nameof(UserBasic.Email)] = email;

        return _logger.ExitMethod(true);
    }

    private bool CheckToRunNameDuplicateCheck(
        ProfilePropertiesChangedMessage message,
        out string name,
        out string displayName)
    {
        _logger.EnterMethod();

        name = string.Empty;
        displayName = string.Empty;

        if (message.ProfileKind != ProfileKind.Group)
        {
            return _logger.ExitMethod(false);
        }

        if (_validationConfiguration.Internal.Group.Name.Duplicate)
        {
            return _logger.ExitMethod(false);
        }

        bool namePropertyExist =
            message.Properties.TryGetValue(nameof(GroupBasic.Name), out object nameObject);

        if (namePropertyExist && nameObject is string nameStr)
        {
            name = nameStr;
        }

        bool displayNamePropertyExist =
            message.Properties.TryGetValue(nameof(GroupBasic.DisplayName), out object displayNameObject);

        if (displayNamePropertyExist && displayNameObject is string displayNameStr)
        {
            displayName = displayNameStr;
        }

        bool result = displayNamePropertyExist || namePropertyExist;

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Validates the given entity using the payload validator.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to validate.</typeparam>
    /// <param name="entity">Entity to validate.</param>
    /// <exception cref="ValidationException">Exception if validation failed.</exception>
    private void ValidateObject<TEntity>(TEntity entity)
    {
        if (entity == null)
        {
            throw new ValidationException(new ValidationAttribute(string.Empty, "Payload can not be null."));
        }

        ValidationResult validationResult = _payloadValidationService.ValidateObject(entity);

        validationResult.CheckAndThrowException();
    }

    /// <summary>
    ///     validates the given assignment using the payload validator.
    /// </summary>
    /// <param name="propertyName">Name of property to use for validation result.</param>
    /// <param name="assignmentPayload">Payload to validate.</param>
    /// <param name="assSelector">Selector of assignment to validate.</param>
    /// <returns>Result of validation.</returns>
    private ValidationResult ValidateAssignment(
        string propertyName,
        AssignmentPayload assignmentPayload,
        Func<AssignmentPayload, IObjectIdent[]> assSelector)
    {
        return _payloadValidationService.ValidateAssignment(propertyName, assignmentPayload, assSelector);
    }

    /// <inheritdoc />
    public async Task ValidatePayloadAsync<TPayload>(
        TPayload payload,
        Initiator initiator = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNull(payload, nameof(payload));

        switch (payload)
        {
            case FunctionCreatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case FunctionDeletedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case FunctionTagsAddedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case FunctionTagsRemovedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case GroupCreatedMessage message:
                await ValidateMessageAsync(message, cancellationToken: cancellationToken);

                break;
            case ObjectAssignmentMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case OrganizationCreatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case ProfileClientSettingsDeletedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case ProfileClientSettingsSetBatchMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case ProfileClientSettingsSetMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case ProfileClientSettingsUpdatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case ProfilePropertiesChangedMessage message:
                await ValidateMessageAsync(
                    message,
                    initiator,
                    cancellationToken);

                break;
            case ProfileTagsAddedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case ProfileTagsRemovedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case RoleCreatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case RoleDeletedMessage message:
                await ValidateMessageAsync(
                    message,
                    initiator,
                    cancellationToken);

                break;
            case RolePropertiesChangedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case RoleTagsAddedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case RoleTagsRemovedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case TagCreatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case TagDeletedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case UserCreatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case UserSettingsSectionCreatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case UserSettingObjectUpdatedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case UserSettingObjectDeletedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            case UserSettingSectionDeletedMessage message:
                await ValidateMessageAsync(message, cancellationToken);

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(payload), "No processor defined.");
        }

        _logger.ExitMethod();
    }
}
