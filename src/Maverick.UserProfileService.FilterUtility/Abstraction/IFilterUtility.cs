namespace Maverick.UserProfileService.FilterUtility.Abstraction
{
    /// <summary>
    ///     Interface for internal filter serializers
    /// </summary>
    /// <typeparam name="TFilter">Result Type of the filter</typeparam>
    public interface IFilterUtility<TFilter>
    {
        /// <summary>
        ///     Deserializes a string to <see cref="TFilter" />
        /// </summary>
        /// <param name="serializedFilter">The serialized string filter</param>
        /// <returns>The deserialized <see cref="TFilter" /></returns>
        TFilter Deserialize(string serializedFilter);

        /// <summary>
        ///     Serializes a <see cref="TFilter" /> to string
        /// </summary>
        /// <param name="filter">The filter object</param>
        /// <returns>The serialized <see cref="TFilter" /> as string</returns>
        string Serialize(TFilter filter);
    }
}
