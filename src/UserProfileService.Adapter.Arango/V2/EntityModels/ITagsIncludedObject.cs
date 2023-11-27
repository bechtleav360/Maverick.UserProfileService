using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public interface ITagsIncludedObject
{
    List<CalculatedTag> Tags { get; set; }
}
