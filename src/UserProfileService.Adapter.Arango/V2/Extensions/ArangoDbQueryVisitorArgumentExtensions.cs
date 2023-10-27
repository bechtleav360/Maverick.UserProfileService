using System;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class ArangoDbQueryVisitorArgumentExtensions
{
    internal static VisitorMethodArgument InvertInequation(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.InequationInverted = true;
        }

        return argument;
    }

    internal static VisitorMethodArgument ShouldBeCastTo(
        this VisitorMethodArgument argument,
        Type targetType)
    {
        if (argument != null && targetType != null)
        {
            argument.CastTo = targetType;
        }

        return argument;
    }

    internal static VisitorMethodArgument AddPreProcessing(
        this VisitorMethodArgument argument,
        Func<string, string> preProcessingFunction,
        bool condition)
    {
        if (condition && argument != null && preProcessingFunction != null)
        {
            argument.PreInitStringProcessing = preProcessingFunction;
        }

        return argument;
    }

    internal static VisitorMethodArgument IsCustom(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.IsCustom = true;
        }

        return argument;
    }

    internal static VisitorMethodArgument IgnoreKey(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.IgnoreKey = true;
        }

        return argument;
    }

    internal static VisitorMethodArgument IsInsideArrayInlineProjection(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.IsInsideArrayInlineProjection = true;
        }

        return argument;
    }

    internal static VisitorMethodArgument IsNestedLambda(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.IsNestedLambda = true;
        }

        return argument;
    }

    internal static VisitorMethodArgument IsLeftSide(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.IsLeftSide = true;
        }

        return argument;
    }

    internal static VisitorMethodArgument ShouldReturnObjectResult(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.ShouldReturnObjectResult = true;
        }

        return argument;
    }

    internal static VisitorMethodArgument SetParentFieldName(
        this VisitorMethodArgument argument,
        string parentFieldName)
    {
        if (argument != null && !string.IsNullOrWhiteSpace(parentFieldName))
        {
            argument.ParentFieldName = parentFieldName;
        }

        return argument;
    }

    internal static VisitorMethodArgument ShouldInsertSubProperty(this VisitorMethodArgument argument)
    {
        if (argument != null)
        {
            argument.ShouldInsertSubProperty = true;
        }

        return argument;
    }
}
