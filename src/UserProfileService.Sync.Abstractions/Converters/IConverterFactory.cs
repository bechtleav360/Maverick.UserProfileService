using UserProfileService.Sync.Abstraction.Configurations;

namespace UserProfileService.Sync.Abstraction.Converters;

/// <summary>
///     A Factory to create instance from type <see cref="IConverter{T}" />
/// </summary>
public interface IConverterFactory<T>
{
    /// <summary>
    ///     Create a converter from type <see cref="IConverter{T}" />
    /// </summary>
    /// <param name="configuration"> The configuration of the third part system. </param>
    /// <param name="currentStep"> The current saga step. </param>
    /// <returns> The created converter <see cref="IConverter{T}" />. </returns>
    public IConverter<T> CreateConverter(
        SourceSystemConfiguration configuration,
        string currentStep);
}
