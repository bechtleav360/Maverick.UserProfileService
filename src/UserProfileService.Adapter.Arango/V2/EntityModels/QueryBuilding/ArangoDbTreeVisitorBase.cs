using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Base class for an ArangoDB tree visitor that processes expression trees.
/// </summary>
public abstract class ArangoDbTreeVisitorBase : ExpressionVisitor, IDisposable
{
    private readonly Dictionary<Type, Func<Expression, VisitorMethodArgument, Expression>> _methodMapping
        = new Dictionary<Type, Func<Expression, VisitorMethodArgument, Expression>>();

    /// <summary>
    ///     A semaphore for thread-safe access (used for locking).
    /// </summary>
    protected readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

    /// <summary>
    ///     Mapping of variable names to collection keys.
    /// </summary>
    protected Dictionary<string, string> VarMapping;
    /// <summary>
    ///     Gets or sets the iteration number for generating unique keys.
    /// </summary>
    protected int IterationNumber { get; set; }
    /// <summary>
    ///     Gets or sets the current key associated with a collection or entity type.
    /// </summary>
    protected string Key { get; set; }
    /// <summary>
    ///     Gets or sets the scope of the collection (e.g. Query, Command).
    /// </summary>
    protected CollectionScope Scope { get; set; }

    /// <summary>
    ///     Gets or sets predefined collection name (optional).
    /// </summary>
    public string PredefineCollectionName { get; set; }

    private Expression VisitLambda(Type innerType, Expression node, VisitorMethodArgument argument)
    {
        MethodInfo method = typeof(ArangoDbTreeVisitorBase)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(
                m => m.Name == nameof(VisitLambda)
                    && m.IsVirtual
                    && m.ContainsGenericParameters
                    && m.IsGenericMethod
                    && m.GetParameters().Any(p => p.ParameterType == typeof(VisitorMethodArgument)))
            .MakeGenericMethod(innerType);

        try
        {
            var result = (Expression)method.Invoke(this, new object[] { node, argument });

            return result;
        }
        catch (TargetInvocationException e) when (e.InnerException is ValidationException)
        {
            throw e.InnerException;
        }
    }

    /// <summary>
    ///     Tries to resolve and update the key associated with a collection or entity type.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="node">The expression node (usually a lambda expression).</param>
    /// <returns><see langword="true"/> if the key was successfully resolved and updated; otherwise, <see langword="false"/>.</returns>
    protected bool TryResolveAndUpdateKey<T>(Expression<T> node)
    {
        if (VarMapping == null)
        {
            throw new Exception("Collection to variable mapping not initialized.");
        }

        if (!string.IsNullOrWhiteSpace(PredefineCollectionName))
        {
            if (VarMapping.TryGetValue(PredefineCollectionName, out string key))
            {
                Key = key;

                return true;
            }

            Key =
                $"{PredefineCollectionName.ToLowerInvariant()}{IterationNumber++}";

            VarMapping.Add(PredefineCollectionName, Key);

            return true;
        }

        if (node?.Type.GenericTypeArguments == null 
            || node.Type.GenericTypeArguments.Length == 0)
        {
            return false;
        }

        if (VarMapping.ContainsKey(GetCollectionName(node.Type.GenericTypeArguments[0], Scope)))
        {
            Key = VarMapping[GetCollectionName(node.Type.GenericTypeArguments[0], Scope)];

            return true;
        }

        Key =
            $"{node.Type.GenericTypeArguments[0].Name[..1].ToLowerInvariant()}{IterationNumber++}";

        VarMapping.Add(GetCollectionName(node.Type.GenericTypeArguments[0], Scope), Key);

        return true;
    }

    /// <summary>
    ///     Associates a visitor method with a specific expression type for dynamic dispatch.
    /// </summary>
    /// <typeparam name="TExpression">The type of the expression.</typeparam>
    /// <param name="method">The method to associate with the expression type.</param>
    protected void When<TExpression>(Func<Expression, VisitorMethodArgument, Expression> method)
        where TExpression : Expression
    {
        _methodMapping.Add(typeof(TExpression), method);
    }

