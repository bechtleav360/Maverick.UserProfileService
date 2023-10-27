using Maverick.UserProfileService.FilterUtility.Implementations;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Extensions;

/// <summary>
///     Extensions for <see cref="List{T}" />
/// </summary>
public static class ListExtensions
{
    /// <summary>
    ///     Extensions for parsing a ViewFilter string and adding them to the list
    /// </summary>
    /// <param name="l">
    ///     <see cref="List{ViewFilterModel}" />
    /// </param>
    /// <param name="filter">The serialized view filter string</param>
    /// <returns>List with added ViewFilterModels</returns>
    public static List<ViewFilterModel> Parse(this List<ViewFilterModel> l, string filter)
    {
        List<ViewFilterModel> result = l ?? new List<ViewFilterModel>();
        var filterSerializer = new ViewFilterUtility();
        result.AddRange(filterSerializer.Deserialize(filter));

        return result;
    }
}
