using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Novell.Directory.Ldap;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Models.Entities;

namespace UserProfileService.Sync.Extensions.Ldap;

/// <summary>
///     Contains extension methods related to <see cref="List{T}" />s.
/// </summary>
internal static class LdapQueriesExtension
{
    private static UserSync EntryToProfileInternal(
        LdapEntry entry,
        ActiveDirectoryConnection settings,
        ILogger logger = null)
    {
        logger?.EnterMethod();

        IEnumerable<string> objectClasses =
            entry.GetAttribute("objectClass").StringValueArray?.Select(a => a.ToLower());

        if (objectClasses != null)
        {
            IEnumerable<string> objectClassesEnumerable = objectClasses as string[] ?? objectClasses.ToArray();

            if (objectClassesEnumerable.Contains("inetorgperson") || objectClassesEnumerable.Contains("user"))
            {
                UserSync userSync = entry.EntryToUser(settings);

                return logger.ExitMethod(userSync);
            }
        }

        if (logger?.IsEnabled(LogLevel.Trace) == true)
        {
            logger.LogTraceMessage(
                "Could not convert ldap entry to internal user sync mode. Ldap entry: {ldapEntry}",
                LogHelpers.Arguments(JsonConvert.SerializeObject(entry)));
        }
        else
        {
            logger?.LogWarnMessage(
                "Could not convert ldap entry to internal user sync mode.",
                LogHelpers.Arguments());
        }

        return logger.ExitMethod<UserSync>(null);
    }

    /// <summary>
    ///     Get all user from the ldap.
    /// </summary>
    /// <param name="ldapQueries">The list of ldap queries for one active directory.</param>
    /// <param name="activeDirectoryConnection">The ldap connection for the users to retrieve.</param>
    /// <param name="logger">The logger for logging purposes.</param>
    /// <returns>A list of <see cref="UserSync" />.</returns>
    public static IList<UserSync> GetAllUsers(
        this List<LdapQueries> ldapQueries,
        ActiveDirectoryConnection activeDirectoryConnection,
        ILogger logger)
    {
        logger?.EnterMethod();

        logger?.LogDebugMessage("Execute {count} queries to ldap.", LogHelpers.Arguments(ldapQueries.Count));

        IList<UserSync> allUsersFromAd = new List<UserSync>();

        foreach (LdapQueries ldapQuery in ldapQueries)
        {
            logger?.LogDebugMessage(
                "Execute query to ldap with filter {filter} and searchBase {searchBase}.",
                LogHelpers.Arguments(ldapQuery.Filter, ldapQuery.SearchBase));

            try
            {
                var ldapOptions = new LdapConnectionOptions();

                // if we ignore the certificate, that we trust every
                // certificate.
                if (activeDirectoryConnection.IgnoreCertificate)
                {
                    // We trust the root Certificate and doing
                    // ssl. Should be validated in the future.
                    ldapOptions.ConfigureRemoteCertificateValidationCallback((_, _, _, _) => true);
                }

                using var connection = new LdapConnection(ldapOptions);

                if (connection.ConfigureConnection(activeDirectoryConnection))
                {
                    try
                    {
                        logger?.LogDebugMessage(
                            "Execute query to ldap with filter {filter}, searchBase {searchBase}, basePath {basePath} and  connectionString {connectionString}.",
                            LogHelpers.Arguments(
                                ldapQuery.Filter,
                                ldapQuery.SearchBase,
                                activeDirectoryConnection.BasePath,
                                activeDirectoryConnection.ConnectionString));

                        List<UserSync> results = connection
                            .SearchWithSimplePaging<UserSync>(
                                ldapQuery,
                                activeDirectoryConnection.BasePath,
                                entry => EntryToProfileInternal(
                                    entry,
                                    activeDirectoryConnection),
                                logger)
                            .ToList();

                        if (!results.Any())
                        {
                            logger?.LogWarnMessage(
                                "No results found in ldap with filter {filter}, searchBase {searchBase}, basePath {basePath} and  connectionString {connectionString}.",
                                LogHelpers.Arguments(
                                    ldapQuery.Filter,
                                    ldapQuery.SearchBase,
                                    activeDirectoryConnection.BasePath,
                                    activeDirectoryConnection.ConnectionString));

                            continue;
                        }

                        logger?.LogInfoMessage("Found '{count}' entries.", LogHelpers.Arguments(results.Count));

                        IList<UserSync> ad = allUsersFromAd;

                        IEnumerable<UserSync> entryResults =
                            results.Where(entry => ad.All(p => p.Id != entry.Id));

                        allUsersFromAd = allUsersFromAd.Concat(entryResults).ToList();
                    }
                    catch (Exception e)
                    {
                        logger?.LogErrorMessage(
                            e,
                            "Failed to get entries for for: '{ldapQuery}'.",
                            LogHelpers.Arguments(JsonConvert.SerializeObject(ldapQuery)));
                    }
                }
            }
            catch (Exception e)
            {
                logger?.LogErrorMessage(
                    e,
                    "Something went wrong while connection to ldap. Continue to try next query.",
                    LogHelpers.Arguments());
            }
        }

        return logger.ExitMethod(allUsersFromAd);
    }
}
