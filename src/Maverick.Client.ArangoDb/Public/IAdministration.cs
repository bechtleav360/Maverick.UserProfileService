using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Administration;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     An interface for interacting with ArangoDB Administrations endpoints.
/// </summary>
public interface IAdministration
{
    /// <summary>
    ///     return the version of the current ArangoDB instance
    /// </summary>
    /// <param name="details">
    ///     if true, some details as the compiler, the host ID, the bundled V8 javascript engine version are
    ///     also returned
    /// </param>
    /// <returns>Object contains the server version and some other server information <see cref="GetServerVersionResponse" />.</returns>
    Task<GetServerVersionResponse> GetServerVersionAsync(bool details = false);
}