    /// <summary>
    ///     Visits an expression node and performs specific actions based on its type.
    /// </summary>
    /// <param name="node">The expression node to visit.</param>
    /// <param name="argument">Additional arguments for the visitor method.</param>
    /// <returns>The modified or processed expression.</returns>
    protected Expression Visit(Expression node, VisitorMethodArgument argument)
    {
        if (_methodMapping.ContainsKey(node.GetType()))
        {
            return _methodMapping[node.GetType()].Invoke(node, argument);
        }

        if (node is BinaryExpression be)
        {
            return VisitBinary(be, argument);
        }

        if (node is ConstantExpression ce)
        {
            return VisitConstant(ce, argument);
        }

        if (node is MemberExpression me)
        {
            return VisitMember(me, argument);
        }

        if (node is MethodCallExpression mce)
        {
            return VisitMethodCall(mce, argument);
        }

        if (node is ParameterExpression pe)
        {
            return VisitParameter(pe, argument);
        }

        if (node is UnaryExpression ue)
        {
            return VisitUnary(ue, argument);
        }

        if (node.NodeType == ExpressionType.Lambda && node.GetType().GenericTypeArguments.Length > 0)
        {
            return VisitLambda(node.GetType().GenericTypeArguments[0], node, argument);
        }

        return base.Visit(node);
    }

    /// <inheritdoc cref="ExpressionVisitor.VisitBinary" />
    protected virtual Expression VisitBinary(BinaryExpression node, VisitorMethodArgument argument)
    {
        return VisitBinary(node);
    }

    /// <inheritdoc cref="ExpressionVisitor.VisitConstant" />
    protected virtual Expression VisitConstant(ConstantExpression node, VisitorMethodArgument argument)
    {
        return VisitConstant(node);
    }

    /// <inheritdoc cref="ExpressionVisitor.VisitMember" />
    protected virtual Expression VisitMember(MemberExpression node, VisitorMethodArgument argument)
    {
        return VisitMember(node);
    }

    /// <inheritdoc cref="ExpressionVisitor.VisitMethodCall" />
    protected virtual Expression VisitMethodCall(MethodCallExpression node, VisitorMethodArgument argument)
    {
        return VisitMethodCall(node);
    }

    /// <inheritdoc cref="ExpressionVisitor.VisitLambda{T}" />
    protected virtual Expression VisitLambda<T>(Expression<T> node, VisitorMethodArgument argument)
    {
        return VisitLambda(node);
    }

    /// <inheritdoc cref="ExpressionVisitor.VisitParameter" />
    protected virtual Expression VisitParameter(ParameterExpression node, VisitorMethodArgument argument)
    {
        return VisitParameter(node);
    }

    /// <inheritdoc cref="ExpressionVisitor.VisitUnary" />
    protected virtual Expression VisitUnary(UnaryExpression node, VisitorMethodArgument argument)
    {
        return VisitUnary(node);
    }

    /// <summary>
    ///     Gets the options for the model builder.
    /// </summary>
    /// <returns>The model builder options.</returns>
    protected abstract ModelBuilderOptions GetModelOptions();

    /// <summary>
    ///     Gets the collection name for the specified entity type and collection scope.
    /// </summary>
    /// <param name="entityType">The type to get the collection name for.</param>
    /// <param name="collectionScope"></param>
    /// <returns>The collection name.</returns>
    protected string GetCollectionName(Type entityType, CollectionScope collectionScope)
    {
        if (GetModelOptions() == null)
        {
            throw new Exception("Cannot find any model builder settings.");
        }

        return collectionScope switch
        {
            CollectionScope.Query => GetModelOptions().GetQueryCollectionName(entityType),
            CollectionScope.Command => GetModelOptions().GetCollectionName(entityType),
            _ => throw new ArgumentOutOfRangeException(nameof(collectionScope), collectionScope, null)
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Lock?.CurrentCount == 0)
        {
            Lock.Release(1);
        }

        Lock?.Dispose();
    }

    /// <inheritdoc />
    public override Expression Visit(Expression node)
    {
        return Visit(node, null);
    }

    /// <summary>
    /// Returns the visitor result of the subtree visitor.
    /// </summary>
    /// <param name="enumerable">The <see cref="IArangoDbEnumerable"/> sequence as part of the </param>
    /// <param name="collectionScope"></param>
    /// <returns></returns>
    // ReSharper disable once UnusedMemberInSuper.Global
    public abstract SubTreeVisitorResult GetResultExpression(
        IArangoDbEnumerable enumerable,
        CollectionScope collectionScope);
}
