using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents an object with tags.
/// </summary>
public interface ITagsIncludedObject
{
    /// <summary>
    ///     Gets or sets a list of tags.
    /// </summary>
    List<CalculatedTag> Tags { get; set; }
}
