using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Query;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Provides access to ArangoDB Cursor API and allows queries execution.
/// </summary>
/// <inheritdoc />
public class AQuery : IAQuery
{
    private readonly Dictionary<string, object> _bindVars = new Dictionary<string, object>();
    private readonly Connection _connection;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
    private readonly StringBuilder _query = new StringBuilder();

    internal AQuery(Connection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<PutCursorResponse<T>> PutCursorAsync<T>(
        string cursorId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (cursorId == null)
        {
            throw new ArgumentNullException(nameof(cursorId));
        }

        if (cursorId.Length == 0)
        {
            throw new ArgumentException($"{nameof(cursorId)} should not be empty", nameof(cursorId));
        }

        var request = new Request(HttpMethod.Put, ApiBaseUri.Cursor, $"/{cursorId}");
        request.BodyAsString = string.Empty;
        Response response = await RequestHandler.ExecuteAsync(_connection, request, cancellationToken: cancellationToken);
        _parameters.Clear();
        _bindVars.Clear();
        _query.Clear();

        if (response.IsSuccessStatusCode)
        {
            (PutCursorResponseEntity<T> result, JsonDeserializationException parserException) =
                response.ParseBodyIncludingErrors<PutCursorResponseEntity<T>>();

            return parserException == null
                ? new PutCursorResponse<T>(response, result)
                : new PutCursorResponse<T>(response, parserException);
        }

        return new PutCursorResponse<T>(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<DeleteCursorResponse> DeleteCursorAsync(
        string cursorId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var request = new Request(HttpMethod.Delete, ApiBaseUri.Cursor, "/" + cursorId);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        _parameters.Clear();
        _bindVars.Clear();
        _query.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new DeleteCursorResponse(response, true);
        }

        return new DeleteCursorResponse(response, response.Exception);
    }

    /// <summary>
    ///     Transforms specified query into minified version with removed leading and trailing whitespaces except new line
    ///     characters.
    /// </summary>
    public static string Minify(string inputQuery)
    {
        string query = inputQuery.Replace("\r", "");

        var cleanQuery = new StringBuilder();

        var lastAcceptedIndex = 0;
        var startRejecting = true;
        var acceptedLength = 0;

        for (var i = 0; i < query.Length; i++)
        {
            if (startRejecting)
            {
                if (query[i] != '\n' && query[i] != '\t' && query[i] != ' ')
                {
                    lastAcceptedIndex = i;
                    startRejecting = false;
                }
            }

            if (!startRejecting)
            {
                if (query[i] == '\n')
                {
                    cleanQuery.Append(query.Substring(lastAcceptedIndex, acceptedLength + 1));

                    acceptedLength = 0;
                    lastAcceptedIndex = i;
                    startRejecting = true;
                }
                else if (i == query.Length - 1)
                {
                    cleanQuery.Append(query.Substring(lastAcceptedIndex, acceptedLength + 1));
                }
                else
                {
                    acceptedLength++;
                }
            }
        }

        return cleanQuery.ToString();
    }

    /// <summary>
    ///     Sets AQL query code.
    /// </summary>
    public AQuery Aql(string query)
    {
        string cleanQuery = Minify(query);

        if (_query.Length > 0)
        {
            _query.Append(" ");
        }

        _query.Append(cleanQuery);

        return this;
    }

    /// <summary>
    ///     Maps key/value bind parameter to the AQL query.
    /// </summary>
    public AQuery BindVar(string key, object value)
    {
        _bindVars.Object(key, value);

        return this;
    }

    /// <summary>
    ///     Determines whether the number of retrieved documents should be returned in `Extra` property of `AResult` instance.
    ///     Default value: false.
    /// </summary>
    public AQuery Count(bool value)
    {
        _parameters.Bool(ParameterName.Count, value);

        return this;
    }

    /// <summary>
    ///     Determines maximum number of result documents to be transferred from the server to the client in one roundtrip. If
    ///     not set this value is server-controlled.
    /// </summary>
    public AQuery BatchSize(int value)
    {
        _parameters.Int(ParameterName.BatchSize, value);

        return this;
    }

    /// <summary>
    ///     flag to determine whether the AQL query results cache
    ///     shall be used.If set to false, then any query cache lookup will be skipped
    ///     for the query.If set to true, it will lead to the query cache being checked
    ///     for the query if the query cache mode is either on or demand.
    /// </summary>
    public AQuery Cache(bool value)
    {
        _parameters.Bool(ParameterName.Cache, value);

        return this;
    }

    /// <summary>
    ///     the maximum number of memory (measured in bytes) that the query is allowed to
    ///     use. If set, then the query will fail with error “resource limit exceeded” in case it allocates too much
    ///     memory. A value of 0 indicates that there is no memory limit.
    /// </summary>
    public AQuery MemoryLimit(long value)
    {
        _parameters.Long(ParameterName.MemoryLimit, value);

        return this;
    }

    /// <summary>
    ///     The time-to-live for the cursor (in seconds). The cursor will be removed on the server
    ///     automatically after the specified amount of time. This is useful to ensure garbage collection of cursors
    ///     that are not fully fetched by clients. If not set, a server-defined value will be used (default: 30 s).
    /// </summary>
    public AQuery Ttl(int value)
    {
        _parameters.Int(ParameterName.Ttl, value);

        return this;
    }

    /// <inheritdoc />
    public async Task<CursorResponse<T>> CreateCursorAsync<T>(
        CreateCursorBody body,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        if (body.Query == null)
        {
            throw new ArgumentNullException(nameof(body.Query));
        }

        if (body.Query.Length == 0)
        {
            throw new ArgumentException($"{nameof(body.Query)} should not be empty", nameof(body.Query));
        }

        var request = new Request<CreateCursorBody>(HttpMethod.Post, ApiBaseUri.Cursor, body);
        request.TrySetTransactionId(_parameters, transactionId);
        request.TrySetBatchSizeParameter(_parameters);
        request.TrySetCacheParameter(_parameters);
        request.TrySetCountParameter(_parameters);
        request.TrySetMemoryLimitParameter(_parameters);
        request.TrySetTTlParameter(_parameters);

        Response response = await RequestHandler.ExecuteAsync(
            _connection,
            request.NormalizeRequest(),
            timeout: timeout,
            cancellationToken: cancellationToken);

        _parameters.Clear();
        _bindVars.Clear();
        _query.Clear();

        if (response.IsSuccessStatusCode)
        {
            (CreateCursorResponseEntity<T> result, JsonDeserializationException parserException) =
                response.ParseBodyIncludingErrors<CreateCursorResponseEntity<T>>();

            _bindVars.Clear();
            _query.Clear();

            return parserException == null
                ? new CursorResponse<T>(response, result)
                : new CursorResponse<T>(response, parserException);
        }

        return new CursorResponse<T>(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CursorResponse<T>> CreateCursorAsync<T>(
        string query,
        Dictionary<string, object> bindVars = null,
        PostCursorOptions options = null,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var body = new CreateCursorBody
        {
            Query = query,
            BindVars = bindVars ?? _bindVars,
            Options = options,
            Count = _parameters.ContainsKey(ParameterName.Count) ? (bool?)_parameters[ParameterName.Count] : null,
            BatchSize = _parameters.ContainsKey(ParameterName.BatchSize)
                ? (long?)_parameters[ParameterName.BatchSize]
                : null,
            MemoryLimit = _parameters.ContainsKey(ParameterName.MemoryLimit)
                ? (long?)_parameters[ParameterName.MemoryLimit]
                : null,
            Cache = _parameters.ContainsKey(ParameterName.Cache) ? (bool?)_parameters[ParameterName.Cache] : null,
            Ttl = _parameters.ContainsKey(ParameterName.Ttl) ? (int?)_parameters[ParameterName.Ttl] : null
        };

        _parameters.Clear();
        _bindVars.Clear();

        return await CreateCursorAsync<T>(
            body,
            transactionId,
            timeout,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ParseQueryResponse> ParseAsync(string query)
    {
        var bodyDocument = new Dictionary<string, object>();
        bodyDocument.String(ParameterName.Query, Minify(query));

        var request = new Request<Dictionary<string, object>>(HttpMethod.Post, ApiBaseUri.Query, bodyDocument);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();
        _bindVars.Clear();
        _query.Clear();

        if (response.IsSuccessStatusCode)
        {
            var result = response.ParseBody<ParseQueryResponseEntity>();

            return new ParseQueryResponse(response, result);
        }

        return new ParseQueryResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<MultiApiResponse<T>> ExecuteQueryAsync<T>(
        string query,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteQueryWithCursorOptionsAsync<T>(
            new CreateCursorBody
            {
                Query = query
            },
            transactionId,
            timeout,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MultiApiResponse<T>> ExecuteQueryWithCursorOptionsAsync<T>(
        CreateCursorBody cursorBody,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var cursorIterator = new CursorIterator<T>(this, cursorBody, transactionId);

        IEnumerable<T> result = new List<T>();
        IList<BaseApiResponse> apiResponses = new List<BaseApiResponse>();

        while (cursorIterator.HasNext && !cursorIterator.Failed)
        {
            await cursorIterator.NextAsync();

            if (!cursorIterator.Failed && cursorIterator.Value != null)
            {
                result = result.Concat(cursorIterator.Value);
            }

            apiResponses.Add(cursorIterator.CursorResponse);
        }

        return new MultiApiResponse<T>(apiResponses, result);
    }
}
