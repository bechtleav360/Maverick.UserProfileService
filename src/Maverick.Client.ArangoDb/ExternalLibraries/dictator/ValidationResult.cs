using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

public class ValidationResult
{
    public bool IsValid => Violations.Count == 0;

    public List<Rule> Violations { get; }

    public ValidationResult()
    {
        Violations = new List<Rule>();
    }

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

    public void AddViolations(List<Rule> rules)
    {
        foreach (Rule rule in rules)
        {
            AddViolation(rule);
        }
    }
}
