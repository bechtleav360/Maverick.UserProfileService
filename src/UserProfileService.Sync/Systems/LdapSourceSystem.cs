using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Models.Results;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Configuration;
using UserProfileService.Sync.Extensions.Ldap;
using UserProfileService.Sync.Models;

namespace UserProfileService.Sync.Systems;

/// <summary>
///     The Implementation of the synchronization for active ldap as
///     source system.
/// </summary>
[System(SyncConstants.System.Ldap)]
public class LdapSourceSystem : ISynchronizationSourceSystem<UserSync>, ISynchronizationSourceSystem<GroupSync>
{
    private readonly LdapSystemConfiguration _ldapSystemConfiguration;
    private readonly ILogger<LdapSourceSystem> _logger;

    /// <summary>
    ///     Create an instance of <see cref="LdapSourceSystem" />
    /// </summary>
    /// <param name="ldapConfiguration">Active directory configuration to connect to target system.</param>
    /// <param name="logger">The logger is used for logging purposes.</param>
    public LdapSourceSystem(IOptionsSnapshot<LdapSystemConfiguration> ldapConfiguration, ILogger<LdapSourceSystem> logger)
    {
        _logger = logger;
        _ldapSystemConfiguration = ldapConfiguration?.Value;
    }

    /// <inheritdoc />
    Task ISynchronizationSourceSystem<GroupSync>.DeleteEntity(string sourceId, CancellationToken token)
    {
        _logger.EnterMethod();

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    Task<IBatchResult<GroupSync>> ISynchronizationSourceSystem<GroupSync>.GetBatchAsync(
        int start,
        int batchSize,
        CancellationToken token)
    {
        _logger.EnterMethod();

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    Task ISynchronizationSourceSystem<UserSync>.DeleteEntity(string sourceId, CancellationToken token)
    {
        _logger.EnterMethod();

        throw new NotImplementedException();
    }

    private string LdapQueriesAsString(LdapQueries[] ldapQueries)
    {
        _logger.EnterMethod();

        string queryString = string.Join(
            ",",
            ldapQueries
                .Where(qu => !string.IsNullOrEmpty(qu.SearchBase))
                .Select(q => q.SearchBase)
                .ToArray());

        return _logger.ExitMethod<string>(queryString);
    }

    internal BatchResult<UserSync> GetBatchResult(List<UserSync> users, int start, int batchSize)
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "Get batch result for {count} users with start {start} and batchSize {batchSize}.",
            LogHelpers.Arguments(users.Count, start, batchSize));

        // Prevent error in GetRange(), if user count is 0
        var result = new List<UserSync>();

        if (users.Any())
        {
            result = users.Count - 1 >= start + batchSize
                ? users.GetRange(start, batchSize)
                : users.GetRange(start, users.Count - start);
        }

        var batchResult = new BatchResult<UserSync>
        {
            ErrorMessage = string.Empty,
            BatchSize = batchSize,
            StartedPosition = start,
            CurrentPosition = start + batchSize - 1,
            NextBatch = users.Count - 1 >= start + batchSize,
            Result = result
        };

        return _logger.ExitMethod(batchResult);
    }

