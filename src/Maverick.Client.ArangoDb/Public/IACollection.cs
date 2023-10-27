using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Collection;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     An interface for interacting with ArangoDB Collections endpoints.
/// </summary>
public interface IACollection
{
    /// <summary>
    ///     Create Collection in the current database context.
    /// </summary>
    /// <param name="collectionName">The name of the collection that will be created.</param>
    /// <returns>
    ///     Object containing some attributes of the created collection or possibly occurred errors
    ///     <see cref="CreateCollectionResponse" />.
    /// </returns>
    Task<CreateCollectionResponse> CreateAsync(string collectionName);

    /// <summary>
    ///     Create Collection in the current database context.
    /// </summary>
    /// <param name="collectionName">Name of the collection that will be created.</param>
    /// <param name="collectionType">Collection type <see cref="ACollectionType" />.</param>
    /// <param name="collectionOptions">
    ///     Parameters that can be set when creating a collection
    ///     <see cref="CreateCollectionOptions" />.
    /// </param>
    /// <returns>
    ///     Object containing some attributes of the created collection or possibly occurred errors
    ///     <see cref="CreateCollectionResponse" />.
    /// </returns>
    Task<CreateCollectionResponse> CreateCollectionAsync(
        string collectionName,
        ACollectionType collectionType = ACollectionType.Document,
        CreateCollectionOptions collectionOptions = null);

    /// <summary>
    ///     Create new collection in current database.
    /// </summary>
    /// <param name="collectionBody">
    ///     Object containing attributes to create a collection <see cref="CreateCollectionBody" />
    /// </param>
    /// <returns>
    ///     Object containing some attributes of the created collection or possibly occurred errors
    ///     <see cref="CreateCollectionResponse" />.
    /// </returns>
    Task<CreateCollectionResponse> CreateCollectionAsync(CreateCollectionBody collectionBody);

