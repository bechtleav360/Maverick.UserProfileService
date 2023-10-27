// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Different index types provided by ArangoDB.
/// </summary>
public enum AIndexType
{
    /// <summary>
    ///     The fulltext index in ArangoDB can be used to split a text attribute into individual words,
    ///     which will then all be inserted into the index for the document that contains them.
    ///     Later on, one can query documents by individual words or a combination of words.
    /// </summary>
    Fulltext,

    /// <summary>
    ///     The geo index in ArangoDB will store a geo coordinate from a configurable location attribute of documents,
    ///     and can be used to efficiently find all documents that are closest to a certain geo coordinate.
    ///     Additionally it supports querying documents whose location is in a circle around a certain geo coordinate.
    /// </summary>
    Geo,

    /// <summary>
    ///     this type of index can be created on demand by end users.
    ///     A hash index can be created on a single attribute or on multiple attributes at the same time, as needed.
    /// </summary>
    Hash,

    /// <summary>
    ///     In the MMFiles engine, the skiplist index actually uses an in-memory skiplist behind the scenes.
    ///     A skiplist is a sorted data structure, so the skiplist index is a general purpose index type that can be used to
    ///     support a lot of different types of queries (equality lookups, range scans, sorting).
    ///     In the RocksDB engine, the skiplist index shares the same implementation as the hash index, so all notes about the
    ///     RocksDB-based hash index apply for the RocksDB-based skiplist index too.
    ///     Again, the name “skiplist” was kept for compatibility reasons only.
    /// </summary>
    Skiplist,

    /// <summary>
    ///     A persistent index is a sorted index that can be used for finding individual documents or ranges of documents.
    /// </summary>
    Persistent,

    /// <summary>
    ///     A TTL index is a regular index on a collection, and it indexes a date/time point for each document.
    /// </summary>
    Ttl,

    /// <summary>
    ///     A primary index is automatically created for each collections.
    ///     It indexes the documents’ primary keys, which are stored in the _key system attribute.
    ///     The primary index is unique and can be used for queries on both the _key and _id attributes.
    ///     There is no way to explicitly create or delete primary indexes.
    /// </summary>
    Primary,

    /// <summary>
    ///     An edge index is automatically created for edge collections.
    ///     It contains connections between vertex documents and is invoked when the connecting edges of a vertex are queried.
    ///     There is no way to explicitly create or delete edge indexes. The edge index is non-unique.
    /// </summary>
    Edge
}
