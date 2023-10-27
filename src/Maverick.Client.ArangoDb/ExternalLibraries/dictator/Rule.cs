using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

public class Rule
{
    private string _message;
    public Constraint Constraint { get; set; }

    public string FieldPath { get; set; }
    public bool IsViolated { get; set; }

    public string Message
    {
        get => _message ?? $"Field '{FieldPath}' violated '{Constraint}' constraint rule.";
        set => _message = value;
    }

    public List<object> Parameters { get; set; }

    public Rule()
    {
        Parameters = new List<object>();
    }
}
