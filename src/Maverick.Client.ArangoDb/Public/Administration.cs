using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Administration;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     A class for interacting with ArangoDB Administration endpoints.
/// </summary>
/// <inheritdoc />
public class Administration : IAdministration
{
    private readonly Connection _connection;

    internal Administration(Connection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<GetServerVersionResponse> GetServerVersionAsync(bool details = false)
    {
        string requestUrl = details ? "?details=true" : "";

        var request = new Request(HttpMethod.Post, ApiBaseUri.Version, requestUrl);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        if (response.IsSuccessStatusCode)
        {
            return new GetServerVersionResponse(response, response.ParseBody<ServerInfos>());
        }

        return new GetServerVersionResponse(response, response.Exception);
    }
}
