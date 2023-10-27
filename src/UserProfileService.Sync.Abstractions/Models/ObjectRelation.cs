using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     The relation object that stores additionally the relations type.
///     The relation type is either a child, parent, or an assignment.
/// </summary>
public class ObjectRelation : ILookUpObject
{
    /// <summary>
    ///     The relation type of the object.
    /// </summary>
    public AssignmentType AssignmentType { set; get; }

    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="ObjectRelation" />. If it is empty or <c>null</c>, the
    ///     membership of this object is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <inheritdoc />
    public KeyProperties ExternalId { get; set; }

    /// <inheritdoc />
    public string MaverickId { get; set; }

    /// <inheritdoc />
    public ObjectType ObjectType { get; set; }

    /// <inheritdoc />
    public string Source { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="ObjectRelation" />
    /// </summary>
    /// <param name="assignmentType">The assignment type of the object.</param>
    /// <param name="externalId">The external id of the object from source.</param>
    /// <param name="maverickId">The created unique maverick id of the object.</param>
    /// <param name="objectType">The type of the object that stored the external und internal ids.</param>
    /// <param name="conditions">
    ///     A list if range conditions valid for this relation. It is null if the relationship is always
    ///     valid
    /// </param>
    public ObjectRelation(
        AssignmentType assignmentType,
        KeyProperties externalId,
        string maverickId,
        ObjectType objectType,
        IList<RangeCondition> conditions = null)
    {
        AssignmentType = assignmentType;
        ExternalId = externalId;
        MaverickId = maverickId;
        ObjectType = objectType;
        Conditions = conditions;
    }

    /// <summary>
    ///     Creates an instance of <see cref="ObjectRelation" />
    /// </summary>
    public ObjectRelation()
    {
    }
}
