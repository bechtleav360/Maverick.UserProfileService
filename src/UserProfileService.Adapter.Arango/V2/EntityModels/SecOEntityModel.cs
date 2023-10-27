using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class SecOEntityModel : SecO
{
    public new IList<CalculatedTag> Tags { set; get; } = new List<CalculatedTag>();
}
