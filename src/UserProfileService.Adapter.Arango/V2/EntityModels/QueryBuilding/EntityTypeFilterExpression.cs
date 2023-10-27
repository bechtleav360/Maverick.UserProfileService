using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class EntityTypeFilterExpression : Expression
{
    public override ExpressionType NodeType => ExpressionType.Default;
    public override Type Type { get; } = typeof(EntityTypeFilterExpression);
}
