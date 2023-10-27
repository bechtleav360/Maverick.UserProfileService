using System;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class ArangoNestedQueryEnumerable<TEntity> : ArangoDbEnumerable<TEntity>
{
    private readonly IArangoDbEnumerable _nestedQuery;

    protected override ArangoDbFilterTreeVisitor FilterVisitor =>
        new ArangoDbFilterTreeVisitor
        {
            PredefineCollectionName = "nested"
        };

    protected override ArangoDbOrderByTreeVisitor OrderByVisitor =>
        new ArangoDbOrderByTreeVisitor
        {
            PredefineCollectionName = "nested"
        };

    protected override ArangoDbSelectionTreeVisitor SelectionVisitor =>
        new ArangoDbSelectionTreeVisitor
        {
            PredefineCollectionName = "nested"
        };

    public ArangoNestedQueryEnumerable(
        Func<IArangoDbEnumerable<TEntity>, IArangoDbEnumerable> factory,
        IArangoDbEnumerable nested)
        : base(
            nested.GetEnumerable().ModelSettings,
            nested.GetEnumerable().LastRequestId)
    {
        ArangoDbEnumerable e = factory.Invoke(this) as ArangoDbEnumerable
            ?? throw new Exception("Cannot convert to ArangoDbEnumerable.");

        WhereExpressions = e.WhereExpressions;
        SelectExpressions = e.SelectExpressions;
        OrderExpressions = e.OrderExpressions;
        Limit = e.Limit;
        Offset = e.Offset;

        _nestedQuery = nested;
    }

    protected override string GetSelectionQueryString(
        CollectionScope collectionScope,
        SubTreeVisitorResult selection)
    {
        if (selection?.CollectionToIterationVarMapping?.ContainsKey("nested") != true)
        {
            throw new Exception("Collection-to-variable mapping is missing key 'nested'.");
        }

        string variable = selection.CollectionToIterationVarMapping["nested"];

        return $"FOR {variable} IN FLATTEN({_nestedQuery.Compile<TEntity>(collectionScope).GetQueryString()})";
    }
}
