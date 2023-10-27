using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class ArangoDbEnumerable : IArangoDbEnumerable
{
    internal MemberExpression DistinctionKey { get; set; }
    internal string LastRequestId { get; }

    internal int? Limit { get; set; }
    internal ModelBuilderOptions ModelSettings { get; }
    internal int? Offset { get; set; }
    internal List<Expression> OrderExpressions { get; set; } = new List<Expression>();
    internal List<Expression> SelectExpressions { get; set; } = new List<Expression>();
    internal List<string> TypeFilter { get; set; }
    internal List<ExpressionDetails> WhereExpressions { get; set; } = new List<ExpressionDetails>();
    protected Dictionary<string, object> ExpressionsSettings { get; set; } = new Dictionary<string, object>();
    protected virtual ArangoDbFilterTreeVisitor FilterVisitor => new ArangoDbFilterTreeVisitor();

    protected virtual ArangoDbOrderByTreeVisitor OrderByVisitor => new ArangoDbOrderByTreeVisitor();
    protected virtual ArangoDbSelectionTreeVisitor SelectionVisitor => new ArangoDbSelectionTreeVisitor();
    public HashSet<Type> ActivatedConversions { get; } = new HashSet<Type>();

    protected ArangoDbEnumerable(ModelBuilderOptions modelSettings, string lastRequestId)
    {
        LastRequestId = lastRequestId;
        ModelSettings = modelSettings;
    }

    public ArangoDbEnumerable(IArangoDbEnumerable current, string newId) : this(
        current.GetEnumerable()
            .ModelSettings)
    {
        ArangoDbEnumerable e = current.GetEnumerable();

        WhereExpressions = e.WhereExpressions;
        SelectExpressions = e.SelectExpressions;
        OrderExpressions = e.OrderExpressions;
        ExpressionsSettings = e.ExpressionsSettings;
        LastRequestId = newId;
        Limit = e.Limit;
        Offset = e.Offset;
        ActivatedConversions = e.ActivatedConversions;
        TypeFilter = e.TypeFilter;
        DistinctionKey = e.DistinctionKey;
    }

    public ArangoDbEnumerable(IArangoDbEnumerable current) : this(
        current,
        current.GetEnumerable().LastRequestId
        ?? throw new ArgumentException("Enumerable object does not have an id!", nameof(current)))
    {
    }

    public ArangoDbEnumerable(ModelBuilderOptions modelSettings)
    {
        ModelSettings = modelSettings;
        LastRequestId = Guid.NewGuid().ToString();
    }

    protected void AddExpressionsSettings(object settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (!ExpressionsSettings.ContainsKey(LastRequestId))
        {
            ExpressionsSettings.Add(LastRequestId, settings);

            return;
        }

        if (ExpressionsSettings[LastRequestId] != null
            && ExpressionsSettings[LastRequestId].GetType() != settings.GetType())
        {
            throw new ArgumentException(
                "Types of already stored settings object and input arguments are not the same.",
                nameof(settings));
        }

        ExpressionsSettings[LastRequestId] = settings;
    }

    protected virtual string GetQueryOfSortingTree<TEntity>(
        CollectionScope collectionScope,
        SubTreeVisitorResult filter)
    {
        using ArangoDbOrderByTreeVisitor orderByVisitor = OrderByVisitor;

        string sorting = OrderExpressions is
        {
            Count: > 0
        }
            ? orderByVisitor.GetResultExpression(
                    this,
                    collectionScope,
                    filter.CollectionToIterationVarMapping)
                .ReturnString
            : string.Empty;

        return sorting;
    }

    protected virtual SubTreeVisitorResult GetQueryOfSelectionTree<TEntity>(
        CollectionScope collectionScope,
        SubTreeVisitorResult filter)
    {
        using ArangoDbSelectionTreeVisitor selectionVisitor = SelectionVisitor;

        SubTreeVisitorResult selection = selectionVisitor.GetResultExpression(
            this,
            collectionScope,
            filter.CollectionToIterationVarMapping);

        return selection;
    }

    protected virtual SubTreeVisitorResult GetQueryOfFilterTree<TEntity>(CollectionScope collectionScope)
    {
        using ArangoDbFilterTreeVisitor filterVisitor = FilterVisitor;

        SubTreeVisitorResult filter = filterVisitor.GetResultExpression(this, collectionScope);

        if (SelectExpressions == null || SelectExpressions.Count == 0)
        {
            Expression<Func<TEntity, TEntity>> simple = o => o;

            SelectExpressions = new List<Expression>
            {
                simple
            };
        }

        return filter;
    }

    protected virtual string GetSelectionQueryString(
        CollectionScope collectionScope,
        SubTreeVisitorResult selection)
    {
        return string.Join(
            " ",
            selection.CollectionToIterationVarMapping
                .Select(o => $"FOR {o.Value} IN {o.Key}"));
    }

    public ArangoDbEnumerable GetEnumerable()
    {
        return this;
    }

    public virtual Type GetInnerType()
    {
        return typeof(object);
    }

    public bool TryGetExpressionsSettings<TSetting>(ExpressionDetails details, out TSetting settings)
    {
        settings = default;

        if (details == null)
        {
            Debug.WriteLine(
                $"{nameof(TryGetExpressionsSettings)}<{typeof(TSetting).Name}>(): Details should not be null.");

            return false;
        }

        if (!ExpressionsSettings.TryGetValue(details.ExpressionId, out object o))
        {
            Debug.WriteLine(
                $"{nameof(TryGetExpressionsSettings)}<{typeof(TSetting).Name}>(): The expression id '{details.ExpressionId}' has no settings registered!");

            return false;
        }

        if (o is not TSetting converted)
        {
            Debug.WriteLine(
                $"{nameof(TryGetExpressionsSettings)}<{typeof(TSetting).Name}>(): Expression '{details.ExpressionId}' has a registered settings object of type '{o.GetType().Name}', but it cannot be cast to '{typeof(TSetting).Name}'.");

            return false;
        }

        settings = converted;

        return true;
    }

    /// <inheritdoc />
    public virtual IArangoDbQueryResult Compile<TEntity>(CollectionScope collectionScope)
    {
        SubTreeVisitorResult filter = GetQueryOfFilterTree<TEntity>(collectionScope);
        SubTreeVisitorResult selection = GetQueryOfSelectionTree<TEntity>(collectionScope, filter);

        string returnValue = selection.ReturnString;

        var limits = string.Empty;

        if (Offset != null || Limit != null)
        {
            int offset = Offset >= 0
                ? Offset ?? 0
                : 0;

            int limit = Limit > 0
                ? Limit ?? 100
                : 100;

            limits = $"LIMIT {offset},{limit}";
        }

        string sorting = GetQueryOfSortingTree<TEntity>(collectionScope, filter);

        if (DistinctionKey != null)
        {
            return new CollectedArangoDbQueryResult(
                selection.CollectionToIterationVarMapping.Keys.Select(c => c).Distinct().ToList(),
                GetSelectionQueryString(collectionScope, selection),
                filter.ReturnString,
                returnValue,
                limits,
                sorting,
                selection.CollectionToIterationVarMapping.First().Value,
                DistinctionKey.Member.Name);
        }

        return new ArangoDbQueryResult(
            filter.ReturnString,
            GetSelectionQueryString(collectionScope, selection),
            sorting,
            limits,
            returnValue,
            selection.CollectionToIterationVarMapping.Keys.Select(c => c).Distinct().ToList());
    }
}

internal class ArangoDbEnumerable<TEntity> : ArangoDbEnumerable, IArangoDbEnumerable<TEntity>
{
    protected ArangoDbEnumerable(ModelBuilderOptions modelSettings, string lastRequestId) : base(
        modelSettings,
        lastRequestId)
    {
    }

    public ArangoDbEnumerable(string newId, IArangoDbEnumerable current, object settings) : base(current, newId)
    {
        AddExpressionsSettings(settings);
    }

    public ArangoDbEnumerable(IArangoDbEnumerable current, object settings) : base(current)
    {
        AddExpressionsSettings(settings);
    }

    public ArangoDbEnumerable(string newId, IArangoDbEnumerable current) : base(current, newId)
    {
    }

    public ArangoDbEnumerable(IArangoDbEnumerable current) : base(current)
    {
    }

    public ArangoDbEnumerable(ModelBuilderOptions modelSettings) : base(modelSettings)
    {
    }

    public override Type GetInnerType()
    {
        return typeof(TEntity);
    }

    public ArangoDbEnumerable<TEntity> GetTypedEnumerable()
    {
        return this;
    }

    /// <inheritdoc />
    public IArangoDbQueryResult Compile(CollectionScope scope)
    {
        return Compile<TEntity>(scope);
    }
}
