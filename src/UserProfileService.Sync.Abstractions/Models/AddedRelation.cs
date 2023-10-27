using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Implementation of <see cref="IRelation" /> for added relations.
/// </summary>
[Model(SyncConstants.Models.AddedRelation)]
public class AddedRelation : Relation
{
    /// <summary>
    ///     Create an instance of <see cref="AddedRelation" />
    /// </summary>
    /// <param name="originalObject">Object the relations belongs to.</param>
    /// <param name="relatedObjects">The related objects of the <paramref name="originalObject" />, ex. children of groups.</param>
    public AddedRelation(LookUpObject originalObject, List<ObjectRelation> relatedObjects = null) : base(
        originalObject,
        relatedObjects)
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="AddedRelation" />
    /// </summary>
    /// <param name="relation">Relation to create a new instance from.</param>
    public AddedRelation(IRelation relation) : base(relation)
    {
    }
}