    /// <inheritdoc />
    public Task<GroupSync> CreateEntity(GroupSync entity, CancellationToken token)
    {
        _logger.EnterMethod();

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<GroupSync> UpdateEntity(string sourceId, GroupSync entity, CancellationToken token)
    {
        _logger.EnterMethod();

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IBatchResult<UserSync>> GetBatchAsync(int start, int batchSize, CancellationToken token)
    {
        _logger.EnterMethod();

        var users = new List<UserSync>();

        if (_ldapSystemConfiguration.LdapConfiguration == null || !_ldapSystemConfiguration.LdapConfiguration.Any())
        {
            var errorMessage =
                $"Cannot find any active directory configuration in {nameof(_ldapSystemConfiguration.LdapConfiguration)}.";

            _logger.LogWarnMessage(errorMessage, LogHelpers.Arguments());

            IBatchResult<UserSync> batchErrorResult = new BatchResult<UserSync>
            {
                ErrorMessage = errorMessage
            };

            return _logger.ExitMethod(Task.FromResult(batchErrorResult));
        }

        foreach (ActiveDirectory singleConnection in _ldapSystemConfiguration.LdapConfiguration)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTraceMessage(
                    "Try to get batch for single connection in ldap: {singleConnection}",
                    LogHelpers.Arguments(JsonConvert.SerializeObject(singleConnection)));
            }
            else
            {
                _logger.LogInfoMessage(
                    "Try to get batch for single connection in ldap for connection string {connectionString}, base path {basePath} and description {description}.",
                    LogHelpers.Arguments(
                        singleConnection.Connection.ConnectionString,
                        singleConnection.Connection.BasePath,
                        singleConnection.Connection.Description));
            }

            if (string.IsNullOrWhiteSpace(singleConnection.Connection.ConnectionString))
            {
                _logger.LogWarnMessage(
                    "Connection string of active directory configuration is empty or only contains whitespace. Description: '{description}'). Skipping to next configured active directory.",
                    LogHelpers.Arguments(singleConnection.Connection.Description));
            }

            List<LdapQueries> ldapQueriesConfig = singleConnection.LdapQueries?.Where(c => c != null)
                .Where(c => !string.IsNullOrWhiteSpace(c.Filter))
                .Select(
                    c => new LdapQueries
                    {
                        SearchBase = c.SearchBase,
                        Filter = c.Filter
                    })
                .ToList();

            if (ldapQueriesConfig == null || !ldapQueriesConfig.Any())
            {
                _logger.LogWarnMessage(
                    "Active directory configuration '{connectionString}/{basePath}'. (Description: '{description}') does not contain any ldap query configuration.",
                    LogHelpers.Arguments(
                        singleConnection.Connection.ConnectionString,
                        singleConnection.Connection.BasePath,
                        singleConnection.Connection.Description));

                continue;
            }

            _ldapSystemConfiguration.EntitiesMapping ??= new Dictionary<string, string>();

            if (!_ldapSystemConfiguration.EntitiesMapping.Any())
            {
                _logger.LogWarnMessage(
                    "Default mapping for user will be used, because no entities mapping found for the connection {connection} and the ldap-search-base: '{ldapQueries}'.",
                    LogHelpers.Arguments(
                        singleConnection.Connection,
                        LdapQueriesAsString(singleConnection.LdapQueries)));
            }

            IList<UserSync> foundUsers = ldapQueriesConfig.GetAllUsers(singleConnection.Connection, _logger, _ldapSystemConfiguration.EntitiesMapping );

            if (foundUsers != null && foundUsers.Any())
            {
                users.AddRange(foundUsers);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTraceMessage(
                        "Found {count} users in ldap system as batch. Current users: {users}",
                        LogHelpers.Arguments(
                            foundUsers?.Count,
                            foundUsers?.Select(f => $"{f.Id} - {f.DisplayName}")));
                }
                else
                {
                    _logger.LogInfoMessage(
                        "Found {count} users in ldap system as batch.",
                        LogHelpers.Arguments(foundUsers?.Count));
                }
            }
            else
            {
                _logger.LogInfoMessage("Found no users in ldap system as batch.", LogHelpers.Arguments());
            }
        }

        IBatchResult<UserSync> batchResult = GetBatchResult(users, start, batchSize);

        return _logger.ExitMethod(Task.FromResult(batchResult));
    }

    /// <inheritdoc />
    public Task<UserSync> CreateEntity(UserSync entity, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<UserSync> UpdateEntity(string sourceId, UserSync entity, CancellationToken token)
    {
        _logger.EnterMethod();

        throw new NotImplementedException();
    }
}
