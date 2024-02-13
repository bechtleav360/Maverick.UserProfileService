using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Default implementation of <see cref="IArangoDbEnumerable"/> used to query ArangoDB for entites.
/// </summary>
public class ArangoDbEnumerable : IArangoDbEnumerable
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

    /// <summary>
    ///     Gets or sets a collection of key-value pairs representing expression settings.
    /// </summary>
    protected Dictionary<string, object> ExpressionsSettings { get; set; } = new Dictionary<string, object>();
    /// <summary>
    ///     Gets a new instance of the <see cref="ArangoDbFilterTreeVisitor"/> class.
    /// </summary>
    protected virtual ArangoDbFilterTreeVisitor FilterVisitor => new ArangoDbFilterTreeVisitor();
    /// <summary>
    ///     Gets a new instance of the <see cref="ArangoDbOrderByTreeVisitor"/> class.
    /// </summary>
    protected virtual ArangoDbOrderByTreeVisitor OrderByVisitor => new ArangoDbOrderByTreeVisitor();
    /// <summary>
    ///     Gets a new instance of the <see cref="ArangoDbSelectionTreeVisitor"/> class.
    /// </summary>
    protected virtual ArangoDbSelectionTreeVisitor SelectionVisitor => new ArangoDbSelectionTreeVisitor();
    /// <summary>
    ///     Gets a set of conversions that were already activated.
    /// </summary>
    public HashSet<Type> ActivatedConversions { get; } = new HashSet<Type>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable"/> class
    ///     with the specified model settings and last request id.
    /// </summary>
    /// <param name="modelSettings">The model builder options.</param>
    /// <param name="lastRequestId">The last request id.</param>
    protected ArangoDbEnumerable(ModelBuilderOptions modelSettings, string lastRequestId)
    {
        LastRequestId = lastRequestId;
        ModelSettings = modelSettings;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable"/> class
    ///     based on the specified <see cref="IArangoDbEnumerable"/> and with the specified id./>
    /// </summary>
    /// <param name="current">The current enumerable.</param>
    /// <param name="newId">The new id.</param>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable"/> class
    ///     based on the specified <see cref="IArangoDbEnumerable"/>.
    /// </summary>
    /// <param name="current"></param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="current"/> does not have an id.</exception>
    public ArangoDbEnumerable(IArangoDbEnumerable current) : this(
        current,
        current.GetEnumerable().LastRequestId
        ?? throw new ArgumentException("Enumerable object does not have an id!", nameof(current)))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable"/> class
    ///     with the specified model builder options.
    /// </summary>
    /// <param name="modelSettings"></param>
    public ArangoDbEnumerable(ModelBuilderOptions modelSettings)
    {
        ModelSettings = modelSettings;
        LastRequestId = Guid.NewGuid().ToString();
    }

    /// <summary>
    ///     Adds or updates the expressions settings for the current <see cref="ArangoDbEnumerable"/>.
    /// </summary>
    /// <param name="settings">The settings object to be added or updated.</param>
    /// <exception cref="ArgumentNullException">Thrown when the input settings object is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the types of already stored settings object and input arguments are not the same.</exception>
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

    /// <summary>
    ///     Retrieves the sorting query from the sorting tree for a given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="collectionScope">The collection scope.</param>
    /// <param name="filter">The result of the filter tree traversal.</param>
    /// <returns>The sorting query as a string.</returns>
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

    /// <summary>
    ///     Retrieves the query of the selection tree for a given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="collectionScope">The collection scope.</param>
    /// <param name="filter">The result of the filter tree traversal.</param>
    /// <returns>The result of the selection tree traversal.</returns>
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

    /// <summary>
    ///     Retrieves the query of the filter tree for a given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="collectionScope">The collection scope.</param>
    /// <returns>The result of the filter tree traversal.</returns>
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

    /// <summary>
    ///     Returns the query string used to perform a selection on a collection with the specified <paramref name="collectionScope"/>
    ///     based on a <see cref="SubTreeVisitorResult"/>.
    /// </summary>
    /// <param name="collectionScope"></param>
    /// <param name="selection"></param>
    /// <returns></returns>
    protected virtual string GetSelectionQueryString(
        CollectionScope collectionScope,
        SubTreeVisitorResult selection)
    {
        return string.Join(
            " ",
            selection.CollectionToIterationVarMapping
                .Select(o => $"FOR {o.Value} IN {o.Key}"));
    }

    /// <inheritdoc />
    public ArangoDbEnumerable GetEnumerable()
    {
        return this;
    }

    /// <inheritdoc />
    public virtual Type GetInnerType()
    {
        return typeof(object);
    }

    /// <summary>
    ///     Tries to retrieve the expressions settings for given expression details.
    /// </summary>
    /// <typeparam name="TSetting">The type of the settings object.</typeparam>
    /// <param name="details">The expression details.</param>
    /// <param name="settings">The retrieved settings object (if successful).</param>
    /// <returns><see langword="true"/> if the settings were successfully retrieved, <see langword="false"/> otherwise.</returns>
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

/// <summary>
///     Represents an enumerable collection for querying data from ArangoDB.
/// </summary>
/// <typeparam name="TEntity">The type of entities in the collection.</typeparam>
public class ArangoDbEnumerable<TEntity> : ArangoDbEnumerable, IArangoDbEnumerable<TEntity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable{TEntity}"/> class.
    /// </summary>
    /// <param name="modelSettings">The model builder options.</param>
    /// <param name="lastRequestId">The last request ID.</param>
    protected ArangoDbEnumerable(ModelBuilderOptions modelSettings, string lastRequestId) : base(
        modelSettings,
        lastRequestId)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable{TEntity}"/> class.
    /// </summary>
    /// <param name="newId">The new ID.</param>
    /// <param name="current">The current enumerable.</param>
    /// <param name="settings">Additional settings.</param>
    public ArangoDbEnumerable(string newId, IArangoDbEnumerable current, object settings) : base(current, newId)
    {
        AddExpressionsSettings(settings);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable{TEntity}"/>
    ///     class based on the specified <see cref="IArangoDbEnumerable"/> and settings object.
    /// </summary>
    /// <param name="current">The current enumerable.</param>
    /// <param name="settings">Additional settings.</param>
    public ArangoDbEnumerable(IArangoDbEnumerable current, object settings) : base(current)
    {
        AddExpressionsSettings(settings);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable{TEntity}"/>
    ///     class based on the specified <see cref="IArangoDbEnumerable"/> and an id.
    /// </summary>
    /// <param name="newId">The new id.</param>
    /// <param name="current">The current enumerable.</param>
    public ArangoDbEnumerable(string newId, IArangoDbEnumerable current) : base(current, newId)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable{TEntity}"/>
    ///     class based on the specified <see cref="IArangoDbEnumerable"/>.
    /// </summary>
    /// <param name="current">The current enumerable.</param>
    public ArangoDbEnumerable(IArangoDbEnumerable current) : base(current)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbEnumerable{TEntity}"/> class
    ///     with the specified <see cref="ModelBuilderOptions"/>.
    /// </summary>
    /// <param name="modelSettings">The model builder options.</param>
    public ArangoDbEnumerable(ModelBuilderOptions modelSettings) : base(modelSettings)
    {
    }

    /// <inheritdoc />
    public override Type GetInnerType()
    {
        return typeof(TEntity);
    }

    /// <inheritdoc />
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
