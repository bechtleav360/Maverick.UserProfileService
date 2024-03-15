using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Validation;

internal class RepoValidationService : IRepoValidationService
{
    private readonly ILogger<RepoValidationService> _logger;
    private readonly IValidationReadService _readService;

    public RepoValidationService(IValidationReadService readService, ILogger<RepoValidationService> logger)
    {
        _readService = readService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateDuplicateFunctionAsync(
        string roleId,
        string organizationId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(roleId, nameof(roleId));
        Guard.IsNotNullOrEmpty(organizationId, nameof(organizationId));

        ICollection<FunctionBasic> existingFunctions =
            await _readService.GetFunctionsAsync(roleId, organizationId);

        if (!existingFunctions.Any())
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResult = new ValidationAttribute(
            null,
            "Function with the given role and organization does already exists.");

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult<RoleBasic>> ValidateRoleExistsAsync(
        string roleId,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(roleId, nameof(roleId));

        RoleBasic role = await _readService.GetRoleAsync(roleId);

        if (role != null)
        {
            return _logger.ExitMethod(new ValidationResult<RoleBasic>(role));
        }

        var validationResult = new ValidationAttribute(
            member,
            "The role with the given id does not exist.");

        return _logger.ExitMethod(new ValidationResult<RoleBasic>(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateOrganizationExistsAsync(
        string organizationId,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(organizationId, nameof(organizationId));

        bool organizationExists = await _readService.CheckProfileExistsAsync(
            organizationId,
            ProfileKind.Organization);

        if (organizationExists)
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResult = new ValidationAttribute(
            member,
            "The organization with the given id does not exist.");

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateFunctionExistsAsync(
        string functionId,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(functionId, nameof(functionId));

        FunctionView functionView = await _readService.GetFunctionAsync(functionId);

        if (functionView != null)
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResult = new ValidationAttribute(
            member,
            $"The function with the given id '{functionId}' does not exist.");

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult<Tag>> ValidateTagExistsAsync(
        string tagId,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(tagId, nameof(tagId));

        Tag tag = await _readService.GetTagAsync(tagId);

        if (tag != null)
        {
            return _logger.ExitMethod(new ValidationResult<Tag>(tag));
        }

        var validationResult = new ValidationAttribute(
            member,
            $"The tag with the given id '{tagId}' does not exist.");

        return _logger.ExitMethod(new ValidationResult<Tag>(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateTagsExistAsync(
        ICollection<string> tagIds,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNull(tagIds, nameof(tagIds));

        IDictionary<string, bool> tagsExistsResults = await _readService.CheckTagsExistAsync(tagIds.ToArray());

        if (tagsExistsResults.All(t => t.Value))
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var additionalInformation = new Dictionary<string, object>
        {
            { "Ids", tagsExistsResults.Where(t => !t.Value).Select(t => t.Key) }
        };

        var validationResult = new ValidationAttribute(
            member,
            "The given tags do not exist.",
            additionalInformation);

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateGroupExistsAsync(
        string name,
        string displayName,
        string groupId = null,
        bool ignoreCase = true,
        string memberName = null,
        string memberDisplayName = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        bool duplicateName = await _readService.CheckGroupNameExistsAsync(
            name ?? displayName,
            displayName ?? name,
            ignoreCase,
            groupId);

        if (!duplicateName)
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResults = new List<ValidationAttribute>
        {
            new ValidationAttribute(
                memberName,
                "Name is already used by another group."),
            new ValidationAttribute(
                memberDisplayName,
                "Display name is already used by another group.")
        };

        return _logger.ExitMethod(new ValidationResult(validationResults));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateUserEmailExistsAsync(
        string email,
        string ignoredId,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(email, nameof(email));

        bool duplicateEmail = await _readService.CheckUserEmailExistsAsync(email, ignoredId);

        if (!duplicateEmail)
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResult = new ValidationAttribute(
            nameof(UserBasic.Email),
            $"Email is already used by another user. Used E-Mail-Address: {email}.");

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateObjectExistsAsync(
        IObjectIdent objectIdent,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNull(objectIdent, nameof(objectIdent));

        bool objectExists =
            await _readService.CheckObjectExistsAsync(objectIdent);

        if (objectExists)
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResult = new ValidationAttribute(
            member,
            $"Resource with id '{objectIdent.Id}' and type '{objectIdent.Type}' does not exists.");

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult<IProfile>> ValidateProfileExistsAsync(
        ProfileIdent profileIdent,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNull(profileIdent, nameof(profileIdent));

        IProfile result = await _readService.GetProfileAsync(profileIdent.Id, profileIdent.ProfileKind);

        if (result != null)
        {
            return new ValidationResult<IProfile>(result);
        }

        var validationResult = new ValidationAttribute(
            member,
            $"Profile with id '{profileIdent.Id}' and kind '{profileIdent.ProfileKind}' does not exists.");

        return new ValidationResult<IProfile>(validationResult);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateObjectsExistAsync(
        ICollection<IObjectIdent> objectIdents,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNull(objectIdents, nameof(objectIdents));

        // Check whether the given profiles are valid and
        // whether the corresponding profiles are also available in the database. 
        var invalidObjects = new List<IObjectIdent>();

        foreach (IObjectIdent objectIdent in objectIdents)
        {
            ValidationResult objectIdentValidationResult = await ValidateObjectExistsAsync(
                objectIdent,
                cancellationToken: cancellationToken);

            if (!objectIdentValidationResult.IsValid)
            {
                invalidObjects.Add(objectIdent);
            }
        }

        if (!invalidObjects.Any())
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var addInfos = new Dictionary<string, object>
        {
            { "Ids", invalidObjects }
        };

        var validationResult =
            new ValidationAttribute(
                member,
                "The given objects are invalid or do not exists.",
                addInfos);

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult<JObject>> ValidateClientSettingsExistsAsync(
        ProfileIdent profile,
        string key,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNull(profile, nameof(profile));
        Guard.IsNotNullOrEmpty(key, nameof(key));

        JObject config =
            await _readService.GetSettingsOfProfileAsync(
                profile.Id,
                profile.ProfileKind,
                key);

        if (config is { HasValues: true })
        {
            return _logger.ExitMethod(new ValidationResult<JObject>(config));
        }

        var validationResult = new ValidationAttribute(
            member,
            $"The profile settings with the given id '{key}' does not exist.");

        return _logger.ExitMethod(new ValidationResult<JObject>(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateContainerProfileAssignmentGraphAsync(
        IObjectIdent objectIdent,
        ICollection<ConditionObjectIdent> assignments,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNull(objectIdent, nameof(objectIdent));
        Guard.IsNotNull(assignments, nameof(assignments));

        // Check that the profile ids are the same as the container profile to which the profiles are to be added
        // and that a recursive hierarchy is created by the assignment. 
        if (!objectIdent.Type.IsContainerProfileType() || !assignments.Any(o => o.Type.IsContainerProfileType()))
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        string[] parents = await _readService.GetAllParentsOfProfile(objectIdent.Id);

        List<ConditionObjectIdent> conflictProfiles = assignments
            .Where(
                profileIdent =>
                    parents.Contains(profileIdent.Id)
                    || objectIdent.Id == profileIdent.Id)
            .ToList();

        if (!conflictProfiles.Any())
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        _logger.LogErrorMessage(
            null,
            "Profile with ids '{ids}' cannot be assigned recursively by itself due to the hierarchy.",
            LogHelpers.Arguments(string.Join(",", conflictProfiles.Select(i => i.Id))));

        var addInfos = new Dictionary<string, object>
        {
            { "Ids", conflictProfiles }
        };

        var validationResult = new ValidationAttribute(
            member,
            "The given profile ids cannot be assigned recursively by itself due to the hierarchy.",
            addInfos);

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateRoleAssignmentsAsync(
        string id,
        string member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(id, nameof(id));

        string[] functionAssignments = await _readService.GetRoleFunctionAssignmentsAsync(id);

        if (!functionAssignments.Any())
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var addInfo = new Dictionary<string, object>
        {
            { "FunctionIds", functionAssignments }
        };

        var validationResult =
            new ValidationAttribute(
                member,
                "Function assignments for role exists.",
                addInfo);

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateAssignmentsExistAsync(
        IObjectIdent objectIdent,
        IList<ConditionObjectIdent> assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();
        
        Guard.IsNotNull(objectIdent, nameof(objectIdent));
        Guard.IsNotNull(assignments, nameof(assignments));

        if (!objectIdent.Type.IsContainerProfileType())
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        IList<ConditionAssignment> missing = await _readService.CheckExistingProfileAssignmentsAsync(
            objectIdent.Id,
            GetProfileKind(objectIdent.Type),
            assignments,
            cancellationToken);

        return new ValidationResult
        {
            Errors = missing.Select(
                    a =>
                        new ValidationAttribute(
                            "Members",
                            "Member assignment missing",
                            new Dictionary<string, object>
                            {
                                {
                                    $"{objectIdent.Type:G} with id equals {objectIdent.Id} misses member assignment",
                                    GenerateDetailedMessage(a)
                                }
                            }))
                .ToArray()
        };

        static string GenerateDetailedMessage(ConditionAssignment a)
        {
            RangeCondition firstCondition = a.Conditions?.FirstOrDefault();

            if (firstCondition == null)
            {
                return $"child '{a.Id}' not assigned on condition = NULL";
            }

            string start = firstCondition.Start?.ToString("u") ?? "NULL";
            string end = firstCondition.End?.ToString("u") ?? "NULL";

            return $"child '{a.Id}' not assigned on condition = [start: {start}, end: {end}]";
        }
    }

    private static ProfileKind GetProfileKind(ObjectType type)
    {
        return type switch
        {
            ObjectType.Group => ProfileKind.Group,
            ObjectType.User => ProfileKind.User,
            ObjectType.Organization => ProfileKind.Organization,
            _ => ProfileKind.Unknown
        };
    }
}
