using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Converters;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Converter;

/// <summary>
///     A Factory to create instance from type <see cref="IConverter{T}" />
/// </summary>
public class ExternalIdConverterFactory<T> : IConverterFactory<T> where T : ISyncModel
{
    private static IConverter<T> RetrieveConverter(ConverterConfiguration converterConfiguration)
    {
        if (converterConfiguration == null)
        {
            return null;
        }

        if (converterConfiguration.ConverterType != ConverterType.Prefix)
        {
            return null;
        }

        if (converterConfiguration.ConverterProperties != null
            && converterConfiguration.ConverterProperties.TryGetValue("Prefix", out string prefix))
        {
            return new PrefixConverter<T>(prefix);
        }

        return null;
    }

    /// <inheritdoc />
    public IConverter<T> CreateConverter(SourceSystemConfiguration configuration, string currentStep)
    {
        return configuration.Source.TryGetValue(currentStep, out SynchronizationOperations config)
            ? RetrieveConverter(config?.Converter)
            : null;
    }
}
