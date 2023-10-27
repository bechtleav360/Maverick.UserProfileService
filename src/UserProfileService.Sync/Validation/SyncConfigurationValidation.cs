using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Validation;

/// <summary>
///     A Class used to validate the <see cref="SyncConfiguration" />
/// </summary>
public class SyncConfigurationValidation : IValidateOptions<SyncConfiguration>
{
    private static readonly LoggerFactory _loggerFactory = new LoggerFactory();

    internal static IList<string> ValidateLdapConnectionSecurityOptions(ActiveDirectory[] activeDirectories)
    {
        var validationErrors = new List<string>();

        const int ldapSslPort = 636;
        const int ldapStandardPort = 389;
        ILogger logger = _loggerFactory.CreateLogger("Validation");

        foreach (ActiveDirectory activeDirectory in activeDirectories)
        {
            // negative port validation
            if (activeDirectory.Connection.Port < 0)
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(ActiveDirectory.Connection.Port)} ': The port of one provided AD is negative!");
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

    private IList<string> ValidateSourceSystemConfiguration(
        Dictionary<string, SourceSystemConfiguration> systems)
    {
        var validationErrors = new List<string>();

        if (systems == null || !systems.Any())
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(SyncConfiguration.SourceConfiguration.Systems)}': It should not be null");
        }
        else
        {
            foreach (KeyValuePair<string, SourceSystemConfiguration> sourceSystemConfiguration in systems)
            {
                validationErrors.AddRange(
                    ValidateSourceSystemConfiguration(sourceSystemConfiguration.Value, sourceSystemConfiguration.Key));
            }
        }

