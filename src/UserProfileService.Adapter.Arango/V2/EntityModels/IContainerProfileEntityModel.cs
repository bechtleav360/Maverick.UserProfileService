using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public interface IContainerProfileEntityModel : IProfileEntityModel, IContainerProfile
{
    IList<Member> Members { set; get; }
}
