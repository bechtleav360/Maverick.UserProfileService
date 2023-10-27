using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Extensions.Ldap;

/// <summary>
///     Contains extension methods related to <see cref="LdapConnection" />s.
/// </summary>
public static class LdapConnectionExtension
{
    /// <summary>
    ///     Searches the ldap with the given parameters and converts the entries to the corresponding entity.
    /// </summary>
    /// <typeparam name="T">Type into which the entry is to be converted.</typeparam>
    /// <param name="connection">Ldap connection in which to search.</param>
    /// <param name="searchBase">The search base of ldap.</param>
    /// <param name="scope">The scope to use.</param>
    /// <param name="filter">Filter for ldap entries.</param>
    /// <param name="converter">Converter used to change the ldap entry to the desired type.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>Collection of converted entries found in ldap.</returns>
    private static IEnumerable<T> SearchWithSimplePaging<T>(
        this LdapConnection connection,
        string searchBase,
        int scope,
        string filter,
        Func<LdapEntry, ISyncModel> converter,
        ILogger logger = null) where T : ISyncModel
    {
        logger?.EnterMethod();

        var cons = new LdapSearchConstraints
        {
            ServerTimeLimit = 0,
            TimeLimit = 0,
            BatchSize = 500
        };

        var simplePageResults = new SimplePagedResultsControlHandler(connection);

        var searchOptions = new SearchOptions(
            searchBase,
            scope,
            filter,
            null,
            false,
            cons);

        List<LdapEntry> results = simplePageResults.SearchWithSimplePaging(searchOptions, 500);

        if (results == null || !results.Any())
        {
            return logger.ExitMethod(Array.Empty<T>());
        }

        IEnumerable<T> entryResults = results
            .Select(
                entry =>
                {
                    try
                    {
                        return converter.Invoke(entry);
                    }
                    catch (Exception e)
                    {
                        logger?.LogErrorMessage(
                            e,
                            "An error occurred while converting ldap entry to internal object.",
                            LogHelpers.Arguments());

                        return null; // Return null and will be removed in next step.
                    }
                })
            .Where(entry => entry != null)
            .OfType<T>();

        return entryResults;
    }

    /// <summary>
    ///     Configure the ldap connection with given <see cref="ActiveDirectoryConnection" /> settings.
    /// </summary>
    /// <param name="connection">Connection to be configured.</param>
    /// <param name="singleAdConnectionSetting">Settings with which the connection is to be configured.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>True if connection is established, otherwise false.</returns>
    public static bool ConfigureConnection(
        this LdapConnection connection,
        ActiveDirectoryConnection singleAdConnectionSetting,
        ILogger logger = null)
    {
        logger.EnterMethod();

        connection.ConnectionTimeout = 0;
        connection.Constraints.TimeLimit = 0;
        connection.SecureSocketLayer = singleAdConnectionSetting.UseSsl;
        int ldapPort = singleAdConnectionSetting.Port ?? LdapConnection.DefaultSslPort;

        if (!singleAdConnectionSetting.UseSsl)
        {
            logger.LogWarnMessage("Ldap uses an insecure connection!", LogHelpers.Arguments());
        }

        else
        {
            logger.LogInfoMessage("Ldap uses a secure connection.", LogHelpers.Arguments());
        }

        logger.LogInfoMessage(
            "The LDAP uses the port: {port}.",
            LogHelpers.Arguments(ldapPort));

        connection.Connect(
            new Uri(singleAdConnectionSetting.ConnectionString).Host,
            ldapPort);

        logger.LogDebugMessage(
            "The connection string for ldap: {singleAdConnectionSetting.ConnectionString}",
            LogHelpers.Arguments(singleAdConnectionSetting.ConnectionString));

        if (connection.Connected)
        {
            connection.Bind(
                LdapConnection.LdapV3,
                singleAdConnectionSetting.ServiceUser,
                singleAdConnectionSetting.ServiceUserPassword);

            return logger.ExitMethod(connection.Bound);
        }

        return logger.ExitMethod(false);
    }

    /// <summary>
    ///     Searches the ldap with the given parameters and converts the entries to the corresponding entity.
    /// </summary>
    /// <typeparam name="T">Type into which the entry is to be converted.</typeparam>
    /// <param name="connection">Ldap connection in which to search.</param>
    /// <param name="ldapQuery">Query to search.</param>
    /// <param name="basePath">The base path of ldap.</param>
    /// <param name="converter">Converter used to change the ldap entry to the desired type.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>Collection of converted entries found in ldap.</returns>
    public static IEnumerable<T> SearchWithSimplePaging<T>(
        this LdapConnection connection,
        LdapQueries ldapQuery,
        string basePath,
        Func<LdapEntry, ISyncModel> converter,
        ILogger logger = null) where T : ISyncModel
    {
        logger?.EnterMethod();

        string delimiter = string.IsNullOrWhiteSpace(ldapQuery.SearchBase) ? string.Empty : ",";

        string searchBase = ldapQuery.SearchBase + delimiter + basePath;

        logger?.LogDebugMessage("Create search base from connection: {filter}.", LogHelpers.Arguments(searchBase));

        IEnumerable<T> results = connection.SearchWithSimplePaging<T>(
            searchBase,
            LdapConnection.ScopeSub,
            ldapQuery.Filter,
            converter,
            logger);

        return logger.ExitMethod(results);
    }
}
