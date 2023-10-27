using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Foxx;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     A class for interacting with ArangoDB Foxxs endpoints.
/// </summary>
public class AFoxx
{
    private readonly Connection _connection;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    internal AFoxx(Connection connection)
    {
        _connection = connection;
    }

    private async Task<FoxxResponse<T>> RequestAsync<T>(HttpMethod httpMethod, string relativeUri)
    {
        Response response = null;

        if (_parameters.Has(ParameterName.Body))
        {
            var request = new Request<object>(httpMethod, relativeUri, _parameters.Object(ParameterName.Body));
            response = await RequestHandler.ExecuteAsync(_connection, request);
        }
        else
        {
            var request = new Request(httpMethod, relativeUri);
            response = await RequestHandler.ExecuteAsync(_connection, request);
        }

        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var result = response.ParseBody<Body<T>>();

            return result == null
                ? new FoxxResponse<T>(response, new Exception("Body of foxx response could not be parsed."))
                : new FoxxResponse<T>(response, result.Result);
        }

        return new FoxxResponse<T>(response, response.Exception);
    }

    /// <summary>
    ///     Serializes specified value as JSON object into request body.
    /// </summary>
    public AFoxx Body(object value)
    {
        _parameters.Object(ParameterName.Body, value);

        return this;
    }

    /// <summary>
    ///     Sends GET request to specified foxx service location.
    /// </summary>
    public async Task<FoxxResponse<T>> GetFoxxAsync<T>(string relativeUri)
    {
        return await RequestAsync<T>(HttpMethod.Get, relativeUri).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sends POST request to specified foxx service location.
    /// </summary>
    public async Task<FoxxResponse<T>> PostFoxxAsync<T>(string relativeUri)
    {
        return await RequestAsync<T>(HttpMethod.Post, relativeUri).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sends PUT request to specified foxx service location.
    /// </summary>
    public async Task<FoxxResponse<T>> PutFoxxAsync<T>(string relativeUri)
    {
        return await RequestAsync<T>(HttpMethod.Put, relativeUri).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sends PATCH request to specified foxx service location.
    /// </summary>
    public async Task<FoxxResponse<T>> PatchFoxxAsync<T>(string relativeUri)
    {
        return await RequestAsync<T>(HttpMethod.Patch, relativeUri).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sends DELETE request to specified foxx service location.
    /// </summary>
    public async Task<FoxxResponse<T>> DeleteFoxxAsync<T>(string relativeUri)
    {
        return await RequestAsync<T>(HttpMethod.Delete, relativeUri).ConfigureAwait(false);
    }
}
