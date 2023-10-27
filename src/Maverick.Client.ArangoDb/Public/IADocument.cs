using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Document;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     An interface for interacting with ArangoDB Documents endpoints.
/// </summary>
public interface IADocument
{
    /// <summary>
    ///     Creates new document within specified collection in current database context.
    /// </summary>
    /// <param name="collectionName">The name of the collection where the document should be created.</param>
    /// <param name="document">Document which should be created in the specified collection.</param>
    /// <param name="options">Parameters that can be set when creating a document<see cref="CreateDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <returns>
    ///     Object containing information about the created document or possibly occurred errors
    ///     <see cref="CreateDocumentResponse" />
    /// </returns>
    Task<CreateDocumentResponse> CreateDocumentAsync(
        string collectionName,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Creates new document within specified collection in current database context.
    /// </summary>
    /// <typeparam name="T">Specified document type</typeparam>
    /// <param name="collectionName">The name of the collection where the document should be created.</param>
    /// <param name="documentObject">Documents which have to be added in the specified collection.</param>
    /// <param name="options">Parameters that can be set when creating a document<see cref="CreateDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <returns>
    ///     Object containing information about the created document or possibly occurred errors
    ///     <see cref="CreateDocumentResponse" />
    /// </returns>
    Task<CreateDocumentResponse> CreateDocumentAsync<T>(
        string collectionName,
        T documentObject,
        CreateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Create more documents in the specified collection
    /// </summary>
    /// <typeparam name="T">Specified document type</typeparam>
    /// <param name="collectionName">Name of the collection where the documents should be created</param>
    /// <param name="documents">Documents which have to be added in the specified collection</param>
    /// <param name="options">Parameters that can be set when creating a document <see cref="CreateDocumentOptions" /></param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <returns>
    ///     Object containing information about the created documents or possibly occurred errors
    ///     <see cref="CreateDocumentsResponse" />
    /// </returns>
    Task<CreateDocumentsResponse> CreateDocumentsAsync<T>(
        string collectionName,
        IList<T> documents,
        CreateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Creates new edge document with document data in current database context.
    /// </summary>
    /// <param name="collectionName">Name of the collection where the (edge) document should be created.</param>
    /// <param name="document">Document which has to be added in the specified collection</param>
    /// <param name="options">Parameters that can be set when creating a document <see cref="CreateDocumentOptions" /></param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <exception cref="ArgumentException">Specified document does not contain '_from' and '_to' fields.</exception>
    /// <returns>
    ///     Object containing information about the created document or possibly occurred errors
    ///     <see cref="CreateDocumentResponse" />
    /// </returns>
    Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Creates new edge document within specified collection between two document vertices in current database context.
    /// </summary>
    /// <param name="collectionName">Name of the collection where the (edge) document should be created.</param>
    /// <param name="fromId">The id of the start document.</param>
    /// <param name="toId">The id of the target document.</param>
    /// <param name="options">Parameters that can be set when creating a document <see cref="CreateDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <exception cref="ArgumentException">Specified 'from' and 'to' ID values have invalid format.</exception>
    /// <returns>
    ///     Object containing information about the created document or possibly occurred errors
    ///     <see cref="CreateDocumentResponse" />
    /// </returns>
    Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        string fromId,
        string toId,
        CreateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Creates new edge with document data within specified collection between two document vertices in current database
    ///     context.
    /// </summary>
    /// <param name="collectionName">Name of the collection where the (edge) document should be created.</param>
    /// <param name="fromId">Id of the start document.</param>
    /// <param name="toId">Id of the target document.</param>
    /// <param name="options">Parameters that can be set when creating a document <see cref="CreateDocumentOptions" />.</param>
    /// <param name="document">Document data</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <exception cref="ArgumentException">Specified 'from' and 'to' ID values have invalid format.</exception>
    /// <returns>
    ///     Object containing information about the created document or possibly occurred errors
    ///     <see cref="CreateDocumentResponse" />
    /// </returns>
    Task<CreateDocumentResponse> CreateEdgeAsync(
        string collectionName,
        string fromId,
        string toId,
        Dictionary<string, object> document,
        CreateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Creates new edge with document data within specified collection between two document vertices in current database
    ///     context.
    /// </summary>
    /// <param name="collectionName">Name of the collection where the (edge) document should be created.</param>
    /// <param name="fromId">Id of the start document.</param>
    /// <param name="toId">Id of the target document.</param>
    /// <param name="obj"></param>
    /// <param name="options">Parameters that can be set when creating a document <see cref="CreateDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <exception cref="ArgumentException">Specified 'from' and 'to' ID values have invalid format.</exception>
    /// <returns>
    ///     Object containing information about the created document or possibly occurred errors
    ///     <see cref="CreateDocumentResponse" />
    /// </returns>
    Task<CreateDocumentResponse> CreateEdgeAsync<T>(
        string collectionName,
        string fromId,
        string toId,
        T obj,
        CreateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Checks for existence of specified document.
    /// </summary>
    /// <param name="id">Document id.</param>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    /// <returns>
    ///     Object containing the revision id of the checked document (when founded) or possibly occurred errors
    ///     <see cref="CheckDocResponse" />
    /// </returns>
    Task<CheckDocResponse> CheckDocumentAsync(string id);

    /// <summary>
    ///     Retrieves specified document.
    /// </summary>
    /// <exception cref="ArgumentException"> Specified 'id' value has invalid format. </exception>
    /// <param name="id"> The document Id </param>
    /// <param name="transactionId"> The id of the transaction (only if the operation us being executed inside a transaction). </param>
    /// <param name="forceDirtyRead"> True if the operation should be execute on a follower (only available in failover). </param>
    /// <returns>
    ///     >Object containing the document data or possibly occurred errors <see cref="GetDocumentResponse{T}" />
    /// </returns>
    Task<GetDocumentResponse<T>> GetDocumentAsync<T>(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves specified document.
    /// </summary>
    /// <param name="id"> The document Id </param>
    /// <param name="transactionId"> The id of the transaction (only if the operation us being executed inside a transaction). </param>
    /// <param name="forceDirtyRead"> True if the operation should be execute on a follower (only available in failover). </param>
    /// <exception cref="ArgumentException"> Specified 'id' value has invalid format. </exception>
    /// <returns>
    ///     >Object containing the document data or possibly occurred errors <see cref="GetDocumentResponse" />
    /// </returns>
    Task<GetDocumentResponse> GetDocumentAsync(string id, string transactionId = null, bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves specified document as dictionary.
    /// </summary>
    /// <param name="id">Document id.</param>
    /// <param name="transactionId"> The id of the transaction (only if the operation us being executed inside a transaction). </param>
    /// <param name="forceDirtyRead"> True if the operation should be execute on a follower (only available in failover). </param>
    /// <returns>
    ///     Object containing the document data or possibly occurred errors <see cref="GetDocumentResponse{T}" />
    /// </returns>
    Task<GetDocumentResponse<Dictionary<string, object>>> GetDocumentAsDictionaryAsync(
        string id,
        string transactionId = null,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves list of edges from specified edge type collection to specified document vertex with given direction.
    /// </summary>
    /// <param name="collectionName">Name of the edge collection, where the edge are stored in.</param>
    /// <param name="startVertexId">ID of the start vertex.</param>
    /// <param name="direction">
    ///     Edges direction <see cref="ADirection"/>.</param>
    /// <exception cref="ArgumentException">Specified 'startVertexID' value has invalid format.</exception>
    /// <param name="transactionId"> The id of the transaction (only if the operation us being executed inside a transaction). </param>
    /// <returns>Object containing a list of edges or possibly occurred errors <see cref="GetEdgesResponse"/>.</returns>
    Task<GetEdgesResponse> GetEdgesAsync(
        string collectionName,
        string startVertexId,
        ADirection direction,
        string transactionId = null,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Updates existing document identified by its handle with new document data.
    /// </summary>
    /// <typeparam name="T">Specified document type.</typeparam>
    /// <param name="documentId">The Id of the document.</param>
    /// <param name="document">Document that has to be updated.</param>
    /// <param name="options">Parameters that can be set when updating a document <see cref="UpdateDocumentOptions" />.</param>
    /// <param name="transactionId">Transaction id (only meaningful if the operation is being executed inside a transaction).</param>
    /// <returns>
    ///     Object containing information about the updated document or possibly occurred errors
    ///     <see cref="UpdateDocumentResponse{T}" />
    /// </returns>
    Task<UpdateDocumentResponse<T>> UpdateDocumentAsync<T>(
        string documentId,
        T document,
        UpdateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Updates existing document identified by its key and the name of corresponding collection with new document data.
    /// </summary>
    /// <typeparam name="T">Specified document type.</typeparam>
    /// <param name="collectionName">Collection name</param>
    /// <param name="documentKey">Document ID.</param>
    /// <param name="document">Document that has to be updated.</param>
    /// <param name="options">Parameters that can be set when updating a document <see cref="UpdateDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <returns>
    ///     Object containing information about the updated document or possibly occurred errors
    ///     <see cref="UpdateDocumentResponse{T}" />
    /// </returns>
    Task<UpdateDocumentResponse<T>> UpdateDocumentAsync<T>(
        string collectionName,
        string documentKey,
        T document,
        UpdateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Partially updates documents, the documents to update are specified by the _key attributes in the body objects.
    /// </summary>
    /// <typeparam name="T">Specified document type.</typeparam>
    /// <param name="collectionName">Collection name where documents are stored.</param>
    /// <param name="documents">List of documents that should be updated.</param>
    /// <param name="options">Parameters that can be set when updating a document <see cref="UpdateDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <returns>
    ///     Object containing information about the updated documents or possibly occurred errors
    ///     <see cref="UpdateDocumentsResponse" />
    /// </returns>
    Task<UpdateDocumentsResponse> UpdateDocumentsAsync<T>(
        string collectionName,
        IList<T> documents,
        UpdateDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Completely replaces existing document identified by its handle with new document data.
    /// </summary>
    /// <param name="documentId">Document ID.</param>
    /// <param name="document">New document.</param>
    /// <param name="options">Parameters that can be set when replacing a document <see cref="ReplaceDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    /// <returns>
    ///     Object containing information about the replaced document or possibly occurred errors
    ///     <see cref="ReplaceDocumentResponse" />
    /// </returns>
    Task<ReplaceDocumentResponse> ReplaceDocumentAsync(
        string documentId,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Completely replaces existing document identified by its handle with new document data.
    /// </summary>
    /// <param name="documentId">Document ID.</param>
    /// <param name="document">New document.</param>
    /// <param name="options">Parameters that can be set when replacing a document <see cref="ReplaceDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    /// <returns>
    ///     Object containing information about the replaced document or possibly occurred errors
    ///     <see cref="ReplaceDocumentResponse" />
    /// </returns>
    Task<ReplaceDocumentResponse> ReplaceDocumentAsync<T>(
        string documentId,
        T document,
        ReplaceDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Completely replaces existing edge identified by its handle with new edge data.
    /// </summary>
    /// <param name="id">(Edge) document ID.</param>
    /// <param name="document">New Edge document</param>
    /// <param name="options">Parameters that can be set when replacing a document <see cref="ReplaceDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <exception cref="ArgumentException">Specified document does not contain '_from' and '_to' fields.</exception>
    /// <returns>
    ///     Object containing information about the replaced (edge) document or possibly occurred errors
    ///     <see cref="ReplaceDocumentResponse" />.
    /// </returns>
    Task<ReplaceDocumentResponse> ReplaceEdgeAsync(
        string id,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Completely replaces existing edge identified by its handle with new edge data. This helper method injects 'fromID'
    ///     and 'toID' fields into given document to construct valid edge document.
    /// </summary>
    /// <param name="id">Edge Id</param>
    /// <param name="fromId">Start vertex ID</param>
    /// <param name="toId">Target vertex ID</param>
    /// <param name="document">New document.</param>
    /// <param name="options">Parameters that can be set when replacing a document <see cref="ReplaceDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <exception cref="ArgumentException">Specified 'from' or 'to' ID values have invalid format.</exception>
    /// <returns>
    ///     Object containing information about the replaced (edge) document or possibly occurred errors
    ///     <see cref="ReplaceDocumentResponse" />.
    /// </returns>
    Task<ReplaceDocumentResponse> ReplaceEdgeAsync(
        string id,
        string fromId,
        string toId,
        Dictionary<string, object> document,
        ReplaceDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Completely replaces existing edge identified by its handle with new edge data. This helper method injects 'fromID'
    ///     and 'toID' fields into given document to construct valid edge document.
    /// </summary>
    /// <typeparam name="T">Specified document type.</typeparam>
    /// <param name="id">Edge ID.</param>
    /// <param name="fromId">Start vertex ID.</param>
    /// <param name="toId">Target vertex ID.</param>
    /// <param name="document">New document.</param>
    /// <param name="options">Parameters that can be set when replacing a document <see cref="ReplaceDocumentOptions" />.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <returns>
    ///     Object containing information about the replaced (edge) document or possibly occurred errors
    ///     <see cref="ReplaceDocumentResponse" />.
    /// </returns>
    Task<ReplaceDocumentResponse> ReplaceEdgeAsync<T>(
        string id,
        string fromId,
        string toId,
        T document,
        ReplaceDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Delete the specified document.
    /// </summary>
    /// <exception cref="ArgumentException">Specified 'id' value has invalid format.</exception>
    /// <param name="documentId">The id of the document that should be deleted</param>
    /// <param name="options">Parameters that can be set to delete a document</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)>
    /// </param>
    /// <returns>
    ///     Object containing information about the deleted document or possibly occurred errors
    ///     <see cref="DeleteDocumentResponse" />
    /// </returns>
    Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string documentId,
        DeleteDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Delete the document in the specified collection with the specified document Id.
    /// </summary>
    /// <param name="collectionName">The name of the document that should be deleted</param>
    /// <param name="documentId">The id of the document that should be deleted</param>
    /// <param name="options">Parameters that can be set to delete a document</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <returns>
    ///     Object containing information about the deleted document or possibly occurred errors
    ///     <see cref="DeleteDocumentResponse" />
    /// </returns>
    Task<DeleteDocumentResponse> DeleteDocumentAsync(
        string collectionName,
        string documentId,
        DeleteDocumentOptions options = null,
        string transactionId = null);

    /// <summary>
    ///     Removes multiple documents
    /// </summary>
    /// <param name="collectionName">The name of the collection containing the documents that should be deleted.</param>
    /// <param name="selectors">
    ///     An array consisting of selectors for
    ///     documents.A selector can either be a string with a key or a string
    ///     with a document handle or an object with a _key attribute.
    /// </param>
    /// <param name="options">
    ///     Parameters that can be set by deleting multiple documents <see cref="DeleteDocumentOptions" />
    /// </param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction)
    /// </param>
    /// <returns>
    ///     Information about the deleted documents or possibly occurred errors <see cref="DeleteDocumentsResponse" />
    /// </returns>
    Task<DeleteDocumentsResponse> DeleteDocumentsAsync(
        string collectionName,
        IList<string> selectors,
        DeleteDocumentOptions options = null,
        string transactionId = null);
}
