using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Is used to identify an object stored in the destination system (originally coming from source).
///     The object id can be put together through various keys. If an unique identifier is not present the filter
///     can be used to set up a key.
/// </summary>
public class KeyProperties : ExternalIdentifier
{
    /// <summary>
    ///     The filter can be used if the source object no unique id und have to be
    ///     assembled from more properties.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Filter Filter { set; get; }

    /// <summary>
    ///     The post filter can be used if the source object no unique id und have to be
    ///     assembled from more properties, which can not be executed on database.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Func<IEnumerable<object>, IEnumerable<object>> PostFilter { set; get; }

    /// <summary>
    ///     Create an instance of <see cref="KeyProperties" />
    /// </summary>
    /// <param name="id">A unique id that identifies the object from a source system.</param>
    /// <param name="source">Key of the source system the identifier is from.</param>
    /// <param name="filter">
    ///     The filter can be used if the source object no unique id und have to be assembled from more
    ///     properties.
    /// </param>
    /// <param name="isConverted"> True if the id has been converted before, else false. </param>
    public KeyProperties(string id, string source, Filter filter = null, bool isConverted = false) : base(
        id,
        source,
        isConverted)
    {
        Filter = filter;
    }
}
