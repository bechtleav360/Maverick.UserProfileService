using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

public class SubTreeVisitorResult : Expression
{
    internal Dictionary<string, string> CollectionToIterationVarMapping { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    internal string ReturnString { get; set; }

    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Default;

    /// <inheritdoc />
    public override Type Type { get; } = typeof(SubTreeVisitorResult);

    /// <inheritdoc />
    public override string ToString()
    {
        return ReturnString;
    }
}
