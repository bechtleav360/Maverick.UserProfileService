using System.Collections.Generic;
using System.Linq;
using UserProfileService.Sync.Abstraction.Converters;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Converter;

/// <summary>
///     A Class used to convert an external id to an other id with prefix.
/// </summary>
public class PrefixConverter<TEntry> : IConverter<TEntry> where TEntry : ISyncModel
{
    /// <summary>
    ///     The prefix that should be add to the external Ids.
    /// </summary>
    private readonly string _prefix;

    /// <summary>
    ///     Create a new instance of <see cref="PrefixConverter{TEntry}" />
    /// </summary>
    /// <param name="prefix"> The prefix used by the converter. </param>
    public PrefixConverter(string prefix)
    {
        _prefix = prefix;
    }

    /// <inheritdoc />
    public TEntry Convert(TEntry source)
    {
        if (source == null || source.ExternalIds == null)
        {
            return source;
        }

        if (source.ExternalIds.Any(e => e.Id.StartsWith(_prefix)))
        {
            return source;
        }

        List<KeyProperties> convertedExternalIds = source
            .ExternalIds
            .Select(
                keyProperty => new KeyProperties(
                    $"{_prefix}{keyProperty.Id}",
                    keyProperty.Source,
                    keyProperty.Filter,
                    true))
            .ToList();

        source.ExternalIds = source.ExternalIds.Concat(convertedExternalIds).ToList();

        return source;
    }
}
