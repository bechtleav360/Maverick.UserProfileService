// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contents the differents status of a collection.
/// </summary>
public enum ACollectionStatus
{
    /// <summary>
    ///     Collection has been newlly created
    /// </summary>
    NewBornCollection = 1,

    /// <summary>
    ///     Collection has been unloaded
    /// </summary>
    Unloaded = 2,

    /// <summary>
    ///     Collection is loaded in memory
    /// </summary>
    Loaded = 3,

    /// <summary>
    ///     Collection in the process of being unloaded
    /// </summary>
    Processing = 4,

    /// <summary>
    ///     Collection has been deleted
    /// </summary>
    Deleted = 5,

    /// <summary>
    ///     Collection is loading
    /// </summary>
    Loading = 6
}
