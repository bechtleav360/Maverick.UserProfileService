using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.PerformanceLogging.Abstractions;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Models.Administration;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Maverick.Client.ArangoDb.Public.Models.Database;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Function;
using Maverick.Client.ArangoDb.Public.Models.Index;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.Client.ArangoDb.Public.Models.Transaction;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Provides access to ArangoDb endpoints.
/// </summary>
public class ArangoDbClient : IArangoDbClient, IDisposable
{
    /// <summary>
    ///     The Central object for interaction with ArangoDB endpoints
    /// </summary>
    private readonly ADatabase _database;

    public string Name { get; }

    public JsonSerializerSettings UsedJsonSerializerSettings => _database.DefaultSerializerSettings;

    /// <summary>
    ///     Initialize the connection to the database with the given connection string.
    /// </summary>
    /// <param name="name">The name of this <see cref="ArangoDbClient" /> instance.</param>
    /// <param name="connectionString">
    ///     Connection string that contains information to establish the connection to an ArangoDB
    ///     server.
    /// </param>
    /// <param name="clientFactory">
    ///     Client factory that can be used to generate httpClient <see cref="IHttpClientFactory" />
    /// </param>
    /// <param name="exceptionOptions">Options, how to handle exceptions within the client.</param>
    /// <param name="defaultSerializerSettings">Default serializer settings as optional parameter.</param>
    /// <param name="performanceLogSettings">
    ///     Performance log setting to be used. If <c>null</c>, no performance log will be
    ///     written.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="name" /> is an empty string or contains only whitespaces.</exception>
    public ArangoDbClient(
        string name,
        string connectionString,
        IHttpClientFactory clientFactory = null,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        IPerformanceLogSettings performanceLogSettings = null)
    {
        Name = name ?? AConstants.ArangoClientName;

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException($"Parameter {nameof(name)} cannot be empty or whitespace.", nameof(name));
        }

