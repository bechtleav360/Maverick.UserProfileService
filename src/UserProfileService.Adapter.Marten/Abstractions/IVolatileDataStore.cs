using Marten;

namespace UserProfileService.Adapter.Marten.Abstractions;

/// <summary>
///     This is only a marker interface that is used
///     to register an own marten instance. This store is used
///     to  save and retrieve volatile data.
/// </summary>
public interface IVolatileDataStore : IDocumentStore
{
}
