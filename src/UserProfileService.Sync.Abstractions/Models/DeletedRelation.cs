using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Implementation of <see cref="IRelation" /> for deleted relations.
/// </summary>
[Model(SyncConstants.Models.DeletedRelation)]
public class DeletedRelation : Relation
{
    /// <summary>
    ///     Create an instance of <see cref="DeletedRelation" />
    /// </summary>
    /// <param name="originalObject">Object the relations belongs to.</param>
    /// <param name="relatedObjects">The related objects of the <paramref name="originalObject" />, ex. children of groups.</param>
    public DeletedRelation(LookUpObject originalObject, List<ObjectRelation> relatedObjects = null) : base(
        originalObject,
        relatedObjects)
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="DeletedRelation" />
    /// </summary>
    /// <param name="relation">Relation to create a new instance from.</param>
    public DeletedRelation(IRelation relation) : base(relation)
    {
    }
}
