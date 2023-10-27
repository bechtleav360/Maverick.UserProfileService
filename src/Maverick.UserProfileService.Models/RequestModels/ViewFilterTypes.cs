using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Defines types of the view filter in the API.
    /// </summary>
    public enum ViewFilterTypes
    {
        /// <summary>
        ///     The filter type is a string.
        /// </summary>
        [FilterSerialize("string")]
        String,

        /// <summary>
        ///     The filter type is a key-value pair.
        /// </summary>
        [FilterSerialize("keyValue")]
        KeyValue,

        /// <summary>
        ///     The filter type is a date.
        /// </summary>
        [FilterSerialize("date")]
        Date
    }
}
