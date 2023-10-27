using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Validation.Abstractions;

/// <summary>
///     Summarize a list of validation results.
/// </summary>
public class ValidationResult
{
    /// <summary>
    ///     A collection of errors
    /// </summary>
    public IList<ValidationAttribute> Errors { get; set; }

    /// <summary>
    ///     Whether validation succeeded
    /// </summary>
    public virtual bool IsValid => Errors.Count == 0;

    internal ValidationResult(List<ValidationAttribute> errors)
    {
        Errors = errors;
    }

    /// <summary>
    ///     Creates a new validationResult
    /// </summary>
    public ValidationResult()
    {
        Errors = new List<ValidationAttribute>();
    }

    /// <summary>
    ///     Creates a new ValidationSumResult from a collection of failures
    /// </summary>
    /// <param name="failures">
    ///     List of <see cref="ValidationAttribute" /> which is later available through <see cref="Errors" />.
    ///     This list get's copied.
    /// </param>
    /// <remarks>
    ///     Every caller is responsible for not adding <c>null</c> to the list.
    /// </remarks>
    public ValidationResult(IEnumerable<ValidationAttribute> failures)
    {
        Errors = failures.Where(failure => failure != null).ToList();
    }

    /// <summary>
    ///     Creates a new ValidationSumResult from a collection of failures
    /// </summary>
    /// <param name="failures">
    ///     List of <see cref="ValidationAttribute" /> which is later available through <see cref="Errors" />.
    ///     This list get's copied.
    /// </param>
    /// <remarks>
    ///     Every caller is responsible for not adding <c>null</c> to the list.
    /// </remarks>
    public ValidationResult(params ValidationAttribute[] failures)
    {
        Errors = failures.Where(failure => failure != null).ToList();
    }

    /// <summary>
    ///     Generates a string representation of the error messages separated by new lines.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ToString(Environment.NewLine);
    }

    /// <summary>
    ///     Generates a string representation of the error messages separated by the specified character.
    /// </summary>
    /// <param name="separator">The character to separate the error messages.</param>
    /// <returns></returns>
    public string ToString(string separator)
    {
        return string.Join(separator, Errors.Select(failure => failure.ErrorMessage));
    }
}

/// <summary>
///     The typed validation result.
/// </summary>
/// <typeparam name="T">The generic type of the result.</typeparam>
public class ValidationResult<T> : ValidationResult
{
    /// <summary>
    ///     Object that was generated or retrieved during validation and is necessary for further processing.
    /// </summary>
    public T Facade { get; }

    /// <summary>
    ///     Creates a new validationResult
    /// </summary>
    /// <param name="facade">
    ///     Object that was generated or retrieved during validation and is necessary for further processing
    /// </param>
    public ValidationResult(T facade)
    {
        Facade = facade;
        Errors = new List<ValidationAttribute>();
    }

    /// <summary>
    ///     Creates a new ValidationSumResult from a collection of failures
    /// </summary>
    /// <param name="failures">
    ///     List of <see cref="ValidationAttribute" /> which is later available through <see cref="ValidationResult.Errors" />.
    ///     This list get's copied.
    /// </param>
    /// <remarks>
    ///     Every caller is responsible for not adding <c>null</c> to the list.
    /// </remarks>
    public ValidationResult(params ValidationAttribute[] failures) : base(failures)
    {
    }
}
