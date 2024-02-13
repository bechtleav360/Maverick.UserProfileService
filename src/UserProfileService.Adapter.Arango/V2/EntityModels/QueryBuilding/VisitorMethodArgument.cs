using System;
using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Represents a visitor method argument.
/// </summary>
public class VisitorMethodArgument
{
    /// <summary>
    ///     Gets or sets the type to cast to.
    /// </summary>
    public Type CastTo { get; set; }
    /// <summary>
    ///     Gets or sets the expression ID.
    /// </summary>
    public string ExpressionId { get; set; }
    /// <summary>
    ///     Gets or sets a value indicating whether to ignore the key.
    /// </summary>
    public bool IgnoreKey { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the inequation operator should be inverted.
    ///     For example, if the character of the operator is ">" (a > b), it will be "&lt;" (b &lt; a).
    /// </summary>
    public bool
        InequationInverted
    {
        get;
        set;
    } // if the character of the operator should be inverted: a > b => b < a, > will be <

    /// <summary>
    ///     Gets or sets a value indicating whether this argument is custom.
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this argument is generic.
    /// </summary>
    public bool IsGeneric { get; set; }

    // If property is set inside array inline projection, set to true => usage of "CURRENT" as iterator
    /// <summary>
    ///     Gets or sets a value indicating whether the property is set inside an array inline projection.
    ///     If set to true, "CURRENT" is used as the iterator.
    /// </summary>
    public bool IsInsideArrayInlineProjection { get; set; }
    /// <summary>
    ///     Gets or sets a value indicating whether this argument represents the left side of an expression.
    /// </summary>
    public bool IsLeftSide { get; set; }
    /// <summary>
    ///     Gets or sets a value indicating whether this argument is part of a nested lambda expression.
    /// </summary>
    public bool IsNestedLambda { get; set; }
    /// <summary>
    ///     Gets or sets a value indicating whether this argument is used only for type filtering.
    /// </summary>
    public bool IsOnlyTypeFilter { get; set; }
    /// <summary>
    ///     Gets or sets the AQL expression for list filtering.
    /// </summary>
    public string ListFilterAqlExpression { get; set; }
    /// <summary>
    ///     Gets or sets a value indicating whether a member is expected.
    /// </summary>
    public bool MemberExpected { get; set; }
    /// <summary>
    ///     Gets or sets the nested property information.
    /// </summary>
    public IList<NestedPropertyInformation> NestedPropertyInformation { get; set; }

    /// <summary>
    ///     Gets or sets the parent field name.
    /// </summary>
    public string ParentFieldName { get; set; }

    /// <summary>
    ///     Gets or sets a function for pre-initializing string processing.
    /// </summary>
    public Func<string, string> PreInitStringProcessing { get; set; }
    /// <summary>
    ///     Gets or sets a value indicating whether a sub-property should be inserted.
    /// </summary>
    public bool ShouldInsertSubProperty { get; set; }
    /// <summary>
    ///     Gets or sets a value indicating whether the result should be an object.
    /// </summary>
    public bool ShouldReturnObjectResult { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="VisitorMethodArgument"/> class.
    /// </summary>
    public VisitorMethodArgument()
    {
    }

    // Copy constructor.
    private protected VisitorMethodArgument(VisitorMethodArgument old)
    {
        IsGeneric = old.IsGeneric;
        IsCustom = old.IsCustom;
        ExpressionId = old.ExpressionId;
        ShouldReturnObjectResult = old.ShouldReturnObjectResult;
        IsLeftSide = old.IsLeftSide;
        CastTo = old.CastTo;
        InequationInverted = old.InequationInverted;
        ParentFieldName = old.ParentFieldName;
        IsOnlyTypeFilter = old.IsOnlyTypeFilter;
        NestedPropertyInformation = old.NestedPropertyInformation;
        IsNestedLambda = old.IsNestedLambda;
        IgnoreKey = old.IgnoreKey;
        PreInitStringProcessing = old.PreInitStringProcessing;
        ShouldInsertSubProperty = old.ShouldInsertSubProperty;
        ListFilterAqlExpression = old.ListFilterAqlExpression;
        MemberExpected = old.MemberExpected;
    }

    /// <summary>
    ///     Creates either a copy of <paramref name="old"/>
    ///     or a new default instance of <see cref="VisitorMethodArgument"/>.
    /// </summary>
    /// <param name="old">The <see cref="VisitorMethodArgument"/> to copy from.</param>
    /// <returns>
    ///     A default <see cref="VisitorMethodArgument"/> if <paramref name="old"/> is <see langword="null"/>.
    ///     Otherwise a new instance with the values of <paramref name="old"/> copied over.
    /// </returns>
    public static VisitorMethodArgument CreateInstance(VisitorMethodArgument old)
    {
        return old != null
            ? new VisitorMethodArgument(old)
            : new VisitorMethodArgument();
    }
}