        _database = new ADatabase(
            connectionString,
            clientFactory,
            exceptionOptions,
            defaultSerializerSettings: defaultSerializerSettings,
            clientName: Name,
            performanceLogSettings: performanceLogSettings);
    }

    /// <summary>
    ///     Initialize the connection to the database with the given connection string.
    /// </summary>
    /// <param name="name">The name of this <see cref="ArangoDbClient" /> instance.</param>
    /// <param name="connectionString">
    ///     Connection string that contains information to establish the connection to an ArangoDB
    ///     server.
    /// </param>
    /// <param name="logger">The logger instance that will take log messages.</param>
    /// <param name="clientFactory">
    ///     Client factory that can be used to generate httpClient <see cref="IHttpClientFactory" />
    /// </param>
    /// <param name="exceptionOptions">Options, how to handle exceptions within the client.</param>
    /// <param name="defaultSerializerSettings">Default serializer settings as optional parameter.</param>
    /// <param name="performanceLogSettings">
    ///     Performance log setting to be used. If <c>null</c>, no performance log will be
    ///     written.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="name" /> is an empty string or contains only whitespaces.</exception>
    public ArangoDbClient(
        string name,
        string connectionString,
        ILogger logger,
        IHttpClientFactory clientFactory = null,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        IPerformanceLogSettings performanceLogSettings = null)
    {
        Name = name ?? AConstants.ArangoClientName;

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException($"Parameter {nameof(name)} cannot be empty or whitespace.", nameof(name));
        }

        _database = new ADatabase(
            connectionString,
            clientFactory,
            exceptionOptions,
            logger,
            defaultSerializerSettings,
            Name,
            performanceLogSettings);
    }

    /// <inheritdoc />
    public async Task<GetServerVersionResponse> GetServerVersionAsync(bool details = false)
    {
        return await _database.Administration.GetServerVersionAsync(details);
    }

    /// <summary>
    ///     dispose the database object  <see cref="ADatabase" />
    /// </summary>
    public void Dispose()
    {
        _database?.Dispose();
    }

    /// <inheritdoc />
    public async Task<CreateDbResponse> CreateDatabaseAsync(
        string databaseName,
        DatabaseInfoEntityOptions creationOptions = null)
    {
        return await _database.CreateDatabaseAsync(databaseName, creationOptions);
    }

    /// <inheritdoc />
    public async Task<CreateDbResponse> CreateDatabaseAsync(
        string databaseName,
        IList<AUser> users,
        DatabaseInfoEntityOptions creationOptions = null)
    {
        return await _database.CreateDatabaseAsync(databaseName, users, creationOptions);
    }

    /// <inheritdoc />
    public async Task<GetCurrentDatabaseResponse> GetCurrentDatabaseInfoAsync()
    {
        return await _database.GetCurrentDatabaseInfoAsync();
    }

    /// <inheritdoc />
    public async Task<GetDatabasesResponse> GetAccessibleDatabasesAsync()
    {
        return await _database.GetAccessibleDatabasesAsync();
    }

    /// <inheritdoc />
    public async Task<GetDatabasesResponse> GetAllDatabasesAsync()
    {
        return await _database.GetAllDatabasesAsync();
    }

    /// <inheritdoc />
    public async Task<DropDbResponse> DropDatabaseAsync(string databaseName)
    {
        return await _database.DropDatabaseAsync(databaseName);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateDocumentAsync(
        string collectionName,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.CreateDocumentAsync(collectionName, document, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateDocumentAsync<T>(
        string collectionName,
        T documentObject,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.CreateDocumentAsync(collectionName, documentObject, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentsResponse> CreateDocumentsAsync<T>(
        string collectionName,
        IList<T> documents,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.CreateDocumentsAsync(collectionName, documents, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.CreateEdgeAsync(collectionName, document, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        string fromId,
        string toId,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.CreateEdgeAsync(collectionName, fromId, toId, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        string fromId,
        string toId,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.CreateEdgeAsync(
            collectionName,
            fromId,
            toId,
            document,
            options,
            transactionId);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateEdgeAsync<T>(
        string collectionName,
        string fromId,
        string toId,
        T obj,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.CreateEdgeAsync(
            collectionName,
            fromId,
            toId,
            obj,
            options,
            transactionId);
    }

    /// <inheritdoc />
    public async Task<CheckDocResponse> CheckDocumentAsync(string id)
    {
        return await _database.Document.CheckDocumentAsync(id);
    }

    /// <inheritdoc />
    public async Task<GetDocumentResponse<T>> GetDocumentAsync<T>(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        return await _database.Document.GetDocumentAsync<T>(id, transactionId, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetDocumentResponse> GetDocumentAsync(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        return await _database.Document.GetDocumentAsync(id, transactionId, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetDocumentResponse<Dictionary<string, object>>> GetDocumentAsDictionaryAsync(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        return await _database.Document.GetDocumentAsDictionaryAsync(id, transactionId, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetEdgesResponse> GetEdgesAsync(
        string collectionName,
        string startVertexId,
        ADirection direction,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        return await _database.Document.GetEdgesAsync(
            collectionName,
            startVertexId,
            direction,
            transactionId,
            forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<UpdateDocumentResponse<T>> UpdateDocumentAsync<T>(
        string documentId,
        T document,
        UpdateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.UpdateDocumentAsync(documentId, document, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<UpdateDocumentResponse<T>> UpdateDocumentAsync<T>(
        string collectionName,
        string documentKey,
        T document,
        UpdateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.UpdateDocumentAsync(
            collectionName,
            documentKey,
            document,
            options,
            transactionId);
    }

    /// <inheritdoc />
    public async Task<UpdateDocumentsResponse> UpdateDocumentsAsync<T>(
        string collectionName,
        IList<T> documents,
        UpdateDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.UpdateDocumentsAsync(collectionName, documents, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<ReplaceDocumentResponse> ReplaceDocumentAsync(
        string documentId,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.ReplaceDocumentAsync(documentId, document, options, transactionId);
    }

    /// <inheritdoc />
    public Task<ReplaceDocumentResponse> ReplaceDocumentAsync<T>(
        string documentId,
        T obj,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        return _database.Document.ReplaceDocumentAsync(documentId, obj, options, transactionId);
    }

    /// <inheritdoc />
    public Task<ReplaceDocumentResponse> ReplaceEdgeAsync(
        string id,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        return _database.Document.ReplaceEdgeAsync(id, document, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<ReplaceDocumentResponse> ReplaceEdgeAsync(
        string id,
        string fromId,
        string toId,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.ReplaceEdgeAsync(
            id,
            fromId,
            toId,
            document,
            options,
            transactionId);
    }

    public async Task<ReplaceDocumentResponse> ReplaceEdgeAsync<T>(
        string id,
        string fromId,
        string toId,
        T obj,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.ReplaceEdgeAsync(
            id,
            fromId,
            toId,
            obj,
            options,
            transactionId);
    }

    /// <inheritdoc />
    public async Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string documentId,
        DeleteDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.DeleteDocumentAsync(documentId, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string collectionName,
        string documentId,
        DeleteDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.DeleteDocumentAsync(collectionName, documentId, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<DeleteDocumentsResponse> DeleteDocumentsAsync(
        string collectionName,
        IList<string> selectors,
        DeleteDocumentOptions options = null,
        string transactionId = null)
    {
        return await _database.Document.DeleteDocumentsAsync(collectionName, selectors, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<CursorResponse<T>> CreateCursorAsync<T>(
        CreateCursorBody body,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await _database.Query.CreateCursorAsync<T>(body, transactionId, timeout, cancellationToken);
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
        return await _database.Query.CreateCursorAsync<T>(
            query,
            bindVars,
            options,
            transactionId,
            timeout,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PutCursorResponse<T>> PutCursorAsync<T>(
        string cursorId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await _database.Query.PutCursorAsync<T>(cursorId, timeout, cancellationToken);
    }

    public async Task<ParseQueryResponse> ParseAsync(string query)
    {
        return await _database.Query.ParseAsync(query);
    }

    /// <inheritdoc />
    public async Task<DeleteCursorResponse> DeleteCursorAsync(
        string cursorId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await _database.Query.DeleteCursorAsync(cursorId, timeout, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MultiApiResponse<T>> ExecuteQueryWithCursorOptionsAsync<T>(
        CreateCursorBody cursorBody,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await _database.Query.ExecuteQueryWithCursorOptionsAsync<T>(
            cursorBody,
            transactionId,
            timeout,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MultiApiResponse<T>> ExecuteQueryAsync<T>(
        string query,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await _database.Query.ExecuteQueryAsync<T>(query, transactionId, timeout, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetAllRunningTransactionsResponse> GetAllRunningTransactionsAsync()
    {
        return await _database.Transaction.GetAllRunningTransactionsAsync();
    }

    /// <inheritdoc />
    public async Task<GetTransactionStatusResponse> GetTransactionStatusAsync(string transactionId)
    {
        return await _database.Transaction.GetTransactionStatusAsync(transactionId);
    }

    /// <inheritdoc />
    public async Task<TransactionOperationResponse> BeginTransactionAsync(
        IEnumerable<string> writeCollections,
        IEnumerable<string> readCollections,
        TransactionOptions options = null)
    {
        return await _database.Transaction.BeginTransactionAsync(writeCollections, readCollections, options);
    }

    /// <inheritdoc />
    public async Task<TransactionOperationResponse> CommitTransactionAsync(string transactionId)
    {
        return await _database.Transaction.CommitTransactionAsync(transactionId);
    }

    /// <inheritdoc />
    public async Task<TransactionOperationResponse> AbortTransactionAsync(string transactionId)
    {
        return await _database.Transaction.AbortTransactionAsync(transactionId);
    }

    /// <inheritdoc />
    public async Task<ExecuteJsTransactionResponse<T>> ExecuteJsTransactionAsync<T>(string action)
    {
        return await _database.Transaction.ExecuteJsTransactionAsync<T>(action);
    }

    /// <inheritdoc />
    public async Task<ExecuteJsTransactionResponse<T>> ExecuteJsTransactionAsync<T>(
        string action,
        IEnumerable<string> writeCollections,
        IEnumerable<string> readCollections,
        JsTransactionOptions options = null)
    {
        return await _database.Transaction.ExecuteJsTransactionAsync<T>(
            action,
            writeCollections,
            readCollections,
            options);
    }

    /// <inheritdoc />
    public async Task<CreateCollectionResponse> CreateAsync(string collectionName)
    {
        return await _database.Collection.CreateAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<CreateCollectionResponse> CreateCollectionAsync(
        string collectionName,
        ACollectionType collectionType = ACollectionType.Document,
        CreateCollectionOptions collectionOptions = null)
    {
        return await _database.Collection.CreateCollectionAsync(
            collectionName,
            collectionType,
            collectionOptions);
    }

    /// <inheritdoc />
    public async Task<CreateCollectionResponse> CreateCollectionAsync(CreateCollectionBody collectionBody)
    {
        return await _database.Collection.CreateCollectionAsync(collectionBody);
    }

    /// <inheritdoc />
    public async Task<GetCollectionResponse> GetCollectionAsync(string collectionName, bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionAsync(collectionName, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<TResponse>> GetCollectionAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionAsync<TResponse>(collectionName, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<TResponse>> GetCollectionPropertiesAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionPropertiesAsync<TResponse>(collectionName, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetCollectionPropertiesResponse> GetCollectionPropertiesAsync(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionPropertiesAsync(collectionName, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetAllCollectionsResponse> GetAllCollectionsAsync()
    {
        return await _database.GetAllCollectionsAsync();
    }

    /// <inheritdoc />
    public async Task<GetAllCollectionsResponse> GetAllCollectionsAsync(bool excludeSystem)
    {
        return await _database.GetAllCollectionsAsync(excludeSystem);
    }

    /// <inheritdoc />
    public async Task<GetCollectionCountResponse> GetCollectionCountAsync(
        string collectionName,
        string transactionId,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionCountAsync(collectionName, transactionId, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<CollectionCountEntity>> GetCollectionCountAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionCountAsync<TResponse>(collectionName);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<TResponse>> GetCollectionFiguresAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionFiguresAsync<TResponse>(collectionName, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetFiguresResponse> GetCollectionFiguresAsync(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetCollectionFiguresAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<GetRevisionIdResponse> GetRevisionIdAsync(string collectionName, bool forceDirtyRead = false)
    {
        return await _database.Collection.GetRevisionIdAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<GetRevisionResponse> GetRevisionAsync(string collectionName, bool forceDirtyRead = false)
    {
        return await _database.Collection.GetRevisionAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<GetCheckSumResponse> GetChecksumAsync(
        string collectionName,
        bool withRevisions = true,
        bool withData = false,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetChecksumAsync(collectionName, withRevisions, withData, forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetAllIndexesResponse> GetAllCollectionIndexesAsync(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await _database.Collection.GetAllCollectionIndexesAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<TruncateCollectionResponse> TruncateCollectionAsync(
        string collectionName,
        string transactionId = null)
    {
        return await _database.Collection.TruncateCollectionAsync(collectionName, transactionId);
    }

    /// <inheritdoc />
    public async Task<LoadCollectionResponse> LoadCollectionAsync(string collectionName, bool count = true)
    {
        return await _database.Collection.LoadCollectionAsync(collectionName, count);
    }

    /// <inheritdoc />
    public async Task<UnloadCollectionResponse> UnloadCollectionAsync(string collectionName)
    {
        return await _database.Collection.UnloadCollectionAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<ChangePropertiesResponse> ChangeCollectionPropertiesAsync(
        CollectionPropertyEntity collectionProperty)
    {
        return await _database.Collection.ChangeCollectionPropertiesAsync(collectionProperty);
    }

    /// <inheritdoc />
    public async Task<ChangePropertiesResponse> ChangeCollectionPropertiesAsync(
        string collectionName,
        bool? waitForSync = null,
        long? journalSize = null)
    {
        return await _database.Collection.ChangeCollectionPropertiesAsync(collectionName, waitForSync, journalSize);
    }

    /// <inheritdoc />
    public async Task<RenameCollectionResponse> RenameCollectionAsync(
        string collectionName,
        string newCollectionName)
    {
        return await _database.Collection.RenameCollectionAsync(collectionName, newCollectionName);
    }

    /// <inheritdoc />
    public async Task<RotateJournalResponse> RotateCollectionJournalAsync(string collectionName)
    {
        return await _database.Collection.RotateCollectionJournalAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<DeleteCollectionResponse> DeleteCollectionAsync(string collectionName)
    {
        return await _database.Collection.DeleteCollectionAsync(collectionName);
    }

    /// <inheritdoc />
    public async Task<CreateFullTextIndexResponse> CreateFullTextIndexAsync(
        string collectionName,
        string[] fields,
        int? minLength)
    {
        return await _database.Index.CreateFullTextIndexAsync(collectionName, fields, minLength);
    }

    /// <inheritdoc />
    public async Task<CreateHashIndexResponse> CreateHashIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse,
        bool deduplicate)
    {
        return await _database.Index.CreateHashIndexAsync(collectionName, fields, unique, sparse, deduplicate);
    }

    /// <inheritdoc />
    public async Task<CreatePersistentIndexResponse> CreatePersistentIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse)
    {
        return await _database.Index.CreatePersistentIndexAsync(collectionName, fields, unique, sparse);
    }

    /// <inheritdoc />
    public async Task<CreateSkipListIndexResponse> CreateSkipListIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse,
        bool deduplicate)
    {
        return await _database.Index.CreateSkipListIndexAsync(collectionName, fields, unique, sparse, deduplicate);
    }

    /// <inheritdoc />
    public async Task<CreateTtlIndexResponse> CreateTtlIndexAsync(
        string collectionName,
        string[] fields,
        int expireAfter)
    {
        return await _database.Index.CreateTtlIndexAsync(collectionName, fields, expireAfter);
    }

    /// <inheritdoc />
    public async Task<DeleteIndexResponse> DeleteIndexAsync(string id)
    {
        return await _database.Index.DeleteIndexAsync(id);
    }

    /// <inheritdoc />
    public async Task<GetIndexResponse> GetIndexAsync(string id)
    {
        return await _database.Index.GetIndexAsync(id);
    }

    /// <inheritdoc />
    public async Task<CreateGeoIndexResponse> CreateGeoIndexAsync(
        string collectionName,
        string[] fields,
        bool geoJson)
    {
        return await _database.Index.CreateGeoIndexAsync(collectionName, fields, geoJson);
    }

    /// <inheritdoc />
    public async Task<AqlFuncResponse> RegisterAqlFuncAsync(string name, string code, bool isDeterministic = true)
    {
        return await _database.Function.RegisterAqlFuncAsync(name, code, isDeterministic);
    }

    /// <inheritdoc />
    public async Task<GetAQlFunctionsResponse> ListAqlFuncAsync(string givenNamespace = null)
    {
        return await _database.Function.ListAqlFuncAsync(givenNamespace);
    }

    /// <inheritdoc />
    public async Task<AqlFuncResponse> UnregisterFuncAsync(string name, bool group = false)
    {
        return await _database.Function.UnregisterFuncAsync(name, group);
    }
}
