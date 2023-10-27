using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     The synchronization model that is returned from the source system.
///     This model also contains the relation from an source object.
/// </summary>
public interface ISyncModel
{
    /// <summary>
    ///     A collection of ids that are used to identify the resource in an external source.
    /// </summary>
    IList<KeyProperties> ExternalIds { get; set; }

    /// <summary>
    ///     The internal Maverick identifier.
    /// </summary>
    string Id { set; get; }

    /// <summary>
    ///     All associated objects that are linked to the group.
    ///     Depending on the system, the list can be empty,
    ///     and the current group is stored as linked at the respective linked object.
    /// </summary>
    List<ObjectRelation> RelatedObjects { set; get; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    string Source { get; set; }
}
