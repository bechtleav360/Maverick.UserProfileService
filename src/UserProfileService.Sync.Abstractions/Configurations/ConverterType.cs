using UserProfileService.Sync.Abstraction.Converters;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     Define the type of the converter <see cref="IConverter{T}" />
/// </summary>
public enum ConverterType
{
    /// <summary>
    ///     Concatenate a prefix to an external Id.
    /// </summary>
    Prefix
}
