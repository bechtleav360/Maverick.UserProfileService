namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     A factory abstraction for a component that can create <see cref="IArangoDbClient" /> instances with custom
///     configuration for a given logical name.
/// </summary>
public interface IArangoDbClientFactory
{
    /// <summary>
    ///     Creates and configures a <see cref="IArangoDbClient" /> instance using the configuration that corresponds to the
    ///     logical name specified by name.
    /// </summary>
    /// <param name="name">The logical name of the client to create.</param>
    /// <returns>A new <see cref="IArangoDbClient" /> instance.</returns>
    IArangoDbClient Create(string name);
}
