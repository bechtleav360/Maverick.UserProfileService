using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Implementation of <see cref="IRelation" />
/// </summary>
public class Relation : IRelation
{
    /// <summary>
    ///     An <see cref="ILookUpObject" /> that is stored for the original object.
    /// </summary>
    public LookUpObject OriginalObject { set; get; }

    /// <summary>
    ///     Stored the relations object in the list for the
    ///     original object.
    /// </summary>
    public List<ObjectRelation> RelatedObjects { set; get; }

    /// <summary>
    ///     Create an instance of <see cref="Relation" />
    /// </summary>
    /// <param name="originalObject">Object the relations belongs to.</param>
    /// <param name="relatedObjects">The related objects of the <paramref name="originalObject" />, ex. children of groups.</param>
    public Relation(LookUpObject originalObject, List<ObjectRelation> relatedObjects = null)
    {
        OriginalObject = originalObject;
        RelatedObjects = relatedObjects ?? new List<ObjectRelation>();
    }

    /// <summary>
    ///     Create an instance of <see cref="Relation" />
    /// </summary>
    /// <param name="relation">Relation to create a new instance from.</param>
    public Relation(IRelation relation)
    {
        OriginalObject = relation.OriginalObject;
        RelatedObjects = relation.RelatedObjects;
    }
}
