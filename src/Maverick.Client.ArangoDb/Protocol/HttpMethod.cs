// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Protocol;

internal enum HttpMethod
{
    Delete,
    Get,
    Head,
    Options,
    Patch,
    Post,
    Put,
    Trace
}