    /// <summary>
    ///     Retrieves basic information about specified collection
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <returns>
    ///     Object containing some attributes of the specified collection or possibly occurred errors
    ///     <see cref="GetCollectionResponse" />.
    /// </returns>
    Task<GetCollectionResponse> GetCollectionAsync(string collectionName, bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information about specified collection.
    /// </summary>
    /// <typeparam name="TResponse">
    ///     Defines the type of the response object.<br />
    ///     <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    ///     If <typeparamref name="TResponse" /> is "<c>string</c>" type, the raw json will be returned.
    /// </typeparam>
    Task<SingleApiResponse<TResponse>> GetCollectionAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information with additional properties about specified collection.
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    ///     If <typeparamref name="TResponse" /> is "<c>string</c>" type, the raw json will be returned.
    /// </typeparam>
    Task<SingleApiResponse<TResponse>> GetCollectionPropertiesAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information with additional properties about specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <returns>
    ///     Object containing detailed about the specified collection or possibly occurred errors
    ///     <see cref="GetCollectionPropertiesResponse" />.
    /// </returns>
    Task<GetCollectionPropertiesResponse> GetCollectionPropertiesAsync(
        string collectionName,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information with additional properties and document count in specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <returns>
    ///     Object containing detailed about the specified collection or possibly occurred errors
    ///     <see cref="GetCollectionCountResponse" />.
    /// </returns>
    Task<GetCollectionCountResponse> GetCollectionCountAsync(
        string collectionName,
        string transactionId = null,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information with additional properties and document count in specified collection.
    /// </summary>
    /// <typeparam name="TResponse">
    ///     If <typeparamref name="TResponse" /> is "<c>string</c>" type, the raw json will be returned.
    /// </typeparam>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    Task<SingleApiResponse<CollectionCountEntity>> GetCollectionCountAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information with additional properties, document count and figures in specified collection.
    /// </summary>
    /// <typeparam name="TResponse">
    ///     If <typeparamref name="TResponse" /> is "<c>string</c>" type, the raw json will be returned.
    /// </typeparam>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    Task<SingleApiResponse<TResponse>> GetCollectionFiguresAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information with additional properties, document count and figures in specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <returns>
    ///     Contains object with statistical information about a collection or possibly occurred errors
    ///     <see cref="GetFiguresResponse" />
    /// </returns>
    Task<GetFiguresResponse> GetCollectionFiguresAsync(string collectionName, bool allowDirytRead = false);

    /// <summary>
    ///     Retrieves the revision ID of a specified collection
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <returns>
    ///     Object containing the revision id of a given collection or possibly occurred errors
    ///     <see cref="GetRevisionIdResponse" />.
    /// </returns>
    Task<GetRevisionIdResponse> GetRevisionIdAsync(string collectionName, bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information and revision ID of specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <result>
    ///     Object containing basic information and revision ID of specified collection or possibly occurred errors
    ///     <see cref="GetRevisionResponse" />.
    /// </result>
    Task<GetRevisionResponse> GetRevisionAsync(string collectionName, bool forceDirtyRead = false);

    /// <summary>
    ///     Retrieves basic information, revision ID and checksum of specified collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="withRevisions">Whether or not to include document revision ids in the checksum calculation.</param>
    /// <param name="withData">Whether or not to include document body data in the checksum calculation.</param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <returns>
    ///     Object with basic collection information, the revision ID and a checksum of specified collection
    ///     <see cref="GetCheckSumResponse" />.
    /// </returns>
    Task<GetCheckSumResponse> GetChecksumAsync(
        string collectionName,
        bool withRevisions = true,
        bool withData = false,
        bool forceDirtyRead = false);

    /// <summary>
    ///     Get all indexes of the specified collection
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="forceDirtyRead">Allow to send request to a follower (only available in the active-failover setup)</param>
    /// <returns>
    ///     Object containing all indexes for the given collection, some debug information, (possibly) occurred errors
    ///     <see cref="GetAllIndexesResponse" />.
    /// </returns>
    Task<GetAllIndexesResponse> GetAllCollectionIndexesAsync(string collectionName, bool forceDirtyRead = false);

    /// <summary>
    ///     Removes all documents from specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="transactionId">
    ///     Transaction id (only meaningful if the operation is being executed inside a stream
    ///     transaction).
    /// </param>
    /// <returns>
    ///     Object containing some information about the truncated collection, some debug information, or possibly
    ///     occurred errors <see cref="TruncateCollectionResponse" />.
    /// </returns>
    Task<TruncateCollectionResponse> TruncateCollectionAsync(string collectionName, string transactionId = null);

    /// <summary>
    ///     Loads specified collection into memory.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="count">If true, the number of documents inside the collection will be returned.</param>
    /// <returns>
    ///     Object containing some information about the loaded collection, some debug information, or possibly occurred
    ///     errors <see cref="LoadCollectionResponse" />.
    /// </returns>
    Task<LoadCollectionResponse> LoadCollectionAsync(string collectionName, bool count = true);

    /// <summary>
    ///     Unloads specified collection from memory.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <returns>
    ///     Object containing some information about the unloaded collection, some debug information, or possibly occurred
    ///     errors <see cref="UnloadCollectionResponse" />.
    /// </returns>
    Task<UnloadCollectionResponse> UnloadCollectionAsync(string collectionName);

    /// <summary>
    ///     Change properties of a specified collection
    /// </summary>
    /// <param name="collectionProperty">
    ///     Object containing some collection properties that can be modified after the creation
    ///     of the collection <see cref="CollectionPropertyEntity" />.
    /// </param>
    /// <returns>
    ///     Object containing some information about the modified collection, some debug information, or possibly occurred
    ///     errors <see cref="ChangePropertiesResponse" />.
    /// </returns>
    Task<ChangePropertiesResponse> ChangeCollectionPropertiesAsync(CollectionPropertyEntity collectionProperty);

    /// <summary>
    ///     Change properties of the given collection
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="waitForSync">
    ///     If true then creating or changing a
    ///     document will wait until the data has been synchronized to disk.
    /// </param>
    /// <param name="journalSize">
    ///     The maximal size of a journal or datafile in bytes.
    ///     The value must be at least 1048576 (1 MB)
    /// </param>
    /// <returns>
    ///     Object containing some information about the modified collection, some debug information, or possibly occurred
    ///     errors <see cref="ChangePropertiesResponse" />.
    /// </returns>
    Task<ChangePropertiesResponse> ChangeCollectionPropertiesAsync(
        string collectionName,
        bool? waitForSync = null,
        long? journalSize = null);

    /// <summary>
    ///     Renames specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="newCollectionName">The new name of the collection.</param>
    /// <returns>
    ///     Object containing some information about the renamed collection, some debug information, or possibly occurred
    ///     errors <see cref="RenameCollectionResponse" />.
    /// </returns>
    Task<RenameCollectionResponse> RenameCollectionAsync(string collectionName, string newCollectionName);

    /// <summary>
    ///     Rotates the journal of specified collection to make the data in the file available for compaction. Current journal
    ///     of the collection will be closed and turned into read-only datafile. This operation is not available in cluster
    ///     environment.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <returns>
    ///     Object containing  boolean value which takes the value true if the journal of the given collection has been
    ///     successful rotated, some debug information, or possibly occurred erro <see cref="RotateJournalResponse" />.
    /// </returns>
    Task<RotateJournalResponse> RotateCollectionJournalAsync(string collectionName);

    /// <summary>
    ///     Deletes specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <returns>
    ///     Object containing the id of the deleted collection, some debug information, or possibly occurred errors
    ///     <see cref="DeleteCollectionResponse" />.
    /// </returns>
    Task<DeleteCollectionResponse> DeleteCollectionAsync(string collectionName);
}
