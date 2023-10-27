using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Interface providing access to ArangoDB endpoints.
/// </summary>
public interface IArangoDbClient : IADatabase,
    IADocument,
    IAQuery,
    IATransaction,
    IACollection,
    IAIndex,
    IAdministration,
    IAFunction
{
    /// <summary>
    ///     Name of the arango client instance.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Serializer settings <see cref="UsedJsonSerializerSettings" /> that are used from the <see cref="IArangoDbClient" />
    /// </summary>
    JsonSerializerSettings UsedJsonSerializerSettings { get; }
}
