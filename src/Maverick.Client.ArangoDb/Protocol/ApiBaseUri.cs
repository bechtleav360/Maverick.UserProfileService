// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Protocol;

internal static class ApiBaseUri
{
    internal static string AqlFunction = "_api/aqlfunction";
    internal static string Collection = "_api/collection";
    internal static string Cursor = "_api/cursor";
    internal static string Database = "_api/database";
    internal static string Document = "_api/document";
    internal static string Edges = "_api/edges";
    internal static string Index = "_api/index";
    internal static string Query = "_api/query";
    internal static string Transaction = "_api/transaction";
    internal static string Version = "_api/version";
}
