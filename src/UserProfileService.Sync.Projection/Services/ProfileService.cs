using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Stores;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Services;

/// <summary>
///     Default implementation of <see cref="IProfileService" />.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IEntityStore _entityStore;
    private readonly ILogger<ProfileService> _logger;
    private readonly IProjectionStateRepository _projectionStateRepository;

    /// <summary>
    ///     Create an instance of <see cref="ProfileService" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="entityStore">The store to handle user operations.</param>
    /// ///
    /// <param name="projectionStateRepository">The store to access information about projection state.</param>
    public ProfileService(
        ILogger<ProfileService> logger,
        IEntityStore entityStore,
        IProjectionStateRepository projectionStateRepository)
    {
        _logger = logger;
        _entityStore = entityStore;
        _projectionStateRepository = projectionStateRepository;
    }

    /// <inheritdoc />
    public async Task<TProfile> CreateProfileAsync<TProfile>(
        TProfile profile,
        CancellationToken cancellationToken = default) where TProfile : ISyncProfile
    {
        _logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Try to create profile of type {type} with data: {data}",
                LogHelpers.Arguments(profile.Kind, profile.ToLogString().AsArgumentList()));
        }
        else
        {
            _logger.LogInfoMessage(
                "Try to create profile of type {type} with id {id}.",
                LogHelpers.Arguments(profile.Kind, profile.Id.AsArgumentList()));
        }

        TProfile createdUser = await _entityStore.CreateProfileAsync(profile, cancellationToken);

        _logger.LogInfoMessage("Successful created user with id {id}.", profile.Id.AsArgumentList());

        return _logger.ExitMethod(createdUser);
    }

    /// <inheritdoc />
    public async Task<TProfile> GetProfileAsync<TProfile>(
        string profileId,
        CancellationToken cancellationToken = default) where TProfile : ISyncProfile
    {
        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Id cannot be null or whitespace.", nameof(profileId));
        }

        var profile = await _entityStore.GetProfileAsync<TProfile>(profileId, cancellationToken);

        if (profile == null)
        {
            throw new InstanceNotFoundException(
                ErrorCodes.ProfileNotFoundString,
                $"No profile found with id '{profileId}'.");
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Successful got profile of type {type} with data: {data}",
                LogHelpers.Arguments(profile.Id, profile.ToLogString()));
        }
        else
        {
            _logger.LogInfoMessage(
                "Successful got profile of type {type} with id {id}.",
                LogHelpers.Arguments(profile.Kind.ToString(), profile.Id));
        }

        return _logger.ExitMethod(profile);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<UserSync>> GetUsersAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();
        IPaginatedList<UserSync> result = await _entityStore.GetUsersAsync(options, cancellationToken);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<GroupSync>> GetGroupsAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();
        IPaginatedList<GroupSync> result = await _entityStore.GetGroupsAsync(options, cancellationToken);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<OrganizationSync>> GetOrganizationsAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IPaginatedList<OrganizationSync> result =
            await _entityStore.GetOrganizationsAsync(options, cancellationToken);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<TProfile> UpdateProfileAsync<TProfile>(
        TProfile profile,
        CancellationToken cancellationToken = default) where TProfile : ISyncProfile
    {
        _logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new ArgumentException("The profile Id should not be null, empty or whitespace", nameof(profile));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Try to update profile with id {id} and data: {data}",
                LogHelpers.Arguments(profile.Id, profile.ToLogString()));
        }
        else
        {
            _logger.LogInfoMessage("Try to update profile with id {id}.", profile.Id.AsArgumentList());
        }

        profile = await _entityStore.UpdateProfileAsync(profile, cancellationToken);

        _logger.LogInfoMessage("Successful updated profile with id {id}.", profile.Id.AsArgumentList());

        return _logger.ExitMethod(profile);
    }

    /// <inheritdoc />
    public async Task DeleteProfileAsync<TProfile>(string id, CancellationToken cancellationToken = default)
        where TProfile : ISyncProfile
    {
        _logger.EnterMethod();

        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Profile id should not be empty or whitespace", nameof(id));
        }

        await _entityStore.DeleteProfileAsync<TProfile>(id, cancellationToken);
        _logger.LogInfoMessage("Successful deleted user with id {id}.", id.AsArgumentList());
        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<bool> TrySaveProjectionStateAsync(
        ProjectionState projectionState,
        IDatabaseTransaction transaction = null,
        ILogger logger = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (projectionState == null)
        {
            throw new ArgumentNullException(nameof(projectionState));
        }

        bool result = await _projectionStateRepository.TrySaveProjectionStateAsync(
            projectionState,
            transaction,
            logger,
            cancellationToken);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, ulong>> GetLatestProjectedEventIdsAsync(
        CancellationToken stoppingToken = default)
    {
        _logger.EnterMethod();

        return _logger.ExitMethod(await _projectionStateRepository.GetLatestProjectedEventIdsAsync(stoppingToken));
    }

    /// <inheritdoc />
    public async Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        return _logger.ExitMethod(
            await _projectionStateRepository.GetPositionOfLatestProjectedEventAsync(cancellationToken));
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<TRole>> GetRolesAsync<TRole>(
        QueryObject options = null,
        CancellationToken cancellationToken = default) where TRole : RoleSync
    {
        _logger.EnterMethod();

        IPaginatedList<TRole> result = await _entityStore.GetRolesAsync<TRole>(options, cancellationToken);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<RoleSync> CreateRoleAsync(RoleSync role, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Try to create role with data: {data}",
                LogHelpers.Arguments(role.ToLogString().AsArgumentList()));
        }
        else
        {
            _logger.LogInfoMessage("Try to create role with id {id}.", LogHelpers.Arguments(role.Id.AsArgumentList()));
        }

        RoleSync createdRole = await _entityStore.CreateRoleAsync(role, cancellationToken);

        _logger.LogInfoMessage("Successful created role with id {id}.", role.Id.AsArgumentList());

        return _logger.ExitMethod(createdRole);
    }

    /// <inheritdoc />
    public async Task<RoleSync> UpdateRoleAsync(RoleSync role, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (string.IsNullOrWhiteSpace(role.Id))
        {
            throw new ArgumentException("The role Id should not be null, empty or whitespace", nameof(role));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Try to update role with id {id} and data: {data}",
                LogHelpers.Arguments(role.Id, role.ToLogString()));
        }
        else
        {
            _logger.LogInfoMessage("Try to update role with id {id}.", role.Id.AsArgumentList());
        }

        RoleSync updatedRole = await _entityStore.UpdateRoleAsync(role, cancellationToken);

        _logger.LogInfoMessage("Successful updated role with id {id}.", role.Id.AsArgumentList());

        return _logger.ExitMethod(updatedRole);
    }

    /// <inheritdoc />
    public async Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (roleId == null)
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException("role id should not be empty or whitespace", nameof(roleId));
        }

        await _entityStore.DeleteRoleAsync(roleId, cancellationToken);
        _logger.LogInfoMessage("Successful deleted user with id {id}.", roleId.AsArgumentList());
        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<RoleSync> GetRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        if (roleId == null)
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException("Id cannot be null or whitespace.", nameof(roleId));
        }

        RoleSync role = await _entityStore.GetRoleAsync(roleId, cancellationToken);

        if (role == null)
        {
            throw new InstanceNotFoundException(
                ErrorCodes.RoleNotFoundString,
                $"No role found with id '{roleId}'.");
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Successful got profile of type {type} with data: {data}",
                LogHelpers.Arguments(role.Id, role.ToLogString()));
        }
        else
        {
            _logger.LogInfoMessage(
                "Successful got role with id {id}.",
                LogHelpers.Arguments(role.Id));
        }

        return _logger.ExitMethod(role);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<TFunction>> GetFunctionsAsync<TFunction>(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default) where TFunction : FunctionSync
    {
        _logger.EnterMethod();

        IPaginatedList<TFunction> result = await _entityStore.GetFunctionsAsync<TFunction>(options, cancellationToken);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<FunctionSync> CreateFunctionAsync(
        FunctionSync function,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Try to create function with data: {data}",
                LogHelpers.Arguments(function.ToLogString().AsArgumentList()));
        }
        else
        {
            _logger.LogInfoMessage(
                "Try to create function with id {id}.",
                LogHelpers.Arguments(function.Id.AsArgumentList()));
        }

        FunctionSync createdFunction = await _entityStore.CreateFunctionAsync(function, cancellationToken);

        _logger.LogInfoMessage("Successful created function with id {id}.", function.Id.AsArgumentList());

        return _logger.ExitMethod(createdFunction);
    }

    /// <inheritdoc />
    public async Task<FunctionSync> UpdateFunctionAsync(
        FunctionSync function,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (string.IsNullOrWhiteSpace(function.Id))
        {
            throw new ArgumentException("The function Id should not be null, empty or whitespace", nameof(function));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Try to update function with id {id} and data: {data}",
                LogHelpers.Arguments(function.Id, function.ToLogString()));
        }
        else
        {
            _logger.LogInfoMessage("Try to update function with id {id}.", function.Id.AsArgumentList());
        }

        function = await _entityStore.UpdateFunctionAsync(function, cancellationToken);

        _logger.LogInfoMessage("Successful updated function with id {id}.", function.Id.AsArgumentList());

        return _logger.ExitMethod(function);
    }

    /// <inheritdoc />
    public async Task DeleteFunctionAsync(string functionId, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (functionId == null)
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException("Profile id should not be empty or whitespace", nameof(functionId));
        }

        await _entityStore.DeleteFunctionAsync(functionId, cancellationToken);
        _logger.LogInfoMessage("Successful deleted user with id {id}.", functionId.AsArgumentList());
        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<FunctionSync> GetFunctionAsync(string functionId, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (functionId == null)
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException("Id cannot be null or whitespace.", nameof(functionId));
        }

        FunctionSync function = await _entityStore.GetFunctionAsync(functionId, cancellationToken);

        if (function == null)
        {
            throw new InstanceNotFoundException(
                ErrorCodes.FunctionNotFoundString,
                $"No function found with id '{functionId}'.");
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Successful got function with data: {data}",
                LogHelpers.Arguments(function.ToLogString()));
        }
        else
        {
            _logger.LogInfoMessage(
                "Successful got function with id {id}.",
                LogHelpers.Arguments(function.Id));
        }

        return _logger.ExitMethod(function);
    }
}
