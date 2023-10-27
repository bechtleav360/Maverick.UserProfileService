using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Transaction;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Provides access to ArangoDB Transaction API.
/// </summary>
/// <inheritdoc />
public class ATransaction : IATransaction
{
    private readonly Connection _connection;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
    private readonly List<string> _readCollections = new List<string>();
    private readonly Dictionary<string, object> _transactionParams = new Dictionary<string, object>();
    private readonly List<string> _writeCollections = new List<string>();

    internal ATransaction(Connection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<GetTransactionStatusResponse> GetTransactionStatusAsync(string transactionId)
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Transaction, $"/{transactionId}");
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var transactionResponse = response.ParseBody<Body<TransactionEntity>>();

            return new GetTransactionStatusResponse(
                response,
                transactionResponse?.Result?.Status ?? TransactionStatus.Unknown);
        }

        return new GetTransactionStatusResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<ExecuteJsTransactionResponse<T>> ExecuteJsTransactionAsync<T>(string action)
    {
        var bodyDocument = new Dictionary<string, object>();

        // required
        bodyDocument.String(ParameterName.Action, action);

        // required
        if (_readCollections.Count > 0)
        {
            bodyDocument.List(ParameterName.Collections + ".read", _readCollections);
        }

        // required
        if (_writeCollections.Count > 0)
        {
            bodyDocument.List(ParameterName.Collections + ".write", _writeCollections);
        }

        // optional
        Request.TrySetBodyParameter(ParameterName.WaitForSync, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.LockTimeout, _parameters, bodyDocument);

        // optional
        if (_transactionParams.Count > 0)
        {
            bodyDocument.Document(ParameterName.Params, _transactionParams);
        }

        var request = new Request<Dictionary<string, object>>(
            HttpMethod.Post,
            ApiBaseUri.Transaction,
            bodyDocument);

        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        _parameters.Clear();
        _readCollections.Clear();
        _writeCollections.Clear();
        _transactionParams.Clear();

        if (response.IsSuccessStatusCode)
        {
            T transactionResult = response.ParseBody<Body<T>>().Result;

            return new ExecuteJsTransactionResponse<T>(response, transactionResult);
        }

        return new ExecuteJsTransactionResponse<T>(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<ExecuteJsTransactionResponse<T>> ExecuteJsTransactionAsync<T>(
        string action,
        IEnumerable<string> writeCollections,
        IEnumerable<string> readCollections = null,
        JsTransactionOptions options = null)
    {
        if (writeCollections == null)
        {
            throw new ArgumentNullException(nameof(writeCollections));
        }

        if (writeCollections.ToList().Count == 0)
        {
            throw new ArgumentException(
                $"{nameof(writeCollections)} should not be empty",
                nameof(writeCollections));
        }

        var body = new PostJsTransactionBody
        {
            Action = action,
            Collections = new PostTransactionRequestCollections
            {
                Read = readCollections,
                Write = writeCollections
            },
            LockTimeout = options?.LockTimeout,
            MaxTransactionSize = options?.MaxTransactionSize,
            WaitForSync = options?.WaitForSync,
            Params = options?.Params
        };

        var request = new Request<PostJsTransactionBody>(HttpMethod.Post, ApiBaseUri.Transaction, body);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        _parameters.Clear();
        _readCollections.Clear();
        _writeCollections.Clear();
        _transactionParams.Clear();

        if (response.IsSuccessStatusCode)
        {
            T transactionResult = response.ParseBody<Body<T>>().Result;

            return new ExecuteJsTransactionResponse<T>(response, transactionResult);
        }

        return new ExecuteJsTransactionResponse<T>(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<TransactionOperationResponse> BeginTransactionAsync(
        IEnumerable<string> writeCollections,
        IEnumerable<string> readCollections,
        TransactionOptions options = null)
    {
        List<string> write = writeCollections?.ToList();
        List<string> read = readCollections?.ToList() ?? new List<string>();

        if (write == null)
        {
            throw new ArgumentNullException(nameof(writeCollections));
        }

        if (write.Count == 0)
        {
            throw new ArgumentException(
                $"{nameof(writeCollections)} should not be empty",
                nameof(writeCollections));
        }

        var bodyDocument = new Dictionary<string, object>();

        if (read.Count > 0)
        {
            bodyDocument.List(ParameterName.Collections + ".read", read);
        }

        bodyDocument.List(ParameterName.Collections + ".write", write);

        if (options != null)
        {
            if (options.LockTimeout != null)
            {
                LockTimeout(options.LockTimeout.Value);
                Request.TrySetBodyParameter(ParameterName.LockTimeout, _parameters, bodyDocument);
            }

            if (options.MaxTransactionSize != null)
            {
                MaxTransactionSize(options.MaxTransactionSize.Value);
                Request.TrySetBodyParameter(ParameterName.MaxTransactionSize, _parameters, bodyDocument);
            }

            if (options.WaitForSync != null)
            {
                WaitForSync(options.WaitForSync.Value);
                Request.TrySetBodyParameter(ParameterName.WaitForSync, _parameters, bodyDocument);
            }
        }

        var request = new Request<Dictionary<string, object>>(
            HttpMethod.Post,
            ApiBaseUri.Transaction,
            bodyDocument,
            "/begin")
        {
            TransactionInformation =
                $"Read: {string.Join(", ", read)}; write: {string.Join(", ", write)}"
        };

        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        _parameters.Clear();
        _readCollections.Clear();
        _writeCollections.Clear();

        if (response.IsSuccessStatusCode)
        {
            var transactionResponse = response.ParseBody<Body<TransactionEntity>>();
            response.AddTransactionId(transactionResponse?.Result?.Id);

            return new TransactionOperationResponse(response, transactionResponse?.Result, _connection);
        }

        return new TransactionOperationResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<TransactionOperationResponse> CommitTransactionAsync(string transactionId)
    {
        var request = new Request(HttpMethod.Put, ApiBaseUri.Transaction, $"/{transactionId}");
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var transactionResponse = response.ParseBody<Body<TransactionEntity>>();

            return new TransactionOperationResponse(response, transactionResponse?.Result, _connection);
        }

        return new TransactionOperationResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<TransactionOperationResponse> AbortTransactionAsync(string transactionId)
    {
        var request = new Request(HttpMethod.Delete, ApiBaseUri.Transaction, $"/{transactionId}");
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var transactionResponse = response.ParseBody<Body<TransactionEntity>>();

            return new TransactionOperationResponse(response, transactionResponse?.Result, _connection);
        }

        return new TransactionOperationResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetAllRunningTransactionsResponse> GetAllRunningTransactionsAsync()
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Transaction);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var transactionResult = response.ParseBody<Dictionary<string, IList<Transaction>>>();
            IList<Transaction> transactions = transactionResult?["transactions"];

            return new GetAllRunningTransactionsResponse(response, transactions);
        }

        return new GetAllRunningTransactionsResponse(response, response.Exception);
    }

    /// <summary>
    ///     Maps read collection to current transaction.
    /// </summary>
    /// <inheritdoc />
    public ATransaction ReadCollection(string collectionName)
    {
        _readCollections.Add(collectionName);

        return this;
    }

    /// <summary>
    ///     Maps write collection to current transaction.
    /// </summary>
    /// <inheritdoc />
    public ATransaction WriteCollection(string collectionName)
    {
        _writeCollections.Add(collectionName);

        return this;
    }

    /// <summary>
    ///     Determines whether or not to wait until data are synchronised to disk. Default value: false.
    /// </summary>
    /// <inheritdoc />
    public ATransaction WaitForSync(bool value)
    {
        // needs to be string value
        _parameters.String(ParameterName.WaitForSync, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Determines a numeric value that can be used to set a timeout for waiting on collection locks. Setting value to 0
    ///     will make ArangoDB not time out waiting for a lock.
    /// </summary>
    /// <inheritdoc />
    public ATransaction LockTimeout(long value)
    {
        _parameters.Long(ParameterName.LockTimeout, value);

        return this;
    }

    /// <summary>
    ///     Set the maximum transaction size before making intermediate commits (RocksDB only).
    /// </summary>
    /// <param name="value"></param>
    /// <inheritdoc />
    public ATransaction MaxTransactionSize(long value)
    {
        _parameters.Long(ParameterName.MaxTransactionSize, value);

        return this;
    }

    /// <summary>
    ///     Maps key/value parameter to current transaction.
    /// </summary>
    /// <inheritdoc />
    public ATransaction Param(string key, object value)
    {
        _transactionParams.Object(key, value);

        return this;
    }
}
