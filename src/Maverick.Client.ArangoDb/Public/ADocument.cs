using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Document;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     A class for interacting with ArangoDB Documents endpoints.
/// </summary>
/// <inheritdoc />
public class ADocument : IADocument
{
    private readonly Connection _connection;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    internal ADocument(Connection connection)
    {
        _connection = connection;
    }

    /// <summary>
    ///     Completely replaces existing document identified by its handle with new document data.
    /// </summary>
    /// <param name="documentId">The id of the document to be replaced.</param>
    /// <param name="json">A JSON representation of a single document that will replace the existent one.</param>
    /// <param name="options">Options to set up the replace operation.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    internal async Task<ReplaceDocumentResponse> ReplaceAsync(
        string documentId,
        string json,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        if (!IsId(documentId))
        {
            throw new ArgumentException("Specified 'id' value (" + documentId + ") has invalid format.");
        }

        string requestUrl = options == null ? "" : "?" + options.ToOptionsString();
        var request = new Request(HttpMethod.Put, ApiBaseUri.Document, "/" + documentId + requestUrl);
        request.BodyAsString = json;

        request.TrySetTransactionId(_parameters, transactionId);

        if (options == null)
        {
            // optional
            request.TrySetQueryStringParameter(ParameterName.WaitForSync, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.IgnoreRevs, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnNew, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnOld, _parameters);
        }

        // optional
        request.TrySetHeaderParameter(ParameterName.IfMatch, _parameters);

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            var res = response.ParseBody<DocumentResponseEntity>();

