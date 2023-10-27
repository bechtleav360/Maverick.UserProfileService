using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

public class Schema
{
    private Constraint _lastAddedConstraint;
    private string _lastAddedFieldPath = "";
    private readonly List<Rule> _rules = new List<Rule>();

    private List<Rule> ValidateFieldValueRules(List<Rule> fieldValueRules, Dictionary<string, object> document)
    {
        var ruleViolations = new List<Rule>();

        foreach (Rule fieldValueRule in fieldValueRules)
        {
            switch (fieldValueRule.Constraint)
            {
                case Constraint.NotNull:
                    if (!ValidateNotNullConstraint(fieldValueRule, document))
                    {
                        ruleViolations.Add(fieldValueRule);
                    }

                    break;
                case Constraint.Type:
                    if (!ValidateTypeConstraint(fieldValueRule, document))
                    {
                        ruleViolations.Add(fieldValueRule);
                    }

                    break;
                case Constraint.Min:
                    if (!ValidateMinConstraint(fieldValueRule, document))
                    {
                        ruleViolations.Add(fieldValueRule);
                    }

                    break;
                case Constraint.Max:
                    if (!ValidateMaxConstraint(fieldValueRule, document))
                    {
                        ruleViolations.Add(fieldValueRule);
                    }

                    break;
                case Constraint.Range:
                    if (!ValidateRangeConstraint(fieldValueRule, document))
                    {
                        ruleViolations.Add(fieldValueRule);
                    }

                    break;
                case Constraint.Size:
                    if (!ValidateSizeConstraint(fieldValueRule, document))
                    {
                        ruleViolations.Add(fieldValueRule);
                    }

                    break;
                case Constraint.Match:
                    if (!ValidateMatchConstraint(fieldValueRule, document))
                    {
                        ruleViolations.Add(fieldValueRule);
                    }

                    break;
            }
        }

        return ruleViolations;
    }

    private bool ValidateNotNullConstraint(Rule fieldValueRule, Dictionary<string, object> document)
    {
        return document.IsNotNull(fieldValueRule.FieldPath);
    }

    private bool ValidateTypeConstraint(Rule fieldValueRule, Dictionary<string, object> document)
    {
        return document.IsType(fieldValueRule.FieldPath, (Type)fieldValueRule.Parameters[0]);
    }

    private bool ValidateMinConstraint(Rule fieldValueRule, Dictionary<string, object> document)
    {
        var minValue = (int)fieldValueRule.Parameters[0];
        object fieldValue = document.Object(fieldValueRule.FieldPath);

        if (fieldValue is string)
        {
            if (((string)fieldValue).Length >= minValue)
            {
                return true;
            }
        }
        else if (fieldValue is byte
                 || fieldValue is sbyte
                 || fieldValue is short
                 || fieldValue is ushort
                 || fieldValue is int
                 || fieldValue is uint
                 || fieldValue is long
                 || fieldValue is ulong)
        {
            if (Convert.ToInt64(fieldValue) >= minValue)
            {
                return true;
            }
        }
        else if (document.IsList(fieldValueRule.FieldPath) || document.IsArray(fieldValueRule.FieldPath))
        {
            if (document.Size(fieldValueRule.FieldPath) >= minValue)
            {
                return true;
            }
        }

        return false;
    }

    private bool ValidateMaxConstraint(Rule fieldValueRule, Dictionary<string, object> document)
    {
        var maxValue = (int)fieldValueRule.Parameters[0];
        object fieldValue = document.Object(fieldValueRule.FieldPath);

        if (fieldValue is string)
        {
            if (((string)fieldValue).Length <= maxValue)
            {
                return true;
            }
        }
        else if (fieldValue is byte
                 || fieldValue is sbyte
                 || fieldValue is short
                 || fieldValue is ushort
                 || fieldValue is int
                 || fieldValue is uint
                 || fieldValue is long
                 || fieldValue is ulong)
        {
            if (Convert.ToInt64(fieldValue) <= maxValue)
            {
                return true;
            }
        }
        else if (document.IsList(fieldValueRule.FieldPath) || document.IsArray(fieldValueRule.FieldPath))
        {
            if (document.Size(fieldValueRule.FieldPath) <= maxValue)
            {
                return true;
            }
        }

        return false;
    }

