using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Function;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     An interface for interacting with ArangoDB Functions endpoints.
/// </summary>
public interface IAFunction
{
    /// <summary>
    ///     Creates new or replaces existing AQL user function with specified name and code.
    /// </summary>
    /// <param name="name">The fully qualified name of the user functions.</param>
    /// <param name="code">A string representation of the function body.</param>
    /// <param name="isDeterministic">
    ///     n optional boolean value to indicate whether the function
    ///     results are fully deterministic(function return value solely depends on
    ///     the input value and return value is the same for repeated calls with same
    ///     input).
    /// </param>
    /// <returns>
    ///     Object contains boolean value which takes the value true when the registration of the AQL function has been
    ///     successful
    ///     / or possibly occurred errors <see cref="AqlFuncResponse" />.
    /// </returns>
    Task<AqlFuncResponse> RegisterAqlFuncAsync(string name, string code, bool isDeterministic = true);

    /// <summary>
    ///     Retrieves list of registered AQL user functions.
    /// </summary>
    /// <param name="givenNamespace">The given namespace</param>
    /// <returns>
    ///     Object containing containing a list of registered AQL user functions <see cref="GetAQlFunctionsResponse" />
    /// </returns>
    Task<GetAQlFunctionsResponse> ListAqlFuncAsync(string givenNamespace = null);

    /// <summary>
    ///     Unregisters specified AQL user function.
    /// </summary>
    /// <param name="name">he name of the AQL user function.</param>
    /// <param name="group">
    ///     If true: The function name provided in name is treated as
    ///     a namespace prefix, and all functions in the specified namespace will be deleted.
    ///     The returned number of deleted functions may become 0 if none matches the string.
    ///     If false: The function name provided in name must be fully
    ///     qualified, including any namespaces.If none matches the name, HTTP 404 is returned.
    /// </param>
    /// <returns>
    ///     Object contains containing a boolean value which takes the value true if the unregistration of the AQL
    ///     function has been successful <see cref="AqlFuncResponse" />
    /// </returns>
    Task<AqlFuncResponse> UnregisterFuncAsync(string name, bool group = false);
}