        return validationErrors;
    }

    private static IList<string> ValidateActiveDirectoryConfiguration(ActiveDirectory[] ldapConfiguration)
    {
        var validationErrors = new List<string>();

        foreach (ActiveDirectory activeDirectory in ldapConfiguration)
        {
            if (activeDirectory != null
                && (activeDirectory.LdapQueries == null || !activeDirectory.LdapQueries.Any()))
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(GeneralSystemConfiguration.LdapConfiguration)} for the system: {SyncConstants.System.Ldap}': One of the AD doesn't have any Ldap queries");
            }
            else if (activeDirectory is
                     {
                         LdapQueries: not null
                     }
                     && activeDirectory.LdapQueries.Any())
            {
                foreach (LdapQueries activeDirectoryLdapQuery in activeDirectory.LdapQueries)
                {
                    if (string.IsNullOrWhiteSpace(activeDirectoryLdapQuery.Filter))
                    {
                        validationErrors.Add(
                            $"Configuration error concerning '{nameof(GeneralSystemConfiguration.LdapConfiguration)} ': The filter is not set (null or whitespace) inside one of the Ldap queries");
                    }

                    if (string.IsNullOrWhiteSpace(activeDirectoryLdapQuery.SearchBase))
                    {
                        validationErrors.Add(
                            $"Configuration error concerning '{nameof(GeneralSystemConfiguration.LdapConfiguration)} ': The search base is not set (null or whitespace) inside one of the Ldap queries");
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
                    Connection: not null
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

    private static IList<string> ValidateSynchronizationOperations(
        string systemName,
        Dictionary<string, SynchronizationOperations> input,
        string propertyName)
    {
        var validationErrors = new List<string>();

        if (input == null || input.Count <= 0)
        {
            return validationErrors;
        }

        foreach (KeyValuePair<string, SynchronizationOperations> entry in input)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                validationErrors.Add(
                    $"Configuration error concerning '{propertyName}' in the system: {systemName} : The key should not be empty or whitespace");
            }

            if (entry.Value == null)
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(SynchronizationOperations)}' for {entry.Key} in the system: {systemName} : The value of the source should not be null");
            }

            if (entry.Value != null
                && entry.Value.Converter != null
                && (entry.Value.Converter.ConverterProperties == null
                    || !entry.Value.Converter.ConverterProperties.Any()))
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(ConverterConfiguration)}' for {entry.Key} in the system: {systemName} : The converter has been defined without properties");
            }
        }

        return validationErrors;
    }

    /// <summary>
    ///     Validates an instance of <see cref="SourceSystemConfiguration"/> given the name of the system.
    /// </summary>
    /// <param name="systemConfig">The configuration to validate.</param>
    /// <param name="systemName">The name of the associated system.</param>
    /// <returns>A list of validation errors. Empty, if <paramref name="systemConfig"/> was valid.</returns>
    protected virtual IList<string> ValidateSourceSystemConfiguration(
        SourceSystemConfiguration systemConfig,
        string systemName)
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(systemName))
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(SyncConfiguration.SourceConfiguration.Systems)} for the system: {systemName}': The provided system should not be null or whitespace");
        }

        else if (!GetSupportedSystems().Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(SyncConfiguration.SourceConfiguration.Systems)} for the system: {systemName}': The provided system is not supported");
        }

        if (systemConfig == null)
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(SyncConfiguration.SourceConfiguration.Systems)} for the system: {systemName}': It should not be null");
        }

        if (systemConfig is { Configuration: null })
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(SourceSystemConfiguration.Configuration)}' for the system: {systemName}: It should not be null");
        }
        else if (systemConfig is
                 {
                     Configuration: not null
                 }
                 && !string.IsNullOrWhiteSpace(systemName))
        {
            validationErrors.AddRange(ValidateGeneralSystemConfiguration(systemConfig.Configuration, systemName));
        }

        if (systemConfig != null
            && (systemConfig.Source == null || !systemConfig.Source.Any())
            && (systemConfig.Destination == null || !systemConfig.Destination.Any()))
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(SourceSystemConfiguration.Source)}' and '{nameof(SourceSystemConfiguration.Destination)}' for the system: {systemName}: both should not be null or empty");
        }

        if (systemConfig is { Source: not null, Destination: null })
        {
            validationErrors.AddRange(
                ValidateSynchronizationOperations(
                    systemName,
                    systemConfig.Source,
                    nameof(SourceSystemConfiguration.Source)));
        }

        if (systemConfig is
            {
                Source: null, Destination: not null
            })
        {
            validationErrors.AddRange(
                ValidateSynchronizationOperations(
                    systemName,
                    systemConfig.Destination,
                    nameof(SourceSystemConfiguration.Destination)));
        }

        if (systemConfig is
            {
                Source: not null, Destination: not null
            })
        {
            validationErrors.AddRange(
                ValidateSynchronizationOperations(
                    systemName,
                    systemConfig.Source,
                    nameof(SourceSystemConfiguration.Source)));

            validationErrors.AddRange(
                ValidateSynchronizationOperations(
                    systemName,
                    systemConfig.Destination,
                    nameof(SourceSystemConfiguration.Destination)));
        }

        return validationErrors;
    }

    /// <summary>
    ///     Returns the names of supported sync systems.
    /// </summary>
    /// <returns>The names of supported systems.</returns>
    protected virtual IEnumerable<string> GetSupportedSystems()
    {
        return SyncConstants.System.All;
    }

    /// <summary>
    ///     Validates an instance of <see cref="GeneralSystemConfiguration"/> given the name of the system.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <param name="systemName">The name of the associated system.</param>
    /// <returns>A list of validation errors. Empty, if <paramref name="configuration"/> was valid.</returns>
    protected virtual IList<string> ValidateGeneralSystemConfiguration(
        GeneralSystemConfiguration configuration,
        string systemName)
    {
        var validationErrors = new List<string>();

        if (!systemName.Equals(SyncConstants.System.Ldap, StringComparison.OrdinalIgnoreCase))
        {
            return validationErrors;
        }

        if (configuration.LdapConfiguration == null || !configuration.LdapConfiguration.Any())
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(GeneralSystemConfiguration.LdapConfiguration)} for the system: {systemName}': It should not be null or empty");
        }
        else
        {
            validationErrors.AddRange(ValidateActiveDirectoryConfiguration(configuration.LdapConfiguration));
        }

        return validationErrors;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, SyncConfiguration options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error occurred: Options object not provided!");
        }

        var validationErrors = new List<string>();

        if (options.SourceConfiguration == null)
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.SourceConfiguration)}': It should not be null");
        }
        else
        {
            if (options.SourceConfiguration.Validation == null)
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(options.SourceConfiguration.Validation)}': It should not be null");
            }
            else
            {
                if (options.SourceConfiguration.Validation.Internal == null)
                {
                    validationErrors.Add(
                        $"Configuration error concerning '{nameof(options.SourceConfiguration.Validation.Internal)}': It should not be null");
                }
            }
        }

        if (options.SourceConfiguration is { Systems: null })
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.SourceConfiguration.Systems)}': It should not be null");
        }

        if (options.SourceConfiguration is
            {
                Systems: not null
            })
        {
            validationErrors.AddRange(ValidateSourceSystemConfiguration(options.SourceConfiguration.Systems));
        }

        if (validationErrors.Any())
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error(s) occurred:{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors.Select((v, i) => $"{i + 1}. {v}"))}");
        }

        return ValidateOptionsResult.Success;
    }
}
