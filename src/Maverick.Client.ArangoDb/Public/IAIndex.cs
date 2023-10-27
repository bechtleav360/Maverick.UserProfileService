using System;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Index;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     An interface for interacting with ArangoDB Indexes endpoints.
/// </summary>
public interface IAIndex
{
    /// <summary>
    ///     Create fulltext index
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="fields">An array of attribute names. Currently, the array is limited to exactly one attribute.</param>
    /// <param name="minLength">
    ///     Minimum character length of words to index. Will default
    ///     to a server-defined value if unspecified.It is thus recommended to set this value explicitly when creating the
    ///     index.
    /// </param>
    /// <returns>
    ///     Object containing some attributes of the new created index or possibly occurred errors
    ///     <see cref="CreateFullTextIndexResponse" />.
    /// </returns>
    Task<CreateFullTextIndexResponse> CreateFullTextIndexAsync(
        string collectionName,
        string[] fields,
        int? minLength);

    /// <summary>
    ///     Create hash index
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="fields">An array of attribute paths.</param>
    /// <param name="unique"> If true, then create a unique index.</param>
    /// <param name="sparse">If true, then create a sparse index.</param>
    /// <param name="deduplicate">If false, the duplication of array values is turned off.</param>
    /// <returns>
    ///     Object containing some attributes of the new created index or possibly occurred errors
    ///     <see cref="CreateHashIndexResponse" />.
    /// </returns>
    Task<CreateHashIndexResponse> CreateHashIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse,
        bool deduplicate);

    /// <summary>
    ///     Create a persistent index
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="fields">An array of attribute paths.</param>
    /// <param name="unique">If true, then create a unique index.</param>
    /// <param name="sparse">If true, then create a sparse index.</param>
    /// <returns>
    ///     Object containing some attributes of the new created index or possibly occurred errors
    ///     <see cref="CreatePersistentIndexResponse" />.
    /// </returns>
    Task<CreatePersistentIndexResponse> CreatePersistentIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse);

    /// <summary>
    ///     Create a skip list index
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="fields">An array of attribute paths.</param>
    /// <param name="unique"> If true, then create a unique index.</param>
    /// <param name="sparse"> If true, then create a sparse index.</param>
    /// <param name="deduplicate"> If false, the duplication of array values is turned off.</param>
    /// <returns>
    ///     Object containing some attributes of the new created index or possibly occurred errors
    ///     <see cref="CreateSkipListIndexResponse" />.
    /// </returns>
    Task<CreateSkipListIndexResponse> CreateSkipListIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse,
        bool deduplicate);

    /// <summary>
    ///     Create a TTL index
    /// </summary>
    /// <param name="collectionName">The name of the collection</param>
    /// <param name="fields"> An array with exactly one attribute path.</param>
    /// <param name="expireAfter">
    ///     The time (in seconds) after a document's creation after which the documents count as
    ///     "expired".
    /// </param>
    /// <returns>
    ///     Object containing some attributes of the new created index or possibly occurred errors
    ///     <see cref="CreateTtlIndexResponse" />.
    /// </returns>
    Task<CreateTtlIndexResponse> CreateTtlIndexAsync(string collectionName, string[] fields, int expireAfter);

    /// <summary>
    ///     Create geo-spatial index
    /// </summary>
    /// <param name="collectionName">The name of the collection</param>
    /// <param name="fields">An array with one or two attribute paths.</param>
    /// <param name="geoJson">
    ///     If a geo-spatial index on a location is constructed
    ///     and geoJson is true, then the order within the array is longitude
    ///     followed by latitude.This corresponds to the format described in
    ///     http://geojson.org/geojson-spec.html#positions
    /// </param>
    /// <returns>
    ///     Object containing some attributes of the new created index or possibly occurred errors
    ///     <see cref="CreateGeoIndexResponse" />.
    /// </returns>
    Task<CreateGeoIndexResponse> CreateGeoIndexAsync(string collectionName, string[] fields, bool geoJson);

    /// <summary>
    ///     Deletes specified index.
    /// </summary>
    /// <param name="id">Index Id.</param>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    /// <returns>
    ///     Object containing the index-handle of the deleted index or possibly occurred errors
    ///     <see cref="DeleteIndexResponse" />
    /// </returns>
    Task<DeleteIndexResponse> DeleteIndexAsync(string id);

    /// <summary>
    ///     Retrieves specified index.
    /// </summary>
    /// <param name="id">Index Id.</param>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    /// <returns>Object containing an index entity or possibly occurred errors <see cref="GetIndexResponse" /></returns>
    Task<GetIndexResponse> GetIndexAsync(string id);
}
