using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a role entity model.
/// </summary>
public class RoleObjectEntityModel : RoleView, IAssignmentObjectEntity
{
    /// <inheritdoc />
    public IList<RangeCondition> Conditions { get; set; }

    /// <inheritdoc />
    [JsonProperty(AConstants.IdSystemProperty)]
    public string SystemId { get; }

    /// <summary>
    ///     Gets or sets a list of tags assigned to this role.
    /// </summary>
    public List<CalculatedTag> Tags { get; set; }
}
