using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Represents a validation rule.
/// </summary>
public class Rule
{
    private string _message;

    /// <summary>
    ///     Gets or sets the constraint for the rule.
    /// </summary>
    public Constraint Constraint { get; set; }

    /// <summary>
    ///     Gets or sets the field path.
    /// </summary>
    public string FieldPath { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the rule is violated.
    /// </summary>
    public bool IsViolated { get; set; }

    /// <summary>
    ///     Gets or sets the message for the rule.
    ///     If no message is set, a default message is generated.
    /// </summary>
    public string Message
    {
        get => _message ?? $"Field '{FieldPath}' violated '{Constraint}' constraint rule.";
        set => _message = value;
    }

    /// <summary>
    ///     Gets or sets the parameters for the rule.
    /// </summary>
    public List<object> Parameters { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Rule"/> class. 
    /// </summary>
    public Rule()
    {
        Parameters = new List<object>();
    }
}
