using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.Models;
using RangeCondition = Maverick.UserProfileService.Models.Models.RangeCondition;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     The tree edge relation contains all relevant information for a relation within a tree.
/// </summary>
public class FirstLevelProjectionTreeEdgeRelation
{
    /// <summary>
    ///     The child that consist out of an id and a type.
    /// </summary>
    public ObjectIdent Child { get; set; }

    /// <summary>
    ///     Defines the date time condition for object assignments.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     The parent container that includes all relevant information.
    /// </summary>
    public IFirstLevelProjectionContainer Parent { get; set; }

    /// <summary>
    ///     The tag assignments that the parent contains.
    /// </summary>
    public IList<TagAssignment> ParentTags { get; set; }

    /// <summary>
    ///     The constructor that creates an instance of <see cref="FirstLevelProjectionTreeEdgeRelation" />.
    /// </summary>
    /// <param name="child">The child that consist out of an id and a type.</param>
    /// <param name="parent">
    ///     The parent container that includes all relevant information.
    /// </param>
    public FirstLevelProjectionTreeEdgeRelation(ObjectIdent child, IFirstLevelProjectionContainer parent)
    {
        Child = child;
        Parent = parent;
    }

    /// <summary>
    ///     The default constructor.
    /// </summary>
    public FirstLevelProjectionTreeEdgeRelation()
    {
    }
}
