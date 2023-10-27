using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Document;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains methods that can be run on an ArangoDB document within a transaction.
/// </summary>
public class TransactionDocumentMethods : TransactionMethods
{
    internal TransactionDocumentMethods(IRunningTransaction transaction)
        : base(transaction)
    {
    }

    /// <summary>
    ///     Retrieves specific document.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="id">The identifier of the ArangoDB document.</param>
    /// <param name="modifier">
    ///     The builder object to add additional option to the request. The transaction id must not be
    ///     added.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. It wraps a <see cref="GetDocumentResponse{TDocument}" />
    ///     containing the requested document.
    /// </returns>
    public async Task<GetDocumentResponse<T>> GetDocumentAsync<T>(
        string id,
        Action<ADocument> modifier = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        }

        ADocument doc = new ADocument(Transaction.UsedConnection)
            .SetTransactionId(Transaction.GetTransactionId());

        modifier?.Invoke(doc);

        return await doc.GetDocumentAsync<T>(id).ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves specific document.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="collection">The name of the collection where the document is stored.</param>
    /// <param name="key">The ArangoDB key ot the document.</param>
    /// <param name="modifier">
    ///     The builder object to add additional option to the request. The transaction id must not be
    ///     added.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. It wraps a <see cref="GetDocumentResponse{TDocument}" />
    ///     containing the requested document.
    /// </returns>
    public async Task<GetDocumentResponse<T>> GetDocumentAsync<T>(
        string collection,
        string key,
        Action<ADocument> modifier = null)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(collection));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        }

        ADocument doc = new ADocument(Transaction.UsedConnection).SetTransactionId(Transaction.GetTransactionId());

        modifier?.Invoke(doc);

        return await doc.GetDocumentAsync<T>($"{collection}/{key}").ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves specific document.
    /// </summary>
    /// <param name="id">The identifier of the ArangoDB document.</param>
    /// <param name="modifier">
    ///     The builder object to add additional option to the request. The transaction id must not be
    ///     added.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. It wraps a <see cref="GetDocumentResponse{TDocument}" />
    ///     containing the requested document.
    /// </returns>
    public async Task<GetDocumentResponse> GetDocumentAsync(
        string id,
        Action<ADocument> modifier = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        }

        ADocument doc = new ADocument(Transaction.UsedConnection)
            .SetTransactionId(Transaction.GetTransactionId());

        modifier?.Invoke(doc);

        return await doc.GetDocumentAsync(id).ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves specific document.
    /// </summary>
    /// <param name="collection">The name of the collection where the document is stored.</param>
    /// <param name="key">The ArangoDB key ot the document.</param>
    /// <param name="modifier">
    ///     The builder object to add additional option to the request. The transaction id must not be
    ///     added.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. It wraps a <see cref="GetDocumentResponse{TDocument}" />
    ///     containing the requested document.
    /// </returns>
    public async Task<GetDocumentResponse> GetDocumentAsync(
        string collection,
        string key,
        Action<ADocument> modifier = null)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(collection));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        }

        ADocument doc = new ADocument(Transaction.UsedConnection)
            .SetTransactionId(Transaction.GetTransactionId());

        modifier?.Invoke(doc);

        return await doc.GetDocumentAsync($"{collection}/{key}").ConfigureAwait(false);
    }

    /// <summary>
    ///     Creates new document within specified collection in current database context.
    /// </summary>
    /// <typeparam name="TDocument">Specified document type</typeparam>
    /// <param name="collection">The name of the collection where the document should be created.</param>
    /// <param name="document">Documents which have to be added in the specified collection.</param>
    /// <param name="options">Parameters that can be set when creating a document<see cref="CreateDocumentOptions" />.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. It wraps an object containing information about the created
    ///     document or possibly occurred errors <see cref="CreateDocumentResponse" />
    /// </returns>
    public Task<CreateDocumentResponse> CreateDocumentAsync<TDocument>(
        string collection,
        TDocument document,
        CreateDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).CreateDocumentAsync(
            collection,
            document,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Creates new edge document within specified collection between two document vertices in current database context.
    /// </summary>
    /// <param name="collection">Name of the collection where the (edge) document should be created.</param>
    /// <param name="fromId">The id of the start document.</param>
    /// <param name="toId">The id of the target document.</param>
    /// <param name="body">The object stored as body of the dge</param>
    /// <param name="options">Parameters that can be set when creating a document <see cref="CreateDocumentOptions" />.</param>
    /// <exception cref="ArgumentException">Specified 'from' and 'to' ID values have invalid format.</exception>
    /// <returns>
    ///     A task that represents asynchronous read operation. It wraps an object containing information about the
    ///     created document or possibly occurred errors <see cref="CreateDocumentResponse" />
    /// </returns>
    public Task<CreateDocumentResponse> CreateEdgeAsync<TBody>(
        string collection,
        string fromId,
        string toId,
        TBody body,
        CreateDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).CreateEdgeAsync(
            collection,
            fromId,
            toId,
            body,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Creates new edge document within specified collection between two document vertices in current database context.
    /// </summary>
    /// <param name="collection">Name of the collection where the (edge) document should be created.</param>
    /// <param name="fromId">The id of the start document.</param>
    /// <param name="toId">The id of the target document.</param>
    /// <param name="options">Parameters that can be set when creating a document <see cref="CreateDocumentOptions" />.</param>
    /// <exception cref="ArgumentException">Specified 'from' and 'to' ID values have invalid format.</exception>
    /// <returns>
    ///     A task that represents asynchronous read operation. It wraps an object containing information about the
    ///     created document or possibly occurred errors <see cref="CreateDocumentResponse" />
    /// </returns>
    public Task<CreateDocumentResponse> CreateEdgeAsync(
        string collection,
        string fromId,
        string toId,
        CreateDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).CreateEdgeAsync(
            collection,
            fromId,
            toId,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Checks for existence of specified document.
    /// </summary>
    /// <param name="collection">The name of the collection where the document is stored.</param>
    /// <param name="key">The ArangoDB key ot the document.</param>
    /// <param name="modifier">
    ///     The builder object to add additional option to the request. The transaction id must not be
    ///     added.
    /// </param>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing the revision id of the
    ///     checked document (when founded) or possibly occurred errors <see cref="CheckDocResponse" />
    /// </returns>
    public async Task<CheckDocResponse> CheckDocumentAsync(
        string collection,
        string key,
        Action<ADocument> modifier = null)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(collection));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        }

        ADocument doc = new ADocument(Transaction.UsedConnection)
            .SetTransactionId(Transaction.GetTransactionId());

        modifier?.Invoke(doc);

        return await doc.CheckDocumentAsync($"{collection}/{key}").ConfigureAwait(false);
    }

    /// <summary>
    ///     Checks for existence of specified document.
    /// </summary>
    /// <param name="id">Document id.</param>
    /// <param name="modifier">
    ///     The builder object to add additional option to the request. The transaction id must not be
    ///     added.
    /// </param>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing the revision id of the
    ///     checked document (when founded) or possibly occurred errors <see cref="CheckDocResponse" />
    /// </returns>
    public async Task<CheckDocResponse> CheckDocumentAsync(
        string id,
        Action<ADocument> modifier = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        }

        ADocument doc = new ADocument(Transaction.UsedConnection)
            .SetTransactionId(Transaction.GetTransactionId());

        modifier?.Invoke(doc);

        return await doc.CheckDocumentAsync(id).ConfigureAwait(false);
    }

    /// <summary>
    ///     Delete the specified document.
    /// </summary>
    /// <param name="collection">Name of the collection in which the document is to be deleted.</param>
    /// <param name="key">The key of the document that should be deleted</param>
    /// <param name="options">Parameters that can be set to delete a document</param>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing information about the
    ///     deleted document or possibly occurred errors <see cref="DeleteDocumentResponse" />
    /// </returns>
    public Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string collection,
        string key,
        DeleteDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).DeleteDocumentAsync(
            collection,
            key,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Delete the specified document.
    /// </summary>
    /// <param name="id">The id of the document that should be deleted</param>
    /// <param name="options">Parameters that can be set to delete a document</param>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing information about the
    ///     deleted document or possibly occurred errors <see cref="DeleteDocumentResponse" />
    /// </returns>
    public Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string id,
        DeleteDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).DeleteDocumentAsync(
            id,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Delete the specified document.
    /// </summary>
    /// <param name="collection">Collection from which documents are removed.</param>
    /// <param name="idSelector">A collection of strings containing ids of all documents to be deleted.</param>
    /// <param name="options">Parameters that can be set to delete documents</param>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing information about the
    ///     deleted document or possibly occurred errors <see cref="DeleteDocumentsResponse" />
    /// </returns>
    public Task<DeleteDocumentsResponse> DeleteDocumentsAsync(
        string collection,
        IList<string> idSelector,
        DeleteDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).DeleteDocumentsAsync(
            collection,
            idSelector,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Completely replaces existing document identified by its handle with new document data.
    /// </summary>
    /// <param name="id">The id of the document to be replaced.</param>
    /// <param name="document">The data of the document that will replace the existent one.</param>
    /// <param name="options">Parameters that can be set when replacing a document <see cref="ReplaceDocumentOptions" />.</param>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing information about the
    ///     replaced document or possibly occurred errors <see cref="ReplaceDocumentResponse" />
    /// </returns>
    public Task<ReplaceDocumentResponse> ReplaceDocumentAsync<TDocument>(
        string id,
        TDocument document,
        ReplaceDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).ReplaceDocumentAsync(
            id,
            document,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Completely replaces existing edge identified by its handle with new document data.
    /// </summary>
    /// <param name="collection">The name of the collection where the document is stored.</param>
    /// <param name="key">The ArangoDB key ot the document.</param>
    /// <param name="document">The data of the document that will replace the existent one.</param>
    /// <param name="options">Parameters that can be set when replacing a document <see cref="ReplaceDocumentOptions" />.</param>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing information about the
    ///     replaced document or possibly occurred errors <see cref="ReplaceDocumentResponse" />
    /// </returns>
    public Task<ReplaceDocumentResponse> ReplaceDocumentAsync<TDocument>(
        string collection,
        string key,
        TDocument document,
        ReplaceDocumentOptions options = null)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(collection));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        }

        return new ADocument(Transaction.UsedConnection).ReplaceDocumentAsync(
            $"{collection}/{key}",
            document,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Updates existing document identified by its key and the name of corresponding collection with new document data.
    /// </summary>
    /// <typeparam name="TDocument">Specified document type.</typeparam>
    /// <param name="collection">Collection name</param>
    /// <param name="key">Document ID.</param>
    /// <param name="document">Document that has to be updated.</param>
    /// <param name="options">Parameters that can be set when updating a document <see cref="UpdateDocumentOptions" />.</param>
    /// <returns>
    ///     Object containing information about the updated document or possibly occurred errors
    ///     <see cref="UpdateDocumentResponse{T}" />
    /// </returns>
    public Task<UpdateDocumentResponse<TDocument>> UpdateDocumentAsync<TDocument>(
        string collection,
        string key,
        TDocument document,
        UpdateDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).UpdateDocumentAsync(
            collection,
            key,
            document,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Updates existing document identified by its handle with new document data.
    /// </summary>
    /// <typeparam name="TDocument">Specified document type.</typeparam>
    /// <param name="id">The Id of the document.</param>
    /// <param name="document">Document that has to be updated.</param>
    /// <param name="options">Parameters that can be set when updating a document <see cref="UpdateDocumentOptions" />.</param>
    /// <returns>
    ///     Object containing information about the updated document or possibly occurred errors
    ///     <see cref="UpdateDocumentResponse{TDocument}" />
    /// </returns>
    public Task<UpdateDocumentResponse<TDocument>> UpdateDocumentAsync<TDocument>(
        string id,
        TDocument document,
        UpdateDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).UpdateDocumentAsync(
            id,
            document,
            options,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Partially updates documents, the documents to update are specified by the _key attributes in the body objects.
    /// </summary>
    /// <typeparam name="TDocument">Specified document type.</typeparam>
    /// <param name="collection">Collection name where documents are stored.</param>
    /// <param name="documents">List of documents that should be updated.</param>
    /// <param name="options">Parameters that can be set when updating a document <see cref="UpdateDocumentOptions" />.</param>
    /// <returns>
    ///     Object containing information about the updated documents or possibly occurred errors
    ///     <see cref="UpdateDocumentsResponse" />
    /// </returns>
    public Task<UpdateDocumentsResponse> UpdateDocumentsAsync<TDocument>(
        string collection,
        IList<TDocument> documents,
        UpdateDocumentOptions options = null)
    {
        return new ADocument(Transaction.UsedConnection).UpdateDocumentsAsync(
            collection,
            documents,
            options,
            Transaction.GetTransactionId());
    }
}
