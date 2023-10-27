using UserProfileService.Sync.Abstraction.Converters;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Converter;

/// <summary>
///     A Default converter
/// </summary>
/// <typeparam name="TEntity"> source entity type </typeparam>
public class DefaultConverter<TEntity> : IConverter<TEntity>
    where TEntity : ISyncModel
{
    /// <inheritdoc />
    public TEntity Convert(TEntity source)
    {
        return source;
    }
}
