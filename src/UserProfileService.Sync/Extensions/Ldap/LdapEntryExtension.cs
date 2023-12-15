using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Models.System.Security.Principal;

namespace UserProfileService.Sync.Extensions.Ldap;

/// <summary>
///     Contains extension methods related to <see cref="LdapEntry" />s.
/// </summary>
internal static class LdapEntryExtension
{
    private static string ExtractDomain(LdapEntry entry)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        string domain = entry.GetAttributeOrDefault("DistinguishedName")?.StringValue;

        if (string.IsNullOrWhiteSpace(domain))
        {
            return string.Empty;
        }

        var buildDomain = new StringBuilder();

        foreach (string domainString in domain.Split(",", StringSplitOptions.RemoveEmptyEntries))
        {
            if (domainString.StartsWith("DC=", StringComparison.OrdinalIgnoreCase))
            {
                buildDomain.Append(domainString.Replace("DC=", string.Empty).Trim().ToLower()).Append(".");
            }
        }

        return buildDomain.Remove(buildDomain.Length - 1, 1).ToString();
    }

    /// <summary>
    ///     Gets an attribute for a given key.
    /// </summary>
    /// <param name="entry">Entry containing the attribute.</param>
    /// <param name="attributeKey">Key of attribute to be returned.</param>
    /// <returns>Attribute, default null.</returns>
    public static LdapAttribute GetAttributeOrDefault(this LdapEntry entry, string attributeKey)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(attributeKey))
        {
            return null;
        }

        LdapAttributeSet set = entry.GetAttributeSet();

        return set.TryGetValue(attributeKey, out LdapAttribute attribute) ? attribute : null;
    }

    /// <summary>
    ///     Returns the SID from the entry.
    /// </summary>
    /// <param name="entry">The entry from which the sid should be extracted.</param>
    /// <param name="settings">The settings from the ldap.</param>
    /// <returns>The SID from the given entry.</returns>
    public static string GetSidFromEntry(this LdapEntry entry, ActiveDirectoryConnection settings)
    {
        string ldapIdKey = settings.GetProfileIdLdapKey();
        byte[] sidBytes = entry.GetAttribute(ldapIdKey).ByteValue;

        if (sidBytes == null || !sidBytes.Any())
        {
            return null;
        }

        switch (ldapIdKey)
        {
            case "objectSid":
                return new SecurityIdentifier(sidBytes, 0).ToString();
            case "objectGuid":
                return new Guid(sidBytes).ToString();
            default:
                string ldapValue = entry.GetAttributeOrDefault(ldapIdKey)?.StringValue;

                if (string.IsNullOrWhiteSpace(ldapValue))
                {
                    throw new ArgumentException(
                        $"The given object identifier key '{ldapIdKey}' is null or empty. DN= {entry.Dn}",
                        nameof(ldapIdKey));
                }

                return ldapValue;
        }
    }

    /// <summary>
    ///     Convert the given <see cref="LdapEntry" /> to <see cref="UserSync" />.
    /// </summary>
    /// <param name="entry">Ldap entry to be converted.</param>
    /// <param name="settings">Settings to use while converting.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>Converted ldap entry as <see cref="UserSync" /></returns>
    public static UserSync EntryToUser(
        this LdapEntry entry,
        ActiveDirectoryConnection settings,
        ILogger logger = null)
    {
        string sid = entry.GetSidFromEntry(settings);

        // Set default values from ldap to object
        var ret = new UserSync
        {
            Id = sid,
            Name = entry.GetAttributeOrDefault("cn")?.StringValue ?? "",
            DisplayName = entry.GetAttributeOrDefault("displayName")?.StringValue
                ?? entry.GetAttribute("cn")?.StringValue ?? "",
            FirstName = entry.GetAttributeOrDefault("givenName")?.StringValue ?? "",
            LastName = entry.GetAttributeOrDefault("sn")?.StringValue ?? "",
            Domain = ExtractDomain(entry),
            ExternalIds = new List<KeyProperties>
            {
                new KeyProperties(sid, SyncConstants.System.Ldap)
            },
            Email = entry.GetAttributeOrDefault("mail")?.StringValueArray?.FirstOrDefault() ?? string.Empty,
            Source = SyncConstants.System.Ldap,
            SynchronizedAt = DateTime.UtcNow,
            UserName = entry.GetAttributeOrDefault("sAmAccountName")?.StringValue ?? ""
        };

        // Overwrite default values with custom property mapping.
        Type userType = ret.GetType();

        foreach (KeyValuePair<string, string> profileMapping in settings.ProfileMapping)
        {
            PropertyInfo propertyInfo = userType.GetProperty(profileMapping.Key);

            if (propertyInfo == null)
            {
                logger?.LogWarnMessage(
                    "The defined profile mapping key '{key}' is not included in the object of type '{type}' and cannot be mapped.",
                    LogHelpers.Arguments(profileMapping.Key, userType.Name));

                continue;
            }

            try
            {
                string ldapValue = entry.GetAttributeOrDefault(profileMapping.Value)?.StringValue ?? "";
                propertyInfo.SetValue(ret, Convert.ChangeType(ldapValue, propertyInfo.PropertyType));
            }
            catch (Exception e)
            {
                logger?.LogErrorMessage(
                    e,
                    "The profile mapping key for ldap '{value}' cannot be extracted and set as property '{key}' in the object. Type: '{type}'",
                    LogHelpers.Arguments(profileMapping.Value, profileMapping.Key, userType.Name));
            }
        }

        return ret;
    }
}
