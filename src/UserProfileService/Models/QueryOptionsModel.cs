using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace UserProfileService.Models;

/// <summary>
///     This model is derived here, so that we can rename the Filter
///     and OrderBy properties by an annotation.
///     The model from whom we deriving is in another namespace and has
///     not the opportunity (package) to user the annotations ( [FromQuery(Name = "")]).
/// </summary>
public class QueryOptionsModel : QueryOptions
{
    /// <summary>
    ///     The filter query that is used to filter the result objects.
    /// </summary>
    [FromQuery(Name = "$filter")]
    public override string Filter { set; get; }

    /// <summary>
    ///     The number of items to return.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public override int Limit { set; get; } = 50;

    /// <summary>
    ///     The number of items to skip before starting to collect the result set.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public override int Offset { set; get; } = 1;

    /// <summary>
    ///     The oder by query that is used to order the result objects for one
    ///     or more specific Property.
    /// </summary>
    [FromQuery(Name = "$orderBy")]
    public override string OrderBy { set; get; }
}
