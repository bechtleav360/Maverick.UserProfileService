using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     An ArangoDB tree visitor that processes filter expression trees.
/// </summary>
public sealed class ArangoDbFilterTreeVisitor : ArangoDbTreeVisitorBase
{
    private List<string> _filterStrings;
    private ModelBuilderOptions _options;
    private StringBuilder _strings = new StringBuilder();

    internal ArangoDbFilterTreeVisitor()
    {
        When<CustomMemberExpression>(VisitCustomMemberExpression);
        When<RawQueryExpression>(VisitRawQueryExpression);
        When<EntityTypeFilterExpression>(VisitEntityTypeFilterExpression);
    }

    private void VisitExpressionDetails(
        ExpressionDetails expression,
        bool isOnlyTypeFilter)
    {
        var argument = new VisitorMethodArgument
        {
            IsGeneric = expression.IsGeneric,
            ExpressionId = expression.ExpressionId,
            IsOnlyTypeFilter = isOnlyTypeFilter,
            NestedPropertyInformation = expression.NestedPropertyInformation
        };

        if (expression.BatchedExpressions?.Count > 0)
        {
            _strings.Append("(");

            // to keep it clean, avoid too many '(', ')'
            if (expression.BatchedExpressions.Count > 1)
            {
                _strings.Append("(");
            }

            for (var i = 0; i < expression.BatchedExpressions.Count; i++)
            {
                if (i > 0)
                {
                    _strings.Append(") OR (");
                }

                Visit(expression.BatchedExpressions[i], argument);
            }

            // to keep it clean, avoid too many '(', ')'
            if (expression.BatchedExpressions.Count > 1)
            {
                _strings.Append(")");
            }

            _strings.Append(")");
        }
        else
        {
            Visit(expression.UsedExpression, argument);
        }
    }

    private Expression VisitEntityTypeFilterExpression(
        Expression node,
        VisitorMethodArgument argument)
    {
        if (node is not EntityTypeFilterExpression)
        {
            throw new ArgumentException("Wrong type of expression.", nameof(node));
        }

        return Expression.Empty();
    }

    private Expression VisitRawQueryExpression(
        Expression node,
        VisitorMethodArgument argument)
    {
        if (node is not RawQueryExpression raw)
        {
            throw new ArgumentException("Wrong type of expression.", nameof(node));
        }

        string usedKey = argument?.IgnoreKey == true
            ? string.Empty
            : $"{Key}.";

        _strings.Append(raw.RawQueryText ?? raw.RawStringBuilder.Invoke($"{usedKey}{raw.MemberInformation.Name}"));

        return Expression.Empty();
    }

    private static bool TryProcessContainsOperatorFiltering(
        string oldMessage,
        string propertyName,
        IList<string> propertyNames,
        string filterAqlExpression,
        out Func<string, bool, string> messageCreator)
    {
        if (propertyNames == null || propertyNames.Count == 0 || string.IsNullOrWhiteSpace(oldMessage))
        {
            messageCreator = default;

            return false;
        }

        messageCreator =
            (values, useAll) => GetAqlArrayExpansionString(
                propertyName,
                filterAqlExpression,
                iterator => GetAqlOfContainsOperatorInContainsMethod(
                    values,
                    $"{iterator}.{propertyNames.First()}",
                    useAll));

        return true;
    }

    private static (string aql, List<string> foundProperties, string fieldName) GetAdvancedAqlFilterInfos(
        string current,
        MemberInfo member,
        bool addAny,
        IList<NestedPropertyInformation> nestedProperties,
        string listFilterAqlExpression)
    {
        if (member is not PropertyInfo propertyInfo)
        {
            return (current, null, null);
        }

        bool isArray = typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);

        Type elementType = isArray
            ? propertyInfo.PropertyType.GetElementType()
            ?? propertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault()
            : null;

        if (elementType == null && (nestedProperties == null || !nestedProperties.Any()))
        {
            return (current, null, current);
        }

        if (elementType == typeof(string) || elementType?.IsPrimitive == true)
        {
            return (addAny
                    ? string.Concat(current, " ANY")
                    : current, null,
                current);
        }

        string additionalFilter = isArray && !string.IsNullOrWhiteSpace(listFilterAqlExpression)
            ? $" FILTER {listFilterAqlExpression.AdjustCurrentTerms()}"
            : null;

        if (nestedProperties != null && nestedProperties.Any(p => p.MethodToUse != null))
        {
            // either one or all except the method "property" (like Count)
            List<string> suffixProperties = nestedProperties
                .Where(p => p.MethodToUse == null)
                .Select(p => p.PropertyName)
                .ToList();

            string aql = GetAqlToCountSequence(
                suffixProperties.Count > 0
                    ? string.Concat(
                        current,
                        ".",
                        string.Join(".", suffixProperties),
                        additionalFilter != null ? "[*" : null,
                        additionalFilter,
                        additionalFilter != null ? "]" : null)
                    : string.Concat(
                        current,
                        additionalFilter != null ? "[*" : null,
                        additionalFilter,
                        additionalFilter != null ? "]" : null));

            return (aql, suffixProperties, current);
        }

        if (nestedProperties != null && nestedProperties.Any())
        {
            return (string.Concat(
                    current,
                    isArray ? $"[*{additionalFilter}]." : ".",
                    string.Join(
                        ".",
                        nestedProperties
                            .Select(
                                (
                                        p,
                                        i)
                                    =>
                                    $"{p.PropertyName}{(p.IsList && i < nestedProperties.Count - 1 ? "[*]" : "")}")),
                    addAny && isArray ? " ANY" : ""),
                nestedProperties.Select(p => p.PropertyName).ToList(), null);
        }

        PropertyInfo cAtt = elementType?
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(
                p => p.GetCustomAttributes(true)
                    .Any(a => a.GetType() == typeof(DefaultFilterValueAttribute))
                    ? new
                    {
                        Weight = 3,
                        Pi = p
                    }
                    : p.Name == "Name"
                        ? new
                        {
                            Weight = 2,
                            Pi = p
                        }
                        : p.Name == "Id"
                            ? new
                            {
                                Weight = 1,
                                Pi = p
                            }
                            : new
                            {
                                Weight = 0,
                                Pi = p
                            })
            .MaxBy(e => e.Weight)
            ?
            .Pi;

