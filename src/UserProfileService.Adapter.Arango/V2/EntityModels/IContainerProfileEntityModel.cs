using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a container profile entity model.
/// </summary>
public interface IContainerProfileEntityModel : IProfileEntityModel, IContainerProfile
{
    /// <summary>
    ///     Gets or sets the list of member profiles of this container.
    /// </summary>
    IList<Member> Members { set; get; }
}
