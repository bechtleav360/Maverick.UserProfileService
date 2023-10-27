// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Protocol;

internal class Body<T> : Body
{
    public T Result { get; set; }
}

internal class Body
{
    public bool Cached { get; set; }
    public int Code { get; set; }

    public long Count { get; set; }

    // standard response fields
    public bool Error { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorNum { get; set; }
    public object Extra { get; set; }
    public bool HasMore { get; set; }

    // operation specific fields
    public string Id { get; set; }
}
