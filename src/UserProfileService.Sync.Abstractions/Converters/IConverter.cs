namespace UserProfileService.Sync.Abstraction.Converters;

/// <summary>
///     Converter used to convert elements from type {T}.
/// </summary>
public interface IConverter<T>
{
    /// <summary>
    ///     Convert an element from type {T} with the defined rules.
    /// </summary>
    /// <param name="source">The element that should be converted.</param>
    /// <returns> The converted element from type {T}.</returns>
    T Convert(T source);
}