        return (cAtt != null
                ? $"{current}[*{additionalFilter}].{cAtt.Name}{(addAny ? " ANY" : "")}"
                : current,
            cAtt != null
                ? new List<string>
                {
                    cAtt.Name
                }
                : null, null);
    }

    private static string GetPropertyValue(
        object value,
        Type castTargetType = null,
        // ReSharper disable once IdentifierTypo => should be correct
        bool? invertInequation = false,
        string relatedPropertyName = null,
        Func<string, string> preInitStringProcessing = null)
    {
        if (value == null)
        {
            return "\"\"";
        }

        if (value.GetType().IsPrimitive)
        {
            switch (value)
            {
                case string s:
                    return $"\"{(preInitStringProcessing != null ? preInitStringProcessing.Invoke(s) : s)}\"";
                case double d:
                    return d.ToString("F");
                case float f:
                    return f.ToString("F");
                case int i:
                    if (castTargetType != null && castTargetType != typeof(int))
                    {
                        break;
                    }

                    return i.ToString("D");
                case long l:
                    if (castTargetType != null && castTargetType != typeof(long))
                    {
                        break;
                    }

                    return l.ToString("D");
                case bool b:
                    return b.ToString();
                default:
                    return value.ToString();
            }
        }

        if (value is FilterOperator filterOperator)
        {
            return invertInequation == true
                ? filterOperator.InvertOperator().ConvertToAqlOperatorString()
                : filterOperator.ConvertToAqlOperatorString();
        }

        // if castTargetType is an enum type, the string representation should be used inside AQL instead of the casted/converted value.
        if (castTargetType != null
            && typeof(Enum).IsAssignableFrom(castTargetType)
            && value is int or long
            && Enum.IsDefined(castTargetType, value))
        {
            return ((Enum)Enum.ToObject(castTargetType, value)).GetPropertyValueToFilter();
        }

        IList<string> conversionIssues = null;

        if ((castTargetType == null || castTargetType == typeof(string))
            && preInitStringProcessing != null
            && value is IEnumerable<string> targetStringList)
        {
            return JsonConvert.SerializeObject(targetStringList.Select(preInitStringProcessing).ToList());
        }

        if (TryToConvertEnumerableToType(value, castTargetType, ref conversionIssues, out string propertyValue))
        {
            return propertyValue;
        }

        if (conversionIssues is
            {
                Count: > 0
            })
        {
            throw new ValidationException(
                string.Concat(
                    "Could not convert input value to appropriate property type. ",
                    "Expected type: ",
                    castTargetType?.Name ?? "unknown",
                    relatedPropertyName != null
                        ? $"{Environment.NewLine}Related field: {relatedPropertyName}"
                        : string.Empty,
                    Environment.NewLine,
                    "Following issues detected:",
                    Environment.NewLine,
                    "* ",
                    string.Join($"{Environment.NewLine}* ", conversionIssues)))
            {
                Data =
                {
                    { "RequestError", true },
                    { "InvalidFilter", true },
                    { "RelatedField", relatedPropertyName }
                }
            };
        }

        return JsonConvert.SerializeObject(value);
    }

    private static bool TryToConvertEnumerableToType(
        object value,
        Type castTargetType,
        ref IList<string> conversionIssues,
        out string propertyValue)
    {
        propertyValue = default;

        if (castTargetType == null || value is not IEnumerable<string> stringList)
        {
            return false;
        }

        if (stringList.TryConvertToType(
                castTargetType,
                out IList<object> converted,
                out conversionIssues))
        {
            propertyValue = JsonConvert.SerializeObject(converted);

            return true;
        }

        return false;
    }

    private Expression VisitCustomMemberExpression(
        Expression node,
        VisitorMethodArgument argument)
    {
        if (node is not CustomMemberExpression custom)
        {
            throw new ArgumentException("Wrong type of input parameter", nameof(node));
        }

        (bool customParameterMethod, Expression resultingExpr) = VisitCustomFilter(custom);

        if (customParameterMethod)
        {
            return resultingExpr;
        }

        // default methods (i.e. for strings)
        switch (custom.MethodInfo.Name)
        {
            case nameof(string.StartsWith):
                ProcessStartsWithMethod(argument, custom);

                break;
            case nameof(string.Contains):
                ProcessContainsMethod(argument, custom);

                break;
            case nameof(string.EndsWith):
                ProcessEndsWithMethod(argument, custom);

                break;
            case nameof(ValidationHelper.IsNullOrEmpty):
                ProcessMethodIsNullOrEmpty(custom);

                break;
            case nameof(ComparingHelpers.SequenceEqual):
                ProcessSequenceEqualOfMethod(argument, custom);

                break;
            case nameof(ComparingHelpers.ContainsValueOf):
                ProcessContainsValueOfMethod(argument, custom);

                break;
            case nameof(ComparingHelpers.CheckExistenceOfElementsInTwoSequences):
                ProcessAnyElementOfOneSequenceInAnother(argument, custom);

                break;
            case nameof(Enumerable.Any):
                ProcessAnyMethodOfEnumerable(argument, custom);

                break;
            case nameof(Enumerable.Count):
                ProcessCountOfSequence(argument, custom);

                break;
            case nameof(ComparingHelpers.CheckExistenceOfElements):
                ProcessContainsValueOfMethod(argument, custom);

                break;
        }

        return Expression.Empty();
    }

    private void ProcessStartsWithMethod(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        TextResultExpression obj =
            Visit(
                custom.Object,
                new VisitorMethodArgument
                {
                    IsCustom = true
                }) as TextResultExpression
            ?? throw new NotSupportedException("No text result expression found as object.");

        TextResultExpression arg = Visit(
                    custom.CalledArguments.First(),
                    VisitorMethodArgument.CreateInstance(argument).IsCustom()) as
                TextResultExpression
            ?? throw new NotSupportedException("No text result expression found as argument.");

        _strings.Append($"LIKE({obj.Result},\"{EscapeAqlLikeExpression(arg.Result).Trim('"')}%\",true)");
    }

    private void ProcessContainsMethod(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        Expression containsObjExpression =
            Visit(
                custom.Object,
                new VisitorMethodArgument
                {
                    IsCustom = true,
                    MemberExpected = true
                });

        // if the list object is a reference to a field/property of the ArangoDb object ...
        if (containsObjExpression is TextResultExpression containsObj)
        {
            TextResultExpression containsArg =
                Visit(
                    custom.CalledArguments.First(),
                    VisitorMethodArgument.CreateInstance(argument).IsCustom()) as TextResultExpression
                ?? throw new NotSupportedException("No text result expression found as first argument.");

            _strings.Append(
                $"LIKE({containsObj.Result},\"%{EscapeAqlLikeExpression(containsArg.Result).Trim('"')}%\",true)");

            return;
        }

        // if the list object is a C# object (=> constant value), the AQL should be different (or somehow vice-versa).
        if (containsObjExpression is ObjectResultExpression objExpression)
        {
            // the argument will be the property/field of the entity and should be a reference to a field in ArangoDb

            string collectionString = ConversionUtilities.GetStringOfObjectForAql(objExpression.Result);

            TextResultExpression containsArg =
                Visit(
                    custom.CalledArguments.First(),
                    new VisitorMethodArgument().IsCustom()) as TextResultExpression
                ?? throw new NotSupportedException(
                    "No text result expression found as first argument Should be a property reference.");

            _strings.Append($"{collectionString} ANY == {containsArg.Result}");

            return;
        }

        throw new NotSupportedException("This visitor does not support this constellation of 'Contains' arguments.");
    }

    private void ProcessEndsWithMethod(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        TextResultExpression endsWithObj =
            Visit(
                custom.Object,
                new VisitorMethodArgument
                {
                    IsCustom = true
                }) as TextResultExpression
            ?? throw new NotSupportedException("No text result expression found as object.");

        TextResultExpression endsWithArg =
            Visit(
                custom.CalledArguments.First(),
                VisitorMethodArgument.CreateInstance(argument).IsCustom()) as TextResultExpression
            ?? throw new NotSupportedException("No text result expression found as argument.");

        _strings.Append(
            $"LIKE({endsWithObj.Result},\"%{EscapeAqlLikeExpression(endsWithArg.Result).Trim('"')}\",true)");
    }

    private void ProcessMethodIsNullOrEmpty(CustomMemberExpression custom)
    {
        if (custom.CalledArguments?.FirstOrDefault() == null)
        {
            throw new Exception("Visit(IsNullOrEmpty): Wrong number of arguments! Missing input argument.");
        }

        var input = Visit(
                custom.CalledArguments[0],
                new VisitorMethodArgument()
                    .IsCustom())
            as TextResultExpression;

        _strings.Append($"({input}==null OR COUNT({input})==0)");
    }

    private void ProcessSequenceEqualOfMethod(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        if (custom.CalledArguments?.FirstOrDefault() == null)
        {
            throw new Exception("Visit(SequenceEqual): Wrong number of arguments! Missing first argument.");
        }

        if (custom.CalledArguments?.ElementAtOrDefault(1) == null)
        {
            throw new Exception("Visit(SequenceEqual): Wrong number of arguments! Missing second argument.");
        }

        (string filterAqlExpression, string _, Type _) = GetFilterAqlExpressionAndResolverInfo(
            custom.CalledArguments.ElementAtOrDefault(2),
            custom.CalledArguments.ElementAtOrDefault(3));

        var firstArgument = Visit(
                custom.CalledArguments[0],
                new VisitorMethodArgument
                    {
                        NestedPropertyInformation =
                            argument.NestedPropertyInformation,
                        ListFilterAqlExpression = filterAqlExpression
                    }
                    .IsCustom()
                    .IsLeftSide()
                    .ShouldInsertSubProperty())
            as TextResultExpression;

        var secondArgument = Visit(custom.CalledArguments[1], new VisitorMethodArgument().IsCustom())
            as TextResultExpression;

        if (firstArgument == null)
        {
            throw new Exception(
                $"Internal error occurred in query builder. Could not resolve first argument in {nameof(ProcessSequenceEqualOfMethod)}.");
        }

        if (secondArgument == null)
        {
            throw new Exception(
                $"Internal error occurred in query builder. Could not resolve second argument in {nameof(ProcessSequenceEqualOfMethod)}.");
        }

        _strings.Append(firstArgument);
        _strings.Append(" IN ");
        _strings.Append(secondArgument);
    }

    private static string GetAqlToCountSequence(string propertyRepresentation)
    {
        return $"COUNT({propertyRepresentation})";
    }

    private void ProcessCountOfSequence(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        if (custom.CalledArguments?.FirstOrDefault() == null)
        {
            throw new Exception("Visit(ProcessCountOfSequence): Wrong number of arguments! Missing first argument.");
        }

        // if expression contains a filter/where clause, ...
        if (custom.CalledArguments.Length == 2)
        {
            ProcessConditionalCountOfSequence(argument, custom);

            return;
        }

        var firstArgument = Visit(
                custom.CalledArguments[0],
                VisitorMethodArgument.CreateInstance(argument)
                    .IsCustom())
            as TextResultExpression;

        if (string.IsNullOrWhiteSpace(firstArgument?.Result))
        {
            throw new Exception(
                $"Internal error occurred in query builder. Could not resolve first argument in {nameof(ProcessCountOfSequence)}.");
        }

        _strings.Append(GetAqlToCountSequence(firstArgument.Result));
    }

    private void ProcessConditionalCountOfSequence(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        if (custom.CalledArguments?.FirstOrDefault() == null)
        {
            throw new Exception(
                $"Internal error occurred in query builder in {nameof(ProcessConditionalCountOfSequence)}(). Wrong number of arguments! Missing first argument.");
        }

        if (custom.CalledArguments.ElementAtOrDefault(1) == null)
        {
            throw new Exception(
                $"Internal error occurred in query builder in {nameof(ProcessConditionalCountOfSequence)}(). Wrong number of arguments! Missing second argument.");
        }

        var firstArgument = Visit(
                custom.CalledArguments[0],
                VisitorMethodArgument.CreateInstance(argument)
                    .IsCustom())
            as TextResultExpression;

        if (string.IsNullOrWhiteSpace(firstArgument?.Result))
        {
            throw new Exception(
                $"Internal error occurred in query builder in {nameof(ProcessConditionalCountOfSequence)}(). Could not resolve first argument.");
        }

        if (custom.CalledArguments[1] is not LambdaExpression la)
        {
            throw new Exception(
                $"Internal error occurred in query builder in {nameof(ProcessConditionalCountOfSequence)}(). The second argument is of wrong type!");
        }

        var secondArgument = Visit(
                la.Body,
                VisitorMethodArgument.CreateInstance(argument)
                    .IsCustom()
                    .IsInsideArrayInlineProjection())
            as IAqlExpression;

        if (string.IsNullOrWhiteSpace(secondArgument?.GetAqlString()))
        {
            throw new Exception(
                $"Internal error occurred in query builder. Could not resolve second argument in {nameof(ProcessConditionalCountOfSequence)}.");
        }

        _strings.Append(
            GetAqlToCountSequence($"NOT_NULL({firstArgument},[])[* FILTER {secondArgument.GetAqlString()}]"));
    }

    private void ProcessAnyElementOfOneSequenceInAnother(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        if (custom.CalledArguments?.FirstOrDefault() == null)
        {
            throw new Exception(
                "Visit(ProcessAnyElementOfOneSequenceInAnother): Wrong number of arguments! Missing first argument.");
        }

        if (custom.CalledArguments?.ElementAtOrDefault(1) == null)
        {
            throw new Exception(
                "Visit(ProcessAnyElementOfOneSequenceInAnother): Wrong number of arguments! Missing second argument.");
        }

        if (custom.CalledArguments?.ElementAtOrDefault(2) == null)
        {
            throw new Exception(
                "Visit(ProcessAnyElementOfOneSequenceInAnother): Wrong number of arguments! Missing third argument.");
        }

        (string aqlExpression, string _, Type _) = GetFilterAqlExpressionAndResolverInfo(
            custom.CalledArguments.ElementAtOrDefault(3),
            custom.CalledArguments.ElementAtOrDefault(4));

        var firstArgument = Visit(
                custom.CalledArguments[0],
                new VisitorMethodArgument
                    {
                        NestedPropertyInformation =
                            argument.NestedPropertyInformation
                    }
                    .AddPreProcessing(
                        ConversionUtilities.ToEscapedStringSuitableForLikeInAql,
                        true)
                    .IsCustom())
            as TextResultExpression;

        if (string.IsNullOrWhiteSpace(firstArgument?.Result))
        {
            throw new Exception(
                $"Internal error occurred in query builder. Could not resolve first argument in {nameof(ProcessSequenceEqualOfMethod)}.");
        }

        var secondArgument = Visit(
                custom.CalledArguments[1],
                new VisitorMethodArgument
                    {
                        ListFilterAqlExpression = aqlExpression
                    }
                    .IsCustom()
                    .ShouldReturnObjectResult()
                    .IsLeftSide()
                    .ShouldInsertSubProperty())
            as ObjectResultExpression;

        if (secondArgument?.Result is not Func<string, bool, string> creator)
        {
            throw new Exception(
                $"Internal error occurred in query builder. Could not resolve second argument in {nameof(ProcessSequenceEqualOfMethod)}.");
        }

        var thirdArgument = Visit(
                custom.CalledArguments[2],
                new VisitorMethodArgument()
                    .IsCustom()
                    .ShouldReturnObjectResult())
            as ObjectResultExpression;

        if (thirdArgument?.Result is not bool result)
        {
            throw new Exception(
                $"Internal error occurred in query builder. Could not resolve second argument in {nameof(ProcessSequenceEqualOfMethod)}.");
        }

        _strings.Append(creator.Invoke(firstArgument.Result, result));
    }

    private void ProcessAnyMethodOfEnumerable(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        if (custom.CalledArguments == null || custom.CalledArguments.Length < 2)
        {
            throw new NotSupportedException(
                $"Wrong number of arguments: This visitor {nameof(ArangoDbFilterTreeVisitor)} does not support 'Any()' without two arguments.");
        }

        var collectionAqlExpression = Visit(
            custom.CalledArguments[0],
            VisitorMethodArgument
                .CreateInstance(argument)
                // custom visitor method calls don't store their result in the global result _string
                .IsCustom()) as TextResultExpression;

        Expression lambdaAqlExpression =
            Visit(
                custom.CalledArguments[1],
                VisitorMethodArgument
                    .CreateInstance(argument)
                    // custom visitor method calls don't store their result in the global result _string
                    .IsCustom()
                    .IgnoreKey()
                    .IsNestedLambda());

        if (collectionAqlExpression == null
            || lambdaAqlExpression is not BinaryTextResultExpression lambdaAqlBinaryExpression)
        {
            throw new Exception(
                $"Internal error in {nameof(ArangoDbFilterTreeVisitor)}: Visit(Any()) could not determine input arguments in a correct way.");
        }

        _strings.Append($"{collectionAqlExpression.Result}[*].{lambdaAqlBinaryExpression.LeftSideResult} ");
        _strings.Append($"ANY {lambdaAqlBinaryExpression.OperatorString} ");
        _strings.Append($"{lambdaAqlBinaryExpression.RightSideResult}");
    }

    private void ProcessContainsValueOfMethod(
        VisitorMethodArgument argument,
        CustomMemberExpression custom)
    {
        if (custom.CalledArguments?.FirstOrDefault() == null)
        {
            throw new Exception("Visit(ContainsValueOf): Wrong number of arguments! Missing first argument.");
        }

        if (custom.CalledArguments?.ElementAtOrDefault(1) == null)
        {
            throw new Exception("Visit(ContainsValueOf): Wrong number of arguments! Missing second argument.");
        }

        if (custom.CalledArguments?.ElementAtOrDefault(2) == null)
        {
            throw new Exception("Visit(ContainsValueOf): Wrong number of arguments! Missing third argument.");
        }

        if (custom.CalledArguments?.ElementAtOrDefault(3) == null)
        {
            throw new Exception("Visit(ContainsValueOf): Wrong number of arguments! Missing fourth argument.");
        }

        (string filterAqlExpression, string conversionMethod, Type returnType) =
            GetFilterAqlExpressionAndResolverInfo(
                custom.CalledArguments.ElementAtOrDefault(4),
                custom.CalledArguments.ElementAtOrDefault(5));

        // to retrieve result of casting operation (if there has been one done)
        VisitorMethodArgument visitorOption = new VisitorMethodArgument
            {
                NestedPropertyInformation =
                    argument?.NestedPropertyInformation,
                ListFilterAqlExpression = filterAqlExpression
            }
            .IsCustom()
            .ShouldInsertSubProperty();

        // if the AQL generation process is "normal", the visitor should visit the property sub-tree as left side property of th equation.
        if (string.IsNullOrWhiteSpace(conversionMethod))
        {
            visitorOption.IsLeftSide();
        }

        var propertyNameExpression = Visit(custom.CalledArguments[0], visitorOption)
            as TextResultExpression;

        var operatorArgument = Visit(
                custom.CalledArguments[2],
                new VisitorMethodArgument()
                    .IsCustom()
                    .InvertInequation())
            as TextResultExpression;

        Type specificEstimatedType =
            returnType
            ?? argument?.NestedPropertyInformation
                ?.FirstOrDefault(p => p.MethodToUse != null)
                ?.MethodToUse
                ?.ReturnType;

        bool isContainsQuery =
            operatorArgument?.RelatedObject is FilterOperator.Contains;

        // comparison value will be an IEnumerable<string>, i.e. ["2","8","7"]
        // the field name will be passed due of better exception handling
        var comparisonExpression = Visit(
                custom.CalledArguments[1],
                new VisitorMethodArgument()
                    .IsCustom()
                    .ShouldBeCastTo(specificEstimatedType ?? visitorOption.CastTo)
                    .SetParentFieldName(propertyNameExpression?.RelatedFieldName)
                    .AddPreProcessing(
                        ConversionUtilities.ToEscapedStringSuitableForLikeInAql,
                        isContainsQuery))
            as TextResultExpression;

        var allMustBeContainedArgument = Visit(
                custom.CalledArguments[3],
                new VisitorMethodArgument()
                    .IsCustom()
                    .ShouldReturnObjectResult())
            as ObjectResultExpression;

        bool useAllInQuery = allMustBeContainedArgument?.Result != null && (bool)allMustBeContainedArgument.Result;

        if (isContainsQuery && CheckTypeCanBeContained(comparisonExpression?.Type))
        {
            _strings.Append(
                GetAqlOfContainsOperatorInContainsMethod(
                    comparisonExpression?.Result,
                    propertyNameExpression?.Result.GetModifiedAqlString(conversionMethod),
                    useAllInQuery));

            return;
        }

        _strings.Append(
            comparisonExpression); // in AQL "[2,8,7] ANY > x.Number": that's why inequation operator must be inverted/flipped

        _strings.Append(useAllInQuery ? " ALL " : " ANY ");
        _strings.Append(operatorArgument?.Result ?? "==");
        _strings.Append(propertyNameExpression?.Result.GetModifiedAqlString(conversionMethod));
    }

    private (string aql, string conversionMethod, Type returnType) GetFilterAqlExpressionAndResolverInfo(
        Expression propertyExpression,
        Expression valueExpression)
    {
        if (propertyExpression == null)
        {
            return (null, null, null);
        }

        var propertyName =
            Visit(propertyExpression, new VisitorMethodArgument().IsCustom().ShouldReturnObjectResult()) as
                ObjectResultExpression;

        if (propertyName?.Result is IVirtualPropertyResolver resolver)
        {
            var textResult = Visit(
                resolver.GetFilterExpression(),
                new VisitorMethodArgument()
                    .IsCustom()
                    .IsNestedLambda()
                    .IsInsideArrayInlineProjection()) as BinaryTextResultExpression;

            if (!string.IsNullOrWhiteSpace(textResult?.GetAqlString())
                || !string.IsNullOrWhiteSpace(resolver.Conversion))
            {
                return (textResult?.GetAqlString(), resolver.Conversion, resolver.GetReturnType());
            }
        }

        if (propertyName?.Result is not string s || string.IsNullOrWhiteSpace(s))
        {
            return (null, null, null);
        }

        if (valueExpression == null)
        {
            return (null, null, null);
        }

        var propertyValue =
            Visit(valueExpression, new VisitorMethodArgument().IsCustom().ShouldReturnObjectResult()) as
                ObjectResultExpression;

        if (propertyValue?.Result == null)
        {
            return (null, null, null);
        }

        return ($"{s}=={ConversionUtilities.GetStringOfObjectForAql(propertyValue.Result)}", null, null);
    }

    private static string GetAqlOfContainsOperatorInContainsMethod(
        string comparisonExpression,
        string propertyNameExpression,
        bool useAllInQuery)
    {
        if (string.IsNullOrEmpty(comparisonExpression) || string.IsNullOrEmpty(propertyNameExpression))
        {
            return string.Empty;
        }

        // ArangoDb query should look like:
        // FOR x IN Whatever FILTER ["input", "values"][* RETURN LIKE(x.Name, CONCAT("%",CURRENT,"%"), true)] ANY == true RETURN x
        //                          °°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°
        // The underlined filter query should be returned by this method. Don't forget to escape % and _ in input values!

        return string.Concat(
            comparisonExpression,
            "[* RETURN LIKE(",
            propertyNameExpression,
            ",CONCAT(\"%\",CURRENT,\"%\"),true)]",
            useAllInQuery
                ? "ALL"
                : "ANY",
            "==true");
    }

    private (bool, Expression) VisitCustomFilter(CustomMemberExpression expression)
    {
        ParameterInfo[] parameters = expression?.MethodInfo?.GetParameters();

        if (parameters == null || parameters.Length == 0)
        {
            return (false, expression);
        }

        if (parameters[0].ParameterType == typeof(IList<string>)
            || typeof(IList<string>).IsAssignableFrom(parameters[0].ParameterType))
        {
            return (true, VisitFilterObjectListCustomFilter(expression));
        }

        return (false, expression);
    }

    private Expression VisitFilterObjectListCustomFilter(CustomMemberExpression expression)
    {
        switch (expression.MethodInfo.Name)
        {
            case nameof(InternalFilterObjectExtensions.AllTagsIncludedIn):
                if (expression.CalledArguments?.Length != 2)
                {
                    throw new ArgumentException(
                        "Expression is not valid. Number of called arguments must be 2.",
                        nameof(expression));
                }

                TextResultExpression obj =
                    Visit(expression.CalledArguments[1], new VisitorMethodArgument().IsCustom()) as
                        TextResultExpression
                    ?? throw new NotSupportedException(
                        $"No text result expression found as expected (Method name: {expression.MethodInfo.Name}).");

                ObjectResultExpression arg =
                    Visit(
                            expression.CalledArguments[0],
                            new VisitorMethodArgument()
                                .IsCustom()
                                .ShouldReturnObjectResult()) as
                        ObjectResultExpression
                    ?? throw new NotSupportedException(
                        $"No object result expression found as expected (Method name: {expression.MethodInfo.Name}).");

                if (arg.Result is not IEnumerable<string> e)
                {
                    throw new Exception(
                        $"Type mismatch in {nameof(VisitFilterObjectListCustomFilter)}(): The visitor result should be an array of strings. (Method name: {expression.MethodInfo.Name})");
                }

                string[] list = e
                    .Where(elem => !string.IsNullOrWhiteSpace(elem))
                    .Select(elem => elem.ToLowerInvariant())
                    .ToArray();

                if (list.Length == 0)
                {
                    throw new Exception(
                        $"Exception in {nameof(VisitFilterObjectListCustomFilter)}(): The array of filtered tags should contain at least one term that is not null or an empty string. (Method name: {expression.MethodInfo.Name})");
                }

                // This method is only used for CalculatedTags.
                // Therefore the name property can be set hard-coded, instead of calculating it
                // (i.e. with GetAdvancedAqlFilterInfos(...)).
                _strings.Append(
                    $"[\"{string.Join("\",\"", list)}\"] ALL IN NOT_NULL({obj.Result},[])[* RETURN LOWER(CURRENT.{nameof(CalculatedTag.Name)})]");

                return expression;
            default:
                throw new NotSupportedException(
                    $"{nameof(VisitFilterObjectListCustomFilter)} call: This method is not supported by this visitor class! (Method name: {expression.MethodInfo.Name})");
        }
    }

    private void AddTypedPropertyFilter<T>(
        Expression<T> node,
        VisitorMethodArgument argument)
    {
        if ((argument != null
                && (
                    argument.IsGeneric
                    // custom visitor method calls don't store their result in the global result _string
                    || argument.IsCustom
                ))
            || node?.Type.GenericTypeArguments == null
            || node.Type.GenericTypeArguments.Length == 0)
        {
            return;
        }

        TryGetTypedPropertyFilter(
            node.Type.GenericTypeArguments[0],
            argument,
            out string query);

        _strings.Append(query);
    }

    private bool TryGetTypedPropertyFilter(
        Type modelType,
        VisitorMethodArgument argument,
        out string query)
    {
        if (argument is
            {
                IsGeneric: true
            }
            || modelType == null)
        {
            query = null;

            return false;
        }

        bool skipSecondCondition = argument?.IsOnlyTypeFilter == true;

        ModelBuilderOptions modelOptions = GetModelOptions();
        (string typeProperty, object typeValue) = modelOptions?.GetTypeInformation(modelType) ?? default;

        if (typeProperty == null || typeValue == null)
        {
            query = null;

            return false;
        }

        string usedKey = argument?.IgnoreKey == true
            ? string.Empty
            : $"{Key}.";

        query =
            $"{usedKey}{typeProperty} == {ConversionUtilities.GetStringOfObjectForAql(typeValue)}{(skipSecondCondition ? string.Empty : " AND ")}";

        return true;
    }

    private static string EscapeAqlLikeExpression(string input)
    {
        return input?.Replace("_", "\\\\_").Replace("%", "\\\\%");
    }

    private static bool TryGetConstantExpressionValue(
        MemberExpression memberAccess,
        out object value)
    {
        if (memberAccess?.Expression is not ConstantExpression cExpr || memberAccess.Member is not FieldInfo fInfo)
        {
            value = null;

            return false;
        }

        value = fInfo.GetValue(cExpr.Value);

        return true;
    }

    private static string ConvertToString(ExpressionType expressionType)
    {
        return expressionType switch
        {
            ExpressionType.Equal => "==",
            ExpressionType.NotEqual => "!=",
            ExpressionType.OrElse => " OR ",
            ExpressionType.AndAlso => " AND ",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(expressionType), expressionType, null)
        };
    }

    private static bool CheckTypeCanBeContained(Type type)
    {
        return type == null || (type != typeof(DateTime) && !type.IsPrimitive);
    }

    internal static string GetAqlArrayExpansionString(
        string propertyName,
        string filterAqlExpression,
        Func<string, string> returnCreator)
    {
        string additionalFilter =
            !string.IsNullOrWhiteSpace(filterAqlExpression)
                ? $"[* FILTER CURRENT.{filterAqlExpression}]"
                : null;

        return
            $"(FOR insideProperty IN NOT_NULL({propertyName},[]){additionalFilter} RETURN {returnCreator.Invoke("insideProperty")}) ANY == true";
    }

    /// <inheritdoc />
    protected override ModelBuilderOptions GetModelOptions()
    {
        return _options;
    }

    /// <inheritdoc />
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        return VisitLambda(node, null);
    }

    /// <inheritdoc />
    protected override Expression VisitLambda<T>(
        Expression<T> node,
        VisitorMethodArgument argument)
    {
        if ((argument == null || !argument.IsNestedLambda) && !TryResolveAndUpdateKey(node))
        {
            return node;
        }

        AddTypedPropertyFilter(node, argument);

        Expression body = Visit(node.Body, argument);

        if (argument == null || !argument.IsNestedLambda)
        {
            Key = null;
        }

        return argument == null || !argument.IsNestedLambda
            ? node
            : body;
    }

    /// <inheritdoc />
    protected override Expression VisitBinary(BinaryExpression node)
    {
        return VisitBinary(node, null);
    }

    /// <inheritdoc />
    protected override Expression VisitBinary(
        BinaryExpression node,
        VisitorMethodArgument argument)
    {
        string GetAppropriateString(Expression input)
        {
            return input switch
            {
                TextResultExpression tResult => tResult.Result,
                IAqlExpression aqlExpr => aqlExpr.GetAqlString(),
                _ => null
            };
        }

        if (argument == null || !argument.IsCustom)
        {
            _strings.Append("(");
        }

        Expression left = Visit(node.Left, argument);

        if (argument == null || !argument.IsCustom)
        {
            // operator
            _strings.Append(ConvertToString(node.NodeType));
        }

        Expression right = Visit(node.Right, argument);

        if (argument == null || !argument.IsCustom)
        {
            _strings.Append(")");

            return node;
        }

        return
            new BinaryTextResultExpression(
                GetAppropriateString(left),
                ConvertToString(node.NodeType),
                GetAppropriateString(right));
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression node)
    {
        return VisitMember(
            node,
            new VisitorMethodArgument
            {
                IsCustom = false
            });
    }

    /// <inheritdoc />
    protected override Expression VisitMember(
        MemberExpression node,
        VisitorMethodArgument arg)
    {
        if (Key == null)
        {
            throw new ArgumentOutOfRangeException(nameof(node), $"Missing or wrong key ('{Key}').");
        }

        // normally it should not be a constant expression => useful, because this method will be called twice and the outer "call" won't return a text result expression.
        if (TryGetConstantExpressionValue(node, out object val))
        {
            if (arg is
                {
                    IsCustom: true
                }
                && (arg.MemberExpected || arg.ShouldReturnObjectResult))
            {
                return new ObjectResultExpression(val);
            }

            if (arg is not
                {
                    ShouldReturnObjectResult: true
                })
            {
                string converted = ConversionUtilities.GetStringOfObjectForAql(val);

                if (arg is not
                    {
                        IsCustom: true
                    })
                {
                    _strings.Append(converted);
                }

                return new TextResultExpression(converted);
            }

            _strings.Append(ConversionUtilities.GetStringOfObjectForAql(val));

            return new ObjectResultExpression(val);
        }

        if (node.Expression is
            {
                NodeType: ExpressionType.MemberAccess
            }
            && Visit(
                    node.Expression,
                    VisitorMethodArgument.CreateInstance(arg).IsCustom().ShouldReturnObjectResult())
                is ObjectResultExpression
                constantExpr)
        {
            if (node.Member is PropertyInfo pInfo)
            {
                // if its a list, return the object result, because it has to be prepared further
                if (typeof(IEnumerable<string>).IsAssignableFrom(pInfo.PropertyType))
                {
                    return new ObjectResultExpression(pInfo.GetValue(constantExpr.Result, null));
                }

                return new TextResultExpression(
                    ConversionUtilities.GetStringOfObjectForAql(pInfo.GetValue(constantExpr.Result, null)));
            }

            if (node.Member is FieldInfo fInfo)
            {
                return new TextResultExpression(
                    ConversionUtilities.GetStringOfObjectForAql(fInfo.GetValue(constantExpr.Result)));
            }
        }

        string usedKey = arg?.IgnoreKey == true
            ? string.Empty
            : arg?.IsInsideArrayInlineProjection == true
                ? "CURRENT."
                : $"{Key}.";

        string message;
        List<string> foundPropertyNames = null;
        string relatedFieldName = null;

        if (arg?.ShouldInsertSubProperty == true)
        {
            (message, foundPropertyNames, relatedFieldName) = GetAdvancedAqlFilterInfos(
                $"{usedKey}{node.Member.Name}",
                node.Member,
                arg.IsLeftSide,
                arg.NestedPropertyInformation,
                arg.ListFilterAqlExpression);
        }
        else
        {
            message = $"{usedKey}{node.Member.Name}";
        }

        if (arg is
            {
                ShouldReturnObjectResult: true,
                IsLeftSide: true
            }
            && TryProcessContainsOperatorFiltering(
                message,
                $"{usedKey}{node.Member.Name}",
                foundPropertyNames,
                arg.ListFilterAqlExpression,
                out Func<string, bool, string> messageCreator))
        {
            return new ObjectResultExpression(messageCreator);
        }

        if (arg is not
            { IsCustom: true })
        {
            _strings.Append(message);

            return node;
        }

        return new TextResultExpression(
            message,
            arg.IsLeftSide
                ? relatedFieldName ?? message
                : null);
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression node)
    {
        return VisitConstant(
            node,
            new VisitorMethodArgument
            {
                IsCustom = false
            });
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(
        ConstantExpression node,
        VisitorMethodArgument argument)
    {
        if (argument is { ShouldReturnObjectResult: true })
        {
            return new ObjectResultExpression(node.Value);
        }

        if (argument is { IsCustom: false })
        {
            _strings.Append(GetPropertyValue(node.Value));

            return Expression.Empty();
        }

        string propertyValue = GetPropertyValue(
            node.Value,
            argument?.CastTo,
            argument?.InequationInverted,
            argument?.ParentFieldName,
            argument?.PreInitStringProcessing);

        return new TextResultExpression(propertyValue, argument?.CastTo, relatedObject: node.Value);
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        return Visit(
                new CustomMemberExpression(node),
                new VisitorMethodArgument
                {
                    IsCustom = true
                })
            ?? node;
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(
        MethodCallExpression node,
        VisitorMethodArgument argument)
    {
        return Visit(new CustomMemberExpression(node), VisitorMethodArgument.CreateInstance(argument).IsCustom())
            ?? node;
    }

    /// <inheritdoc />
    protected override Expression VisitBlock(BlockExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitConditional(ConditionalExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitDebugInfo(DebugInfoExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitDefault(DefaultExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitDynamic(DynamicExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override ElementInit VisitElementInit(ElementInit node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitGoto(GotoExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitIndex(IndexExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitInvocation(InvocationExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitLabel(LabelExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override LabelTarget VisitLabelTarget(LabelTarget node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitListInit(ListInitExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitLoop(LoopExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override MemberBinding VisitMemberBinding(MemberBinding node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitNew(NewExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitSwitch(SwitchExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override SwitchCase VisitSwitchCase(SwitchCase node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitTry(TryExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(
        UnaryExpression node,
        VisitorMethodArgument argument)
    {
        if (node.NodeType != ExpressionType.Convert)
        {
            throw new InvalidOperationException("This is not supported by this kind of visitor.");
        }

        //especially for kind enums like profile kind
        if (node.Type == typeof(int) && typeof(Enum).IsAssignableFrom(node.Operand.Type))
        {
            return Visit(node.Operand, argument.ShouldBeCastTo(node.Operand.Type));
        }

        if (node.Type == typeof(object) || node.Type == typeof(DateTime) || node.Type == typeof(DateTime?))
        {
            return Visit(node.Operand, argument.ShouldBeCastTo(node.Operand.Type));
        }

        if (node.Method != null && node.Method.IsStatic)
        {
            MethodCallExpression calling = Expression.Call(null, node.Method, node.Operand);

            return Visit(calling, argument.IsCustom());
            //return Visit(node.Operand, argument.ShouldBeCastTo(node.Operand.Type));
        }

        throw new InvalidOperationException("This is not supported by this kind of visitor.");
    }

    /// <inheritdoc />
    public override SubTreeVisitorResult GetResultExpression(
        IArangoDbEnumerable enumerable,
        CollectionScope collectionScope)
    {
        Lock.Wait(1);
        _filterStrings = new List<string>();

        VarMapping = new Dictionary<string, string>();
        _options = enumerable.GetEnumerable().ModelSettings;
        Scope = collectionScope;

        try
        {
            var step = 0;

            List<ExpressionDetails> expressions = enumerable.GetEnumerable().WhereExpressions;

            var isOnlyTypeFilter = false;

            if (expressions == null || expressions.Count == 0)
            {
                if (!TryGetTypedPropertyFilter(enumerable.GetInnerType(), null, out _))
                {
                    return new SubTreeVisitorResult
                    {
                        ReturnString = null,
                        CollectionToIterationVarMapping = null
                    };
                }

                isOnlyTypeFilter = true;

                expressions = new List<ExpressionDetails>
                {
                    new ExpressionDetails(
                        enumerable.GetEnumerable().LastRequestId,
                        Expression.Lambda(
                            typeof(Action<>).MakeGenericType(enumerable.GetInnerType()),
                            new EntityTypeFilterExpression(),
                            Expression.Parameter(enumerable.GetInnerType())))
                };
            }

            foreach (ExpressionDetails expression in expressions)
            {
                _strings = new StringBuilder();
                Key = null;
                bool? and = null;

                if (expression.CombinedByAnd.HasValue)
                {
                    and = expression.CombinedByAnd;
                }

                and ??= enumerable.GetEnumerable()
                    .TryGetExpressionsSettings(expression, out CombinedQuerySettings s)
                    ? s.CombinedByAnd
                    : null;

                if (expression is NestedExpressionDetails nested)
                {
                    _strings.Append("(");

                    for (var index = 0; index < nested.Children.Count; index++)
                    {
                        if (index > 0)
                        {
                            _strings.Append(
                                nested.BinaryOperator == BinaryOperator.And
                                    ? " AND "
                                    : " OR ");
                        }

                        VisitExpressionDetails(nested.Children[index], isOnlyTypeFilter);
                    }

                    _strings.Append(")");
                }
                else
                {
                    VisitExpressionDetails(expression, isOnlyTypeFilter);
                }

                if (step++ > 0)
                {
                    _filterStrings.Add(and ?? false ? " AND " : " OR ");
                }

                _filterStrings.Add(_strings.ToString());

                _strings = new StringBuilder();
            }

            string temp = string.Concat(
                "FILTER",
                _filterStrings.Count > 1
                    ? "("
                    : " ",
                string.Join(string.Empty, _filterStrings),
                _filterStrings.Count > 1
                    ? ")"
                    : " ");

            var result = new SubTreeVisitorResult
            {
                ReturnString = temp,
                CollectionToIterationVarMapping =
                    new Dictionary<string, string>(VarMapping, StringComparer.OrdinalIgnoreCase)
            };

            _filterStrings = null;
            VarMapping = null;

            return result;
        }
        finally
        {
            Lock.Release(1);
        }
    }

    /// <inheritdoc />
    public override Expression Visit(Expression node)
    {
        throw new InvalidOperationException("This method should not be used in this context!");
    }

    private class CustomMemberExpression : Expression
    {
        public Expression[] CalledArguments { get; }

        public MethodInfo MethodInfo { get; }
        public override ExpressionType NodeType { get; }
        public Expression Object { get; }
        public override Type Type { get; }

        public CustomMemberExpression(MethodCallExpression node)
        {
            NodeType = node.NodeType;
            Type = GetType();
            CalledArguments = node.Arguments.ToArray();
            MethodInfo = node.Method;
            Object = node.Object;
        }
    }

    private class TextResultExpression : Expression
    {
        public override ExpressionType NodeType { get; }
        public string RelatedFieldName { get; }
        public object RelatedObject { get; }

        public string Result { get; }
        public override Type Type { get; }

        public TextResultExpression(
            string result,
            Type relatedType,
            string relatedFieldName = null,
            object relatedObject = null)
            : this(result, relatedFieldName, relatedObject)
        {
            Type = relatedType ?? relatedObject?.GetType() ?? typeof(TextResultExpression);
        }

        public TextResultExpression(
            string result,
            string relatedFieldName = null,
            object relatedObject = null)
        {
            Result = result;
            Type = relatedObject?.GetType() ?? typeof(TextResultExpression);
            NodeType = ExpressionType.Constant;
            RelatedFieldName = relatedFieldName;
            RelatedObject = relatedObject;
        }

        public override string ToString()
        {
            return Result;
        }
    }

    private class ObjectResultExpression : Expression
    {
        public override ExpressionType NodeType { get; }

        public object Result { get; }
        public override Type Type { get; }

        public ObjectResultExpression(object result)
        {
            Result = result;
            Type = typeof(ObjectResultExpression);
            NodeType = ExpressionType.Constant;
        }

        public override string ToString()
        {
            return Result?.ToString() ?? string.Empty;
        }
    }
}
