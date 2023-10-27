using System.Text.RegularExpressions;
using Sprache;
using UserProfileService.Queries.Language.ExpressionOperators;
using UserProfileService.Queries.Language.TreeDefinition;
using UserProfileService.Queries.Language.Utilities;

namespace UserProfileService.Queries.Language.Grammar;

/// <summary>
///     The filter query grammar is used to parse a filter-Operation string and return
///     a tree that can be traversed.
/// </summary>
public sealed class FilterQueryGrammar
{
    /// <summary>
    ///     The binary operator are used to parse an expression an return the <see cref="ExpressionCombinator" />.
    /// </summary>
    internal readonly Parser<ExpressionCombinator> BinaryOperator = Parse
        .String(FilterExpressionCombinator.OrCombinator)
        .Return(ExpressionCombinator.Or)
        .Or(
            Parse.String(
                    FilterExpressionCombinator
                        .AndCombinator)
                .Return(ExpressionCombinator.And));

    /// <summary>
    ///     The current position in the tree.
    /// </summary>
    internal BinaryExpressionNode? CurrentNode;

    /// <summary>
    ///     The root of the tree that will be returned.
    /// </summary>
    internal RootNode? RootNode;

    /// <summary>
    ///     The query string is save for debug reasons and can be found at the root of the
    ///     returned tree.
    /// </summary>
    private readonly string? _queryString;

    /// <summary>
    ///     The binary expression is used as a look-ahead to evaluated if the string has and binaryExpression.
    ///     If no binary expression exists, the tree has only a root with one leaf.
    /// </summary>
    internal Parser<ExpressionCombinator> BinaryOperatorAsLookAheadParser =>
        Parse.RegexMatch(new Regex($".*\\s+{FilterExpressionCombinator.OrCombinator}\\s+", RegexOptions.IgnoreCase))
            .Return(ExpressionCombinator.Or)
            .Or(
                Parse.RegexMatch(
                        new Regex($".*\\s+{FilterExpressionCombinator.AndCombinator}\\s+", RegexOptions.IgnoreCase))
                    .Return(ExpressionCombinator.And))
            .Or(Parse.Return(ExpressionCombinator.None));

    /// <summary>
    ///     An Expression that consists out of a left and right-hand-side an is combined with an operator.
    /// </summary>
    internal Parser<TreeNode> ExpressionNode =>
        from leftHandSide in LeftHandSide.Token()
        from operatorExpression in OperatorEnum.Token()
        from rightHandSideValue in InnerValue.Token()
        select CreateExpressionNode(leftHandSide, rightHandSideValue, operatorExpression);

    /// <summary>
    ///     The inner not quoted value that is normally a number.
    /// </summary>
    internal Parser<string> InnerNotQuotedValue =>
        from dataType in Parse.Regex("[A-Za-z]*").Text().Token()
        from innerValueWithoutQuotation in Parse.Number.Text().Token().Named("InnerNotQuotedValue(right-side)")
        select innerValueWithoutQuotation;

    /// <summary>
    ///     The inner value of an expression that contains a quoted value.
    /// </summary>
    internal Parser<string> InnerQuotedValue =>
        from dataType in Parse.Regex("[A-Za-z]*").Text().Token()
        from openQuotedValue in Parse.Chars(
            QuotedCharacter.SingleQuotedCharacter,
            QuotedCharacter.SingleQuotedCharacterForStrings)
        from innerValue in Parse.CharExcept(
                new List<char>
                {
                    QuotedCharacter.SingleQuotedCharacter,
                    QuotedCharacter.SingleQuotedCharacterForStrings
                })
            .Many()
            .Text()
            .Named("QuoatedValue(right-side)")
            .Token()
        from closeQuotedValue in Parse.Chars(
            QuotedCharacter.SingleQuotedCharacter,
            QuotedCharacter.SingleQuotedCharacterForStrings)
        select innerValue;

    /// <summary>
    ///     The inner value of an expression that can contain a quoted or unquoted value.
    /// </summary>
    internal Parser<string> InnerValue => InnerQuotedValue.Or(InnerNotQuotedValue);

    /// <summary>
    ///     The left hand side is not nested an can be parsed.
    /// </summary>
    internal Parser<string> LeftHandNotNested =>
        Parse.Regex("[A-Za-z]+").Text().Token().Named("LeftHandSideExpressionNotNested");

    /// <summary>
    ///     The left hand side of an expression. Here we check if the left hand side
    ///     references nested attributes/value.
    /// </summary>
    internal Parser<string> LeftHandSide =>
        from checkForNestedValues in Parse.Regex("/+").Preview()
        from parserClause in checkForNestedValues.GetOrDefault() == string.Empty
            ? LeftHandNotNested
            : LeftHandSideNested
        select parserClause;

    /// <summary>
    ///     The left hand side is nested and has to be parsed. The "/"
    ///     will be replaced through the "." .
    /// </summary>
    internal Parser<string> LeftHandSideNested =>
        Parse.RegexMatch("[a-zA-z/]+")
            .Select(p => p.Groups[0].Value.Replace("/", "."))
            .Text()
            .Token()
            .Named("LeftHandSideExpressionNested");