    private bool ValidateRangeConstraint(Rule fieldValueRule, Dictionary<string, object> document)
    {
        var minValue = (int)fieldValueRule.Parameters[0];
        var maxValue = (int)fieldValueRule.Parameters[1];
        object fieldValue = document.Object(fieldValueRule.FieldPath);

        if (fieldValue is string)
        {
            if (((string)fieldValue).Length >= minValue && ((string)fieldValue).Length <= maxValue)
            {
                return true;
            }
        }
        else if (fieldValue is byte
                 || fieldValue is sbyte
                 || fieldValue is short
                 || fieldValue is ushort
                 || fieldValue is int
                 || fieldValue is uint
                 || fieldValue is long
                 || fieldValue is ulong)
        {
            if (Convert.ToInt64(fieldValue) >= minValue && Convert.ToInt64(fieldValue) <= maxValue)
            {
                return true;
            }
        }
        else if (document.IsList(fieldValueRule.FieldPath) || document.IsArray(fieldValueRule.FieldPath))
        {
            int size = document.Size(fieldValueRule.FieldPath);

            if (size >= minValue && size <= maxValue)
            {
                return true;
            }
        }

        return false;
    }

    private bool ValidateSizeConstraint(Rule fieldValueRule, Dictionary<string, object> document)
    {
        var sizeValue = (int)fieldValueRule.Parameters[0];

        if (document.IsString(fieldValueRule.FieldPath))
        {
            string fieldValue = document.String(fieldValueRule.FieldPath);

            if (fieldValue.Length == sizeValue)
            {
                return true;
            }
        }
        else if ((document.IsList(fieldValueRule.FieldPath) || document.IsArray(fieldValueRule.FieldPath))
                 && document.Size(fieldValueRule.FieldPath) == sizeValue)
        {
            return true;
        }

        return false;
    }

    private bool ValidateMatchConstraint(Rule fieldValueRule, Dictionary<string, object> document)
    {
        var patternValue = (string)fieldValueRule.Parameters[0];
        var ignoreCase = (bool)fieldValueRule.Parameters[1];
        object fieldValue = document.Object(fieldValueRule.FieldPath);

        if (fieldValue is string)
        {
            if (ignoreCase)
            {
                if (Regex.IsMatch((string)fieldValue, patternValue, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
            else
            {
                if (Regex.IsMatch((string)fieldValue, patternValue))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Specifies field path which must exist in the document schema and must conform to further constraints.
    /// </summary>
    public Schema MustHave(string fieldPath)
    {
        _lastAddedConstraint = Constraint.MustHave;
        _lastAddedFieldPath = fieldPath;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            _rules.Add(rule);
        }

        return this;
    }

    /// <summary>
    ///     Specifies field path which if exists must conform to further constraints.
    /// </summary>
    public Schema ShouldHave(string fieldPath)
    {
        _lastAddedConstraint = Constraint.ShouldHave;
        _lastAddedFieldPath = fieldPath;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            _rules.Add(rule);
        }

        return this;
    }

    /// <summary>
    ///     Previously specified field path cannot have null value.
    /// </summary>
    public Schema NotNull()
    {
        _lastAddedConstraint = Constraint.NotNull;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            _rules.Add(rule);
        }

        return this;
    }

    /// <summary>
    ///     Previously specified field path value must be of specified type.
    /// </summary>
    public Schema Type<T>()
    {
        return Type(typeof(T));
    }

    /// <summary>
    ///     Previously specified field path value must be of specified type.
    /// </summary>
    public Schema Type(Type type)
    {
        _lastAddedConstraint = Constraint.Type;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            rule.Parameters.Add(type);

            _rules.Add(rule);
        }
        else
        {
            rule.Parameters.Clear();
            rule.Parameters.Add(type);
        }

        return this;
    }

    /// <summary>
    ///     Previously specified field path must have specified minimal value.
    /// </summary>
    public Schema Min(int minValue)
    {
        _lastAddedConstraint = Constraint.Min;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            rule.Parameters.Add(minValue);

            _rules.Add(rule);
        }
        else
        {
            rule.Parameters.Clear();
            rule.Parameters.Add(minValue);
        }

        return this;
    }

    /// <summary>
    ///     Previously specified field path must have specified maximal value.
    /// </summary>
    public Schema Max(int maxValue)
    {
        _lastAddedConstraint = Constraint.Max;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            rule.Parameters.Add(maxValue);

            _rules.Add(rule);
        }
        else
        {
            rule.Parameters.Clear();
            rule.Parameters.Add(maxValue);
        }

        return this;
    }

