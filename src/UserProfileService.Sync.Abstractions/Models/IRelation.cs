using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Shows a relations between an object that exists.
///     The relation can be an assignment, parent or child.
/// </summary>
public interface IRelation
{
    /// <summary>
    ///     An <see cref="ILookUpObject" /> that is stored for the original object.
    /// </summary>
    LookUpObject OriginalObject { set; get; }

    /// <summary>
    ///     Stored the relations object in the list for the
    ///     original object.
    /// </summary>
    List<ObjectRelation> RelatedObjects { set; get; }
}
