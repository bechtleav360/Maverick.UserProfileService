using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains schema validation properties during setting uo a new collection.
/// </summary>
/// <remarks>
///     While ArangoDB is schema-less, it allows to enforce certain document structures on collection level. The desired
///     structure can be described in the popular JSON Schema format
/// </remarks>
public class ACollectionSchema
{
    /// <summary>
    ///     Controls when the validation will be applied.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public ASchemaLevel Level { get; set; }

    /// <summary>
    ///     The message that will be used when validation fails.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     It contains the JSON Schema description. It must be a valid schema object as outlined in
    ///     <see href="https://json-schema.org/specification.html" />.
    /// </summary>
    public string Rule { get; set; }
}