    /// <summary>
    ///     The operator enum parser an operator and return the equivalent <see cref="OperatorType" />.
    /// </summary>
    internal Parser<OperatorType> OperatorEnum =>
        Parse.String(ExpressionOperators.ExpressionOperators.EqualsOperator)
            .Return(OperatorType.EqualsOperator)
            .Or(
                Parse.IgnoreCase(ExpressionOperators.ExpressionOperators.GreaterThenOperator)
                    .Return(OperatorType.GreaterThenOperator))
            .Or(
                Parse.IgnoreCase(ExpressionOperators.ExpressionOperators.GreaterEqualsOperator)
                    .Return(OperatorType.GreaterEqualsOperator))
            .Or(
                Parse.IgnoreCase(ExpressionOperators.ExpressionOperators.LessThenOperator)
                    .Return(OperatorType.LessThenOperator))
            .Or(
                Parse.IgnoreCase(ExpressionOperators.ExpressionOperators.LessEqualsOperator)
                    .Return(OperatorType.LessEqualsOperator))
            .Or(
                Parse.IgnoreCase(ExpressionOperators.ExpressionOperators.NotEqualsOperator)
                    .Return(OperatorType.NotEqualsOperator))
            .Or(
                Parse.IgnoreCase(ExpressionOperators.ExpressionOperators.EqualsOperator)
                    .Return(OperatorType.EqualsOperator))
            .Or(
                Parse.IgnoreCase(ExpressionOperators.ExpressionOperators.ContainsOperator)
                    .Return(OperatorType.Contains));

    /// <summary>
    ///     The parser combines the one or more expressions with a binary operator
    ///     (current supported operators are "and" and "or" operators).
    /// </summary>
    internal Parser<TreeNode> QueryBinaryParser =>
        Parse.ChainOperator(
            BinaryOperator,
            ExpressionNode,
            CreateBinaryExpressionLeaf);

    /// <summary>
    ///     This parser os used to created a root with one expression.
    ///     The filter string than has only one expression.
    /// </summary>
    internal Parser<TreeNode> RootNodeParsed =>
        from expressionNodeString in ExpressionNode
        select CreateRootNodeExpression(
            expressionNodeString,
            FilterOption.DollarFilter);

    /// <summary>
    ///     Parses the string and return the root node of the tree.
    /// </summary>
    internal Parser<TreeNode> TreeParser =>
        from parser in QueryBinaryParser
        select RootNode;

    /// <summary>
    ///     The binary look-ahead is used to decide if the tree has an only one or more expressions,
    ///     that is combined with a binary operator.
    /// </summary>
    public Parser<TreeNode> QueryFilterParser =>
        from operatorExpression in BinaryOperatorAsLookAheadParser.Preview()
        from selectionTrying in operatorExpression.GetOrDefault() == ExpressionCombinator.None
            ? RootNodeParsed
            : TreeParser
        select selectionTrying;

    /// <summary>
    ///     Creates an object of type <see cref="FilterQueryGrammar" />.
    /// </summary>
    /// <param name="queryString">The query string that the filter grammar should process.</param>
    public FilterQueryGrammar(string queryString)
    {
        _queryString = queryString;
    }

    /// <summary>
    ///     Is used to create an expression node that contains a left and right-hand-side
    ///     that is combined by an operator.
    /// </summary>
    /// <param name="leftHandSide">The left hand side of an expression.</param>
    /// <param name="rightHandType">The right hand side of an expression.</param>
    /// <param name="operatorType">The operator that combines the the right adn left-hand-side expression.</param>
    /// <returns>Returns an <see cref="ExpressionNode" />.</returns>
    internal TreeNode CreateExpressionNode(string leftHandSide, string rightHandType, OperatorType operatorType)
    {
        if (string.IsNullOrWhiteSpace(leftHandSide))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(leftHandSide));
        }

        if (string.IsNullOrWhiteSpace(rightHandType))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rightHandType));
        }

        return new ExpressionNode(leftHandSide, rightHandType, operatorType);
    }

    /// <summary>
    ///     Created an binary expression tree that combines two expression with a binary operator.
    /// </summary>
    /// <param name="binaryOperator">The binary operator that is used to combines the expressions.</param>
    /// <param name="leftExpression">The left hand expression-</param>
    /// <param name="rightExpression">The right hand expression.</param>
    /// <returns>Returns a <see cref="BinaryExpressionNode" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     If the
    ///     <param ref="leftExpression" />
    ///     or
    ///     <param ref="rightExpression" />
    ///     are null.
    /// </exception>
    internal TreeNode CreateBinaryExpressionLeaf(
        ExpressionCombinator binaryOperator,
        TreeNode? leftExpression,
        TreeNode? rightExpression)
    {
        if (leftExpression == null)
        {
            throw new ArgumentNullException(nameof(leftExpression));
        }

        if (rightExpression == null)
        {
            throw new ArgumentNullException(nameof(rightExpression));
        }

        if (RootNode == null)
        {
            RootNode = new RootNode(FilterOption.DollarFilter, _queryString ?? string.Empty)
            {
                LeftChild = new BinaryExpressionNode(leftExpression, rightExpression, binaryOperator)
            };

            CurrentNode = (BinaryExpressionNode)RootNode.LeftChild;

            return CurrentNode;
        }

        var binaryExpressionTree = new BinaryExpressionNode(
            CurrentNode!.RightChild,
            rightExpression,
            binaryOperator);

        CurrentNode.RightChild = binaryExpressionTree;
        CurrentNode = binaryExpressionTree;

        return CurrentNode;
    }

    /// <summary>
    ///     Creates a root node that has only one child expression.
    /// </summary>
    /// <param name="treeNode">The tree node expression that will append to the root.</param>
    /// <param name="option">The filter string option that is used for the </param>
    /// <returns>Returns a root node that has only one child.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="treeNode" /> is null.</exception>
    internal TreeNode CreateRootNodeExpression(TreeNode? treeNode, FilterOption option)
    {
        if (treeNode == null)
        {
            throw new ArgumentNullException(nameof(treeNode));
        }

        RootNode = new RootNode(option, _queryString ?? string.Empty)
        {
            LeftChild = treeNode
        };

        return RootNode;
    }
}
