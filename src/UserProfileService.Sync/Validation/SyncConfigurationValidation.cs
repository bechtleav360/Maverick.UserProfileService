using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Validation;

/// <summary>
///     A Class used to validate the <see cref="SyncConfiguration" />
/// </summary>
public class SyncConfigurationValidation : IValidateOptions<SyncConfiguration>
{
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
