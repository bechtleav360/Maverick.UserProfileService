using System;
using System.Linq.Expressions;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal sealed class OrderByExpression : Expression
{
    internal SortOrder SortingOrder { get; }

    public Expression Body { get; }

    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Constant;

    /// <inheritdoc />
    public override Type Type { get; }

    public OrderByExpression(Expression body, SortOrder sortingOrder)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));
        SortingOrder = sortingOrder;
        Type = body.GetType();
    }
}
