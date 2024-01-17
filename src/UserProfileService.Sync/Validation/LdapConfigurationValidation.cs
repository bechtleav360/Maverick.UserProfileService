using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Configuration;

namespace UserProfileService.Sync.Validation;

/// <summary>
///     A Class used to validate the <see cref="LdapConfigurationValidation" />
/// </summary>
public class LdapConfigurationValidation: IValidateOptions<LdapSystemConfiguration>
{
    private static LoggerFactory _loggerFactory = new LoggerFactory();
    
    private static IList<string> ValidateActiveDirectoryConfiguration(ActiveDirectory[] ldapConfiguration)
    {
        var validationErrors = new List<string>();

        foreach (ActiveDirectory activeDirectory in ldapConfiguration)
        {
            if (activeDirectory != null && (activeDirectory.LdapQueries == null || !activeDirectory.LdapQueries.Any()))
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(ldapConfiguration)} for the system: Ldap': One of the AD doesn't have any Ldap queries");
            }
            else if (activeDirectory is
                     {
                         LdapQueries:
                         {
                         }
                     }
                     && activeDirectory.LdapQueries.Any())
            {
                foreach (LdapQueries activeDirectoryLdapQuery in activeDirectory.LdapQueries)
                {
                    if (string.IsNullOrWhiteSpace(activeDirectoryLdapQuery.Filter))
                    {
                        validationErrors.Add(
                            $"Configuration error concerning '{nameof(ldapConfiguration)} ': The filter is not set (null or whitespace) inside one of the Ldap queries");
                    }

                    if (string.IsNullOrWhiteSpace(activeDirectoryLdapQuery.SearchBase))
                    {
                        validationErrors.Add(
                            $"Configuration error concerning '{nameof(ldapConfiguration)} ': The search base is not set (null or whitespace) inside one of the Ldap queries");
                    }
                }
            }

            if (activeDirectory != null && activeDirectory.Connection == null)
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(ActiveDirectory.Connection)} ': The connection information of one provided AD are missing (null)");
            }

            if (activeDirectory is
                {
                    Connection:
                    {
                    }
                })
            {
                if (string.IsNullOrWhiteSpace(activeDirectory.Connection.ConnectionString))
                {
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(ActiveDirectory.Connection.ConnectionString)} ': The connection string of one provided AD is null or whitespace");
                }

                if (string.IsNullOrWhiteSpace(activeDirectory.Connection.AuthenticationType))
                {
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(ActiveDirectory.Connection.AuthenticationType)} ': The authentication type of one provided AD is null or whitespace");
                }

                if (string.IsNullOrWhiteSpace(activeDirectory.Connection.ServiceUser))
                {
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(ActiveDirectory.Connection.ServiceUser)} ': The service user of one provided AD is null or whitespace");
                }

                if (string.IsNullOrWhiteSpace(activeDirectory.Connection.ServiceUserPassword))
                {
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(ActiveDirectory.Connection.ServiceUserPassword)} ': The service user password of one provided AD is null or whitespace");
                }

                if (string.IsNullOrWhiteSpace(activeDirectory.Connection.BasePath))
                {
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(ActiveDirectory.Connection.BasePath)} ': The base path of one provided AD is null or whitespace");
                }
            }
        }

        IList<string> errorValidationSslOption = ValidateLdapConnectionSecurityOptions(ldapConfiguration);

        if (errorValidationSslOption.Any())
        {
            validationErrors.AddRange(errorValidationSslOption);
        }

        return validationErrors;
    }

    internal static IList<string> ValidateLdapConnectionSecurityOptions(ActiveDirectory[] activeDirectories)
    {
        var validationErrors = new List<string>();

        const int ldapSslPort = 636;
        const int ldapStandardPort = 389;
        var logger = _loggerFactory.CreateLogger("Validation");

        foreach (ActiveDirectory activeDirectory in activeDirectories)
        {
            // negative port validation
            if (activeDirectory.Connection.Port < 0)
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(ActiveDirectory.Connection.Port)} ': The port of one provided AD is negativ!");
            }

            // ignore certificate option with ssl
            switch (activeDirectory.Connection.UseSsl)
            {
                case true when !activeDirectory.Connection.IgnoreCertificate:
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(ActiveDirectory.Connection.IgnoreCertificate)}': The '{nameof(ActiveDirectory.Connection.UseSsl)}' option is set to true in one of the provided AD, but the option {nameof(ActiveDirectory.Connection.IgnoreCertificate)} is not!");

                    break;
                case false when activeDirectory.Connection.IgnoreCertificate:
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(ActiveDirectory.Connection.IgnoreCertificate)}': The '{nameof(ActiveDirectory.Connection.UseSsl)} option is set to false in one of the provided AD, but the option {nameof(ActiveDirectory.Connection.IgnoreCertificate)} is not!");

                    break;
            }

            // port warning with ssl
            switch (activeDirectory.Connection.UseSsl)
            {
                case true when activeDirectory.Connection.Port != ldapSslPort:
                    logger.LogWarnMessage(
                        "Configuration warning concerning '{Port}': The '{UseSsl}' option is set to true in one of the provided AD and you are not using the ssl standard port: ({ldapSslPort})",
                        LogHelpers.Arguments(
                            nameof(activeDirectory.Connection.Port),
                            nameof(activeDirectory.Connection.UseSsl),
                            ldapSslPort));

                    break;
                case false when activeDirectory.Connection.Port != ldapStandardPort:
                    logger.LogWarnMessage(
                        "Configuration warning concerning '{Port}'; The '{UseSsl}' option is set to false in one of the provided AD and you are not using the standard port: ({ldapStandardPort})",
                        LogHelpers.Arguments(
                            nameof(activeDirectory.Connection.Port),
                            nameof(activeDirectory.Connection.UseSsl),
                            ldapStandardPort));

                    break;
            }
        }

        return validationErrors;
    }

    internal List<string> ValidateMapping(LdapSystemConfiguration ldapSystem)
    {
        var errorList = new List<string>();

        if (ldapSystem?.EntitiesMapping == null)
        {
            errorList.Add("The entities mapping is null and not defined.");
        }

        if (ldapSystem?.EntitiesMapping?.Count == 0)
        {
            errorList.Add("The entities mapping is defined, but empty.");
        }
        
        return errorList;
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string name, LdapSystemConfiguration options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error occurred: Options object not provided!");
        }

        var validationErrors = ValidateActiveDirectoryConfiguration(options.LdapConfiguration);
        
        validationErrors.AddRange(ValidateMapping(options));
        
        if (validationErrors.Any())
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error(s) occurred:{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors.Select((v, i) => $"{i + 1}. {v}"))}");
        }

        return ValidateOptionsResult.Success;
    }
}
