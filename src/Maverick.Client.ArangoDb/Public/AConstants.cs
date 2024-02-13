using System.Text.RegularExpressions;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains constant strings used by Arango and this client.
/// </summary>
public static class AConstants
{
    internal static readonly Regex KeyRegex = new Regex(@"^[a-zA-Z0-9_\-:.@()+,=;$!*'%]*$");

    /// <summary>
    ///     Gets the Arango client name.
    /// </summary>
    public const string ArangoClientName = "Arango_DB_Client";

    /// <summary>
    ///     Gets the separator string of a ArangoDB document handle (i.e. [collectionName][separator][key]).
    /// </summary>
    public const string DocumentHandleSeparator = "/";

    /// <summary>
    ///     Gets the property name of the ArangoDB id property.
    /// </summary>
    public const string IdSystemProperty = "_id";

    /// <summary>
    ///     Gets the property name of the ArangoDB key property.
    /// </summary>
    public const string KeySystemProperty = "_key";

    /// <summary>
    ///     Gets the name of the ArangoDB property used to identify the source node of an edge.
    /// </summary>
    public const string SystemPropertyFrom = "_from";

    /// <summary>
    ///     Gets the name of the ArangoDB property used to identify the destination node of an edge.
    /// </summary>
    public const string SystemPropertyTo = "_to";
}
