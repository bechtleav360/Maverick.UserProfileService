using System.Linq.Expressions;
using UserProfileService.Queries.Language.TreeDefinition;

namespace UserProfileService.Queries.Language.Visitors;

/// <summary>
///     The base visitor provides methods to travers a tree.
///     This class can be traversed an build a significant
///     query in a special language (i.e. SQL, Linq..)
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract class TreeNodeVisitorBase<TResult>
{
    /// <summary>
    ///     Describes how a <see cref="Expression" /> node should be visited.
    /// </summary>
    /// <param name="expression">The <see cref="BinaryExpressionNode" /> that should be visited.</param>
    /// <param name="optionalParameter">Optional parameter that are seperated though a comma.</param>
    /// <returns>
    ///     A
    ///     <typeparam ref="TResult" />
    ///     that will be created after visiting the <see cref="BinaryExpressionNode" />.
    /// </returns>
    protected abstract TResult VisitBinaryExpressionNode(
        BinaryExpressionNode expression,
        params object[] optionalParameter);

    /// <summary>
    ///     Describes how a <see cref="ExpressionNode" /> node should be visited.
    /// </summary>
    /// <param name="property">The <see cref="ExpressionNode" /> that should be visited.</param>
    /// <param name="optionalParameter">Optional parameter that are seperated though a comma.</param>
    /// <returns>
    ///     A
    ///     <typeparam ref="TResult" />
    ///     that will be created after visiting the <see cref="ExpressionNode" />.
    /// </returns>
    protected abstract TResult VisitExpressionNode(ExpressionNode property, params object[] optionalParameter);

    /// <summary>
    ///     The method controls how the node are processed.
    /// </summary>
    /// <param name="node">The node that should be handled.</param>
    /// <param name="optionalParameter">Optional parameter that are seperated though a comma.</param>
    /// <returns>
    ///     A
    ///     <typeparam ref="TResult" />
    ///     that will be created after visiting the <see cref="TreeNode" />.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="node" /> is out of range.</exception>
    protected virtual TResult VisitNode(TreeNode node, params object[] optionalParameter)
    {
        return node switch
        {
            ExpressionNode expressionNode => VisitExpressionNode(expressionNode, optionalParameter),
            BinaryExpressionNode binaryExpressionNode => VisitBinaryExpressionNode(
                binaryExpressionNode,
                optionalParameter),
            RootNode rootNode => VisitNode(rootNode.LeftChild, optionalParameter),
            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };
    }

    /// <summary>
    ///     Describes how a <see cref="RootNode" /> node should be visited.
    /// </summary>
    /// <param name="root">The <see cref="RootNode" /> that should be visited.</param>
    /// <param name="optionalParameter">Optional parameter that are seperated though a comma.</param>
    /// <returns>
    ///     A
    ///     <typeparam ref="TResult" />
    ///     that will be created after visiting the <see cref="RootNode" />.
    /// </returns>
    protected abstract TResult VisitRoot(RootNode root, params object[] optionalParameter);

    /// <summary>
    ///     The method returns an Expression that is valid for the Where-Linq-method.
    /// </summary>
    /// <param name="root">An <see cref="RootNode" /> of the tree that can be traversed.</param>
    /// <typeparam name="TModel">The explicit model for which the where-clause should be created.</typeparam>
    /// <returns>Returns an <see cref="Expression{TDelegate}" /> that can be used for the Linq-Where method.</returns>
    public abstract Expression<Func<TModel, bool>> Visit<TModel>(TreeNode? root);
}
