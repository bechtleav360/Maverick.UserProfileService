using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;

internal class FirstLevelProjectionTraversalResponse<TEntity>
{
    /// <summary>
    ///     States if the start vertex was found.
    /// </summary>
    public bool IsStartVertexKnown { get; set; }

    /// <summary>
    ///     Contains the actual result of the traversal.
    /// </summary>
    public List<TEntity> Response { get; set; } = new List<TEntity>();
}
