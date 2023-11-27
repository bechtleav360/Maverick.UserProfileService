using System;
using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

public class VisitorMethodArgument
{
    public Type CastTo { get; set; }
    public string ExpressionId { get; set; }
    public bool IgnoreKey { get; set; }

    public bool
        InequationInverted
    {
        get;
        set;
    } // if the character of the operator should be inverted: a > b => b < a, > will be <

    public bool IsCustom { get; set; }
    public bool IsGeneric { get; set; }

    // If property is set inside array inline projection, set to true => usage of "CURRENT" as iterator
    public bool IsInsideArrayInlineProjection { get; set; }
    public bool IsLeftSide { get; set; }
    public bool IsNestedLambda { get; set; }
    public bool IsOnlyTypeFilter { get; set; }
    public string ListFilterAqlExpression { get; set; }
    public bool MemberExpected { get; set; }
    public IList<NestedPropertyInformation> NestedPropertyInformation { get; set; }

    public string ParentFieldName { get; set; }

    public Func<string, string> PreInitStringProcessing { get; set; }
    public bool ShouldInsertSubProperty { get; set; }
    public bool ShouldReturnObjectResult { get; set; }

    public VisitorMethodArgument()
    {
    }

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

    public static VisitorMethodArgument CreateInstance(VisitorMethodArgument old)
    {
        return old != null
            ? new VisitorMethodArgument(old)
            : new VisitorMethodArgument();
    }
}
