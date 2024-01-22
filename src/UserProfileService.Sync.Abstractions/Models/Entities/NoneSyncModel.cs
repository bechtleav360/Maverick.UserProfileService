using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Models.Entities;

/// <summary>
///     This model is only thought as default value.
///     It is done for register a none handler.
/// </summary>
public class NoneSyncModel : ISyncModel
{
    /// <inheritdoc />
    public IList<KeyProperties> ExternalIds { get; set; }

    /// <inheritdoc />
    public string Id { get; set; }

    /// <inheritdoc />
    public List<ObjectRelation> RelatedObjects { get; set; }

    /// <inheritdoc />
    public string Source { get; set; }
}
