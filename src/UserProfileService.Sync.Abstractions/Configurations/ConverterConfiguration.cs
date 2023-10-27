using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     An object containing the configuration information for a converter.
/// </summary>
public class ConverterConfiguration
{
    /// <summary>
    ///     The properties of the Converter
    /// </summary>
    public Dictionary<string, string> ConverterProperties { get; set; }

    /// <summary>
    ///     The type of the converter <see cref="ConverterType" />
    /// </summary>
    public ConverterType ConverterType { get; set; }
}
