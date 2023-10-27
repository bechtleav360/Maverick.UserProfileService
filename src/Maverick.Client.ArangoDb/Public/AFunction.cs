using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Function;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     A class for interacting with ArangoDB Functions endpoints.
/// </summary>
/// <inheritdoc />
public class AFunction : IAFunction
{
    private readonly Connection _connection;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    internal AFunction(Connection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<AqlFuncResponse> RegisterAqlFuncAsync(string name, string code, bool isDeterministic = true)
    {
        var bodyDocument = new Dictionary<string, object>();
        // required
        bodyDocument.String(ParameterName.Name, name);
        // required
        bodyDocument.String(ParameterName.Code, code);

        // optional
        if (!isDeterministic)
        {
            IsDeterministic(isDeterministic);
        }

        Request.TrySetBodyParameter(ParameterName.IsDeterministic, _parameters, bodyDocument);

        var request = new Request<Dictionary<string, object>>(
            HttpMethod.Post,
            ApiBaseUri.AqlFunction,
            bodyDocument);

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new AqlFuncResponse(response, true);
        }

        return new AqlFuncResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetAQlFunctionsResponse> ListAqlFuncAsync(string givenNamespace = null)
    {
        string requestUrl = givenNamespace == null ? "" : $"?namespace={givenNamespace}";
        var request = new Request(HttpMethod.Get, ApiBaseUri.AqlFunction, requestUrl);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var res = response.ParseBody<Body<IList<FunctionEntity>>>();

            return new GetAQlFunctionsResponse(response, res?.Result);
        }

        return new GetAQlFunctionsResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<AqlFuncResponse> UnregisterFuncAsync(string name, bool group = false)
    {
        var requestUrl = $"?group={group}";
        var request = new Request(HttpMethod.Delete, ApiBaseUri.AqlFunction, "/" + name + requestUrl);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new AqlFuncResponse(response, true);
        }

        return new AqlFuncResponse(response, response.Exception);
    }

    /// <summary>
    ///     Determines whether function return value solely depends on the input value and return value is the same for
    ///     repeated calls with same input. This parameter is currently not applicable and may be used in the future for
    ///     optimisation purpose.
    /// </summary>
    public AFunction IsDeterministic(bool value)
    {
        _parameters.Bool(ParameterName.IsDeterministic, value);

        return this;
    }

    /// <summary>
    ///     Determines optional namespace from which to return all registered AQL user functions.
    /// </summary>
    public AFunction Namespace(string value)
    {
        _parameters.String(ParameterName.Namespace, value);

        return this;
    }

    /// <summary>
    ///     Determines whether the function name is treated as a namespace prefix, and all functions in the specified namespace
    ///     will be deleted. If set to false, the function name provided in name must be fully qualified, including any
    ///     namespaces. Default value: false.
    /// </summary>
    public AFunction Group(bool value)
    {
        _parameters.String(ParameterName.Group, value.ToString().ToLower());

        return this;
    }
}
