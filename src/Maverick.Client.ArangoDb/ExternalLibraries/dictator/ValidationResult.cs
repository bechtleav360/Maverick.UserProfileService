using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Represents the result of a validation process.
/// </summary>
public class ValidationResult
{
    /// <summary>
    ///     Gets a value indicating whether the validation result is valid (no violations).
    /// </summary>
    public bool IsValid => Violations.Count == 0;

    /// <summary>
    ///     Gets the list of violated rules.
    /// </summary>
    public List<Rule> Violations { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    public ValidationResult()
    {
        Violations = new List<Rule>();
    }

    /// <summary>
    ///     Adds a single violation rule to the result.
    /// </summary>
    /// <param name="rule">The violated rule to add.</param>
    public void AddViolation(Rule rule)
    {
        var violatedRule = new Rule
        {
            FieldPath = rule.FieldPath,
            Constraint = rule.Constraint,
            Parameters = rule.Parameters,
            Message = rule.Message,
            IsViolated = true
        };

        Violations.Add(violatedRule);
    }

    /// <summary>
    ///     Adds multiple violation rules to the result.
    /// </summary>
    /// <param name="rules">The list of violated rules to add.</param>
    public void AddViolations(List<Rule> rules)
    {
        foreach (Rule rule in rules)
        {
            AddViolation(rule);
        }
    }
}