    /// <summary>
    ///     Previously specified field path must be in specified range.
    /// </summary>
    public Schema Range(int minValue, int maxValue)
    {
        _lastAddedConstraint = Constraint.Range;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            rule.Parameters.Add(minValue);
            rule.Parameters.Add(maxValue);

            _rules.Add(rule);
        }
        else
        {
            rule.Parameters.Clear();
            rule.Parameters.Add(minValue);
            rule.Parameters.Add(maxValue);
        }

        return this;
    }

    /// <summary>
    ///     Previously specified field path must have specified number of items in collection.
    /// </summary>
    public Schema Size(int collectionSize)
    {
        _lastAddedConstraint = Constraint.Size;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            rule.Parameters.Add(collectionSize);

            _rules.Add(rule);
        }
        else
        {
            rule.Parameters.Clear();
            rule.Parameters.Add(collectionSize);
        }

        return this;
    }

    /// <summary>
    ///     Previously specified field path must match specified regular expression.
    /// </summary>
    public Schema Match(string regex)
    {
        return Match(regex, false);
    }

    /// <summary>
    ///     Previously specified field path must match specified regular expression.
    /// </summary>
    public Schema Match(string regex, bool ignoreCase)
    {
        _lastAddedConstraint = Constraint.Match;

        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule == null)
        {
            rule = new Rule
            {
                FieldPath = _lastAddedFieldPath,
                Constraint = _lastAddedConstraint
            };

            rule.Parameters.Add(regex);
            rule.Parameters.Add(ignoreCase);

            _rules.Add(rule);
        }
        else
        {
            rule.Parameters.Clear();
            rule.Parameters.Add(regex);
            rule.Parameters.Add(ignoreCase);
        }

        return this;
    }

    /// <summary>
    ///     Specifies custom error message if previous schema constraints were violated.
    /// </summary>
    public Schema Message(string errorMessage)
    {
        Rule rule = _rules.FirstOrDefault(
            r =>
                r.FieldPath == _lastAddedFieldPath && r.Constraint == _lastAddedConstraint);

        if (rule != null)
        {
            rule.Message = errorMessage;
        }

        return this;
    }

    /// <summary>
    ///     Performs validation based on previous constraints on specified document.
    /// </summary>
    public ValidationResult Validate(Dictionary<string, object> document)
    {
        var validationResult = new ValidationResult();

        List<Rule> fieldRules = _rules.Where(
                rule =>
                    rule.Constraint == Constraint.MustHave || rule.Constraint == Constraint.ShouldHave)
            .ToList();

        foreach (Rule fieldRule in fieldRules)
        {
            switch (fieldRule.Constraint)
            {
                case Constraint.MustHave:
                    if (document.Has(fieldRule.FieldPath))
                    {
                        List<Rule> fieldValueRules = _rules.Where(
                                rule =>
                                    rule.FieldPath == fieldRule.FieldPath && rule.Constraint != Constraint.MustHave)
                            .ToList();

                        validationResult.AddViolations(ValidateFieldValueRules(fieldValueRules, document));
                    }
                    else
                    {
                        validationResult.AddViolation(fieldRule);
                    }

                    break;
                case Constraint.ShouldHave:
                    if (document.Has(fieldRule.FieldPath))
                    {
                        List<Rule> fieldValueRules = _rules.Where(
                                rule =>
                                    rule.FieldPath == fieldRule.FieldPath
                                    && rule.Constraint != Constraint.ShouldHave)
                            .ToList();

                        validationResult.AddViolations(ValidateFieldValueRules(fieldValueRules, document));
                    }

                    break;
            }
        }

        return validationResult;
    }
}