            return new ReplaceDocumentResponse(response, res);
        }

        return new ReplaceDocumentResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CheckDocResponse> CheckDocumentAsync(string id)
    {
        if (!IsId(id))
        {
            throw new ArgumentException("Specified 'id' value (" + id + ") has invalid format.");
        }

        var request = new Request(HttpMethod.Head, ApiBaseUri.Document, "/" + id);
        // optional
        request.TrySetHeaderParameter(ParameterName.IfMatch, _parameters);
        // optional: If revision is different -> HTTP 200. If revision is identical -> HTTP 304.
        request.TrySetHeaderParameter(ParameterName.IfNoneMatch, _parameters);

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();
        string result = response.ResponseHeaders.ETag.ToString().Replace("\"", "");

        return new CheckDocResponse(response, result);
    }

    /// <inheritdoc />
    public async Task<GetEdgesResponse> GetEdgesAsync(
        string collectionName,
        string startVertexId,
        ADirection direction,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        if (!IsId(startVertexId))
        {
            throw new ArgumentException("Specified 'startVertexID' value (" + startVertexId + ") has invalid format.");
        }

        var request = new Request(HttpMethod.Get, ApiBaseUri.Edges, "/" + collectionName);
        request.TrySetTransactionId(_parameters, transactionId);

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        // required
        request.QueryString.Add(ParameterName.Vertex, startVertexId);
        // required
        request.QueryString.Add(ParameterName.Direction, direction.ToString().ToLower());

        Response response = await RequestHandler.ExecuteAsync(_connection, request, forceDirtyRead);
        _parameters.Clear();

        if (!response.IsSuccessStatusCode)
        {
            return new GetEdgesResponse(response, response.Exception);
        }

        var result = response.ParseBody<EdgesResponseEntity>();

        return new GetEdgesResponse(response, result);
    }

    /// <summary>
    ///     Determines whether or not to wait until data are synchronised to disk. Default value: false.
    /// </summary>
    public ADocument WaitForSync(bool value)
    {
        // needs to be string value
        _parameters.String(ParameterName.WaitForSync, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     If set to true, the insert becomes a replace-insert. If a document with the
    ///     same _key already exists the new document is not rejected with unique
    ///     constraint violated but will replace the old document
    /// </summary>
    public ADocument Overwrite(bool value)
    {
        // needs to be string value
        _parameters.String(ParameterName.Overwrite, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Conditionally operate on document with specified revision.
    /// </summary>
    public ADocument IfMatch(string revision)
    {
        _parameters.String(ParameterName.IfMatch, revision);

        return this;
    }

    /// <summary>
    ///     Conditionally operate on document which current revision does not match specified revision.
    /// </summary>
    public ADocument IfNoneMatch(string revision)
    {
        _parameters.String(ParameterName.IfNoneMatch, revision);

        return this;
    }

    /// <summary>
    ///     Determines whether to '_rev' field in the given document is ignored. If this is set to false, then the '_rev'
    ///     attribute given in the body document is taken as a precondition. The document is only replaced if the current
    ///     revision is the one specified.
    /// </summary>
    public ADocument IgnoreRevs(bool value)
    {
        // needs to be string value
        _parameters.String(ParameterName.IgnoreRevs, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Determines whether to keep any attributes from existing document that are contained in the patch document which
    ///     contains null value. Default value: true.
    /// </summary>
    public ADocument KeepNull(bool value)
    {
        // needs to be string value
        _parameters.String(ParameterName.KeepNull, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Determines whether the value in the patch document will overwrite the existing document's value. Default value:
    ///     true.
    /// </summary>
    public ADocument MergeObjects(bool value)
    {
        // needs to be string value
        _parameters.String(ParameterName.MergeObjects, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Determines whether to return additionally the complete new document under the attribute 'new' in the result.
    /// </summary>
    public ADocument ReturnNew()
    {
        // needs to be string value
        _parameters.String(ParameterName.ReturnNew, true.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Determines whether to return additionally the complete previous revision of the changed document under the
    ///     attribute 'old' in the result.
    /// </summary>
    public ADocument ReturnOld()
    {
        // needs to be string value
        _parameters.String(ParameterName.ReturnOld, true.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Set the transaction id to excute a supported operation inside a stream transaction.
    /// </summary>
    /// <param name="transactionId"></param>
    public ADocument SetTransactionId(string transactionId)
    {
        if (transactionId == null)
        {
            throw new ArgumentNullException(nameof(transactionId));
        }

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new ArgumentException("Value cannot be empty or whitespace.", nameof(transactionId));
        }

        _parameters.String(ParameterName.TransactionId, transactionId);

        return this;
    }

    /// <summary>
    ///     If set to true, an empty object will be returned as response
    /// </summary>
    /// <param name="value"></param>
    public ADocument Silent(bool value)
    {
        // needs to be string value
        _parameters.String(ParameterName.Silent, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Set the mode of an overwriting operation. If set, the overwrite flag will be set to true.
    ///     <seealso cref="Overwrite" />()<br />
    ///     If a document with the same _key already exists the new document is not<br />
    ///     rejected with unique constraint violated but will replace the old document.
    /// </summary>
    /// <remarks>
    ///     Note that operations with overwrite parameter require a _key attribute in the request payload,<br />
    ///     therefore they can only be performed on collections sharded by _key.
    /// </remarks>
    /// <param name="value">The overwrite option to be selected.</param>
    /// <returns>The current instance of <see cref="ADocument" /> with a modified parameter collection.</returns>
    public ADocument OverWriteMode(AOverwriteMode value)
    {
        Overwrite(true);
        // needs to be string value
        _parameters.String(ParameterName.OverWriteMode, value.ToString().ToLower());

        return this;
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateDocumentAsync(
        string collectionName,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        return await CreateDocumentAsync<Dictionary<string, object>>(
                collectionName,
                document,
                options,
                transactionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateDocumentAsync<T>(
        string collectionName,
        T documentObject,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        string requestUrl = options == null ? "" : "?" + options.ToOptionsString();

        var request = new Request<T>(
            HttpMethod.Post,
            ApiBaseUri.Document,
            documentObject,
            $"/{collectionName}" + requestUrl);

        if (options == null)
        {
            // optional
            request.TrySetQueryStringParameter(ParameterName.WaitForSync, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.Overwrite, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.Silent, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnNew, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnOld, _parameters);

            // optional - only available for ArangoDb ver. >= 3.7
            request.TrySetQueryStringParameter(ParameterName.OverWriteMode, _parameters);
        }

        request.TrySetTransactionId(_parameters, transactionId);

        Response response = await RequestHandler.ExecuteAsync(
            _connection,
            request,
            _connection.DefaultSerializerSettings);

        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new CreateDocumentResponse(response, response.ParseBody<DocumentResponseEntity>());
        }

        return new CreateDocumentResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentsResponse> CreateDocumentsAsync<T>(
        string collectionName,
        IList<T> documents,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        string requestUrl = options == null ? "" : "?" + options.ToOptionsString();

        var request = new Request<IList<T>>(
            HttpMethod.Post,
            ApiBaseUri.Document,
            documents,
            $"/{collectionName}" + requestUrl);

        request.TrySetTransactionId(_parameters, transactionId);

        Response response = await RequestHandler.ExecuteAsync(
            _connection,
            request,
            _connection.DefaultSerializerSettings);

        if (response.IsSuccessStatusCode)
        {
            return new CreateDocumentsResponse(response, response.ParseBody<IList<DocumentResponseEntity>>());
        }

        return new CreateDocumentsResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        if (!document.Has("_from") && !document.Has("_to"))
        {
            throw new ArgumentException("Specified document does not contain '_from' and '_to' fields.");
        }

        return await CreateDocumentAsync(collectionName, document, options, transactionId).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        string fromId,
        string toId,
        CreateDocumentOptions options = null,
        string transactionId = null)
    {
        if (!IsId(fromId))
        {
            throw new ArgumentException("Specified 'from' value (" + fromId + ") has invalid format.");
        }

        if (!IsId(toId))
        {
            throw new ArgumentException("Specified 'to' value (" + toId + ") has invalid format.");
        }

        var document = new Dictionary<string, object>
        {
            { "_from", fromId },
            { "_to", toId }
        };

        return await CreateDocumentAsync(collectionName, document, options, transactionId).ConfigureAwait(false);
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
        if (!IsId(fromId))
        {
            throw new ArgumentException("Specified 'from' value (" + toId + ") has invalid format.");
        }

        if (!IsId(toId))
        {
            throw new ArgumentException("Specified 'from' value (" + toId + ") has invalid format.");
        }

        document.From(fromId);
        document.To(toId);

        return await CreateDocumentAsync(collectionName, document, options, transactionId).ConfigureAwait(false);
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
        if (!IsId(fromId))
        {
            throw new ArgumentException("Specified 'from' value (" + toId + ") has invalid format.");
        }

        if (!IsId(toId))
        {
            throw new ArgumentException("Specified 'from' value (" + toId + ") has invalid format.");
        }

        var document = new Dictionary<string, object>();

        try
        {
            document = Dictator.ToDocument(obj);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse : {nameof(obj)}", ex);
        }

        return await CreateEdgeAsync(
                collectionName,
                fromId,
                toId,
                document,
                options,
                transactionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<GetDocumentResponse<T>> GetDocumentAsync<T>(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        if (!IsId(id))
        {
            throw new ArgumentException("Specified 'id' value (" + id + ") has invalid format.");
        }

        var request = new Request(HttpMethod.Get, ApiBaseUri.Document, "/" + id);
        request.TrySetTransactionId(_parameters, transactionId);

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        // optional
        request.TrySetHeaderParameter(ParameterName.IfMatch, _parameters);
        // optional: If revision is different -> HTTP 200. If revision is identical -> HTTP 304.
        request.TrySetHeaderParameter(ParameterName.IfNoneMatch, _parameters);

        Response response = await RequestHandler.ExecuteAsync(_connection, request, forceDirtyRead);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new GetDocumentResponse<T>(response, response.ParseBody<T>());
        }

        return new GetDocumentResponse<T>(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetDocumentResponse> GetDocumentAsync(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        if (!IsId(id))
        {
            throw new ArgumentException("Specified 'id' value (" + id + ") has invalid format.");
        }

        var request = new Request(HttpMethod.Get, ApiBaseUri.Document, "/" + id);
        request.TrySetTransactionId(_parameters, transactionId);

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        // optional
        request.TrySetHeaderParameter(ParameterName.IfMatch, _parameters);
        // optional: If revision is different -> HTTP 200. If revision is identical -> HTTP 304.
        request.TrySetHeaderParameter(ParameterName.IfNoneMatch, _parameters);

        Response response = await RequestHandler.ExecuteAsync(_connection, request, forceDirtyRead);
        _parameters.Clear();

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            var docWithData = response.ParseBody<DocumentWithData>();
            var doc = response.ParseBody<Dictionary<string, object>>();
            doc.Remove("_id");
            doc.Remove("_rev");
            doc.Remove("_key");
            docWithData.DocumentData = doc;

            return new GetDocumentResponse(response, docWithData);
        }

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return new GetDocumentResponse(response);
        }

        return new GetDocumentResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetDocumentResponse<Dictionary<string, object>>> GetDocumentAsDictionaryAsync(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        return await GetDocumentAsync<Dictionary<string, object>>(id, transactionId, forceDirtyRead)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UpdateDocumentResponse<T>> UpdateDocumentAsync<T>(
        string documentId,
        T document,
        UpdateDocumentOptions options = null,
        string transactionId = null)
    {
        if (!IsId(documentId))
        {
            throw new ArgumentException("Specified 'id' value (" + documentId + ") has invalid format.");
        }

        string requestUrl = options == null ? "" : "?" + options.ToOptionsString();

        var request = new Request<T>(
            HttpMethod.Patch,
            ApiBaseUri.Document,
            document,
            "/" + documentId + requestUrl);

        request.TrySetTransactionId(_parameters, transactionId);

        if (options == null)
        {
            // optional
            request.TrySetQueryStringParameter(ParameterName.WaitForSync, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.KeepNull, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.MergeObjects, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.IgnoreRevs, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnNew, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnOld, _parameters);
        }

        // optional
        request.TrySetHeaderParameter(ParameterName.IfMatch, _parameters);

        Response response = await RequestHandler.ExecuteAsync(
            _connection,
            request,
            _connection.DefaultSerializerSettings);

        _parameters.Clear();

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            var res = response.ParseBody<UpdateDocumentEntity<T>>();

            return new UpdateDocumentResponse<T>(response, res);
        }

        return new UpdateDocumentResponse<T>(
            response,
            new ApiErrorException(response.ParseBody<ArangoErrorResponse>()));
    }

    /// <inheritdoc />
    public async Task<UpdateDocumentResponse<T>> UpdateDocumentAsync<T>(
        string collectionName,
        string documentKey,
        T document,
        UpdateDocumentOptions options = null,
        string transactionId = null)
    {
        return await UpdateDocumentAsync(collectionName + "/" + documentKey, document, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<UpdateDocumentsResponse> UpdateDocumentsAsync<T>(
        string collectionName,
        IList<T> documents,
        UpdateDocumentOptions options = null,
        string transactionId = null)
    {
        string requestUrl = options == null ? "" : "?" + options.ToOptionsString();

        var request = new Request<IList<T>>(
            HttpMethod.Patch,
            ApiBaseUri.Document,
            documents,
            "/" + collectionName + requestUrl);

        request.TrySetTransactionId(_parameters, transactionId);

        if (options == null)
        {
            // optional
            request.TrySetQueryStringParameter(ParameterName.WaitForSync, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.KeepNull, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.MergeObjects, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.IgnoreRevs, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnNew, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnOld, _parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(
            _connection,
            request,
            _connection.DefaultSerializerSettings);

        if (response.IsSuccessStatusCode)
        {
            var res = response.ParseBody<IList<DocumentResponseEntity>>();

            return new UpdateDocumentsResponse(response, res);
        }

        return new UpdateDocumentsResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<ReplaceDocumentResponse> ReplaceDocumentAsync(
        string documentId,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        var requestBodyAsString = string.Empty;

        try
        {
            requestBodyAsString = document.SerializeObject(_connection.DefaultSerializerSettings);
        }
        catch (Exception)
        {
            throw new ArgumentException($"Failed to parse: {nameof(document)}");
        }

        return await ReplaceAsync(documentId, requestBodyAsString, options, transactionId).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ReplaceDocumentResponse> ReplaceDocumentAsync<T>(
        string documentId,
        T document,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        var requestBodyAsString = string.Empty;

        try
        {
            requestBodyAsString = document.SerializeObject(_connection.DefaultSerializerSettings);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to parse: {nameof(document)}", ex);
        }

        return await ReplaceAsync(documentId, requestBodyAsString, options, transactionId).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ReplaceDocumentResponse> ReplaceEdgeAsync(
        string id,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        if (!document.Has("_from") && !document.Has("_to"))
        {
            throw new ArgumentException("Specified document does not contain '_from' and '_to' fields.");
        }

        var requestBodyAsString = string.Empty;

        try
        {
            requestBodyAsString = document.SerializeObject(_connection.DefaultSerializerSettings);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to parse: {nameof(document)}", ex);
        }

        return await ReplaceAsync(id, requestBodyAsString, options, transactionId).ConfigureAwait(false);
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
        if (!IsId(fromId))
        {
            throw new ArgumentException("Specified 'from' value (" + fromId + ") has invalid format.");
        }

        if (!IsId(toId))
        {
            throw new ArgumentException("Specified 'to' value (" + toId + ") has invalid format.");
        }

        document.From(fromId);
        document.To(toId);

        var requestBodyAsString = string.Empty;

        try
        {
            requestBodyAsString = document.SerializeObject(_connection.DefaultSerializerSettings);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to parse: {nameof(document)}", ex);
        }

        return await ReplaceAsync(id, requestBodyAsString, options, transactionId).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ReplaceDocumentResponse> ReplaceEdgeAsync<T>(
        string id,
        string fromId,
        string toId,
        T obj,
        ReplaceDocumentOptions options = null,
        string transactionId = null)
    {
        var document = new Dictionary<string, object>();

        try
        {
            document = Dictator.ToDocument(obj);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to parse: {nameof(obj)}", ex);
        }

        return await ReplaceEdgeAsync(
                id,
                fromId,
                toId,
                document,
                options,
                transactionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string documentId,
        DeleteDocumentOptions options = null,
        string transactionId = null)
    {
        if (!IsId(documentId))
        {
            throw new ArgumentException("Specified 'id' value (" + documentId + ") has invalid format.");
        }

        string requestUrl = options == null ? "" : "?" + options.ToOptionsString();
        var request = new Request(HttpMethod.Delete, ApiBaseUri.Document, "/" + documentId + requestUrl);

        if (options == null)
        {
            // optional
            request.TrySetQueryStringParameter(ParameterName.WaitForSync, _parameters);
            // optional
            request.TrySetQueryStringParameter(ParameterName.ReturnOld, _parameters);
            request.TrySetQueryStringParameter(ParameterName.Silent, _parameters);
        }

        request.TrySetTransactionId(_parameters, transactionId);

        // optional
        request.TrySetHeaderParameter(ParameterName.IfMatch, _parameters);

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            var doc = response.ParseBody<DeleteDocumentResponseEntity>();

            return new DeleteDocumentResponse(response, doc);
        }

        return new DeleteDocumentResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string collectionName,
        string documentId,
        DeleteDocumentOptions options = null,
        string transactionId = null)
    {
        return await DeleteDocumentAsync(collectionName + "/" + documentId, options, transactionId);
    }

    /// <inheritdoc />
    public async Task<DeleteDocumentsResponse> DeleteDocumentsAsync(
        string collectionName,
        IList<string> selectors,
        DeleteDocumentOptions options = null,
        string transactionId = null)
    {
        string requestUrl = options == null ? "" : $"?{options.ToOptionsString()}";

        var request = new Request<IList<string>>(
            HttpMethod.Delete,
            ApiBaseUri.Document,
            selectors,
            $"/{collectionName}{requestUrl}");

        if (options == null)
        {
            request.TrySetQueryStringParameter(ParameterName.WaitForSync, _parameters);
            request.TrySetQueryStringParameter(ParameterName.ReturnOld, _parameters);
            request.TrySetQueryStringParameter(ParameterName.IgnoreRevs, _parameters);
        }

        request.TrySetTransactionId(_parameters, transactionId);

        // optional
        if (options?.IfMatch != null)
        {
            _parameters.String(ParameterName.IfMatch, options.IfMatch);
        }

        request.TrySetHeaderParameter(ParameterName.IfMatch, _parameters);

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var docs = response.ParseBody<List<DocumentBase>>();

            return new DeleteDocumentsResponse(response, docs);
        }

        return new DeleteDocumentsResponse(response, response.Exception);
    }

    /// <summary>
    ///     Determines if specified value has valid document `_id` format.
    /// </summary>
    public static bool IsId(string id)
    {
        if (id.Contains("/"))
        {
            string[] split = id.Split('/');

            if (split.Length == 2 && split[0].Length > 0 && split[1].Length > 0)
            {
                return IsKey(split[1]);
            }
        }

        return false;
    }

    /// <summary>
    ///     Determines if specified value has valid document `_key` format.
    /// </summary>
    public static bool IsKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        return AConstants.KeyRegex.IsMatch(key);
    }

    /// <summary>
    ///     Determines if specified value has valid document `_rev` format.
    /// </summary>
    public static bool IsRev(string revision)
    {
        if (string.IsNullOrEmpty(revision))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Constructs document ID from specified collection and key values.
    /// </summary>
    public static string Identify(string collection, long key)
    {
        if (string.IsNullOrEmpty(collection))
        {
            return null;
        }

        return collection + "/" + key;
    }

    /// <summary>
    ///     Constructs document ID from specified collection and key values. If key format is invalid null value is returned.
    /// </summary>
    public static string Identify(string collection, string key)
    {
        if (string.IsNullOrEmpty(collection))
        {
            return null;
        }

        if (IsKey(key))
        {
            return collection + "/" + key;
        }

        return null;
    }

    /// <summary>
    ///     Parses key value out of specified document ID. If ID has invalid value null is returned.
    /// </summary>
    public static string ParseKey(string id)
    {
        if (id.Contains("/"))
        {
            string[] split = id.Split('/');

            if (split.Length == 2 && split[0].Length > 0 && split[1].Length > 0)
            {
                if (IsKey(split[1]))
                {
                    return split[1];
                }
            }
        }

        return null;
    }
}
