namespace Maverick.Client.ArangoDb.Public.Models.Function;

/// <summary>
///     Contains the attribute of an AQL Function
/// </summary>
public class FunctionEntity
{
    /// <summary>
    ///     A string representation of the function body
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     an optional boolean value to indicate whether the function
    ///     results are fully deterministic(function return value solely depends on
    ///     the input value and return value is the same for repeated calls with same
    ///     input). The isDeterministic attribute is currently not used but may be
    ///     used later for optimizations.
    /// </summary>
    public bool IsDeterministic { get; set; }

    /// <summary>
    ///     The fully qualified name of the user function
    /// </summary>
    public string Name { get; set; }
}
