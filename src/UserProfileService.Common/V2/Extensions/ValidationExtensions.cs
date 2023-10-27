using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains helping methods to validate model classes.
/// </summary>
public static class ValidationExtensions
{
    private static string SerializeToJsonString(this object obj)
    {
        return obj == null
            ? "{}"
            : JsonConvert.SerializeObject(obj, Formatting.None);
    }

    /// <summary>
    ///     Throws a <see cref="ValidationException" /> if the given object instance is not valid.
    /// </summary>
    /// <remarks>
    ///     This method evaluates all <see cref="ValidationAttribute" />s attached to the object's type. <br />
    ///     It also validates all the object's properties.
    /// </remarks>
    /// <param name="instance">The object instance to test.  It cannot be null.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="instance" /> is null.</exception>
    /// <exception cref="ValidationException">When <paramref name="instance" /> is found to be invalid.</exception>
    public static void Validate(this QueryObjectList instance)
    {
        if (instance == null)
        {
            return;
        }

        Validator.ValidateObject(instance, new ValidationContext(instance), true);
    }

    /// <summary>
    ///     Determines of the given object instance is valid. If it is valid, it wil return <c>true</c>, otherwise <c>false</c>
    ///     .
    /// </summary>
    /// <remarks>
    ///     This method evaluates all <see cref="ValidationAttribute" />s attached to the object's type. <br />
    ///     It also validates all the object's properties.
    /// </remarks>
    /// <returns><c>true</c>, it the <paramref name="instance" /> validates.</returns>
    /// <param name="instance">The object instance to test.  It cannot be null.</param>
    /// <param name="validationResults">The results of all failed validation checks.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="instance" /> is null.</exception>
    public static bool TryValidate(this QueryObjectList instance, out IList<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();

        if (instance == null)
        {
            return true;
        }

        return Validator.TryValidateObject(instance, new ValidationContext(instance), validationResults, true);
    }

    /// <summary>
    ///     Throws a <see cref="ValidationException" /> if the given object instance is not valid.
    /// </summary>
    /// <remarks>
    ///     This method evaluates all <see cref="ValidationAttribute" />s attached to the object's type. <br />
    ///     It also validates all the object's properties.
    /// </remarks>
    /// <param name="instance">The object instance to test.  It cannot be null.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="instance" /> is null.</exception>
    /// <exception cref="ValidationException">When <paramref name="instance" /> is found to be invalid.</exception>
    public static void Validate(this QueryObject instance)
    {
        if (instance == null)
        {
            return;
        }

        Validator.ValidateObject(instance, new ValidationContext(instance), true);
    }

    /// <summary>
    ///     Throws a <see cref="ValidationException" /> if the given object instance is not valid.<br />
    ///     This method overload will write debug log messages to a provided <paramref name="logger" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method evaluates all <see cref="ValidationAttribute" />s attached to the object's type. <br />
    ///     It also validates all the object's properties.
    /// </remarks>
    /// <param name="instance">The object instance to test.  It cannot be null.</param>
    /// <param name="logger">The logger that willa accept debug messages of this method.</param>
    /// <param name="caller">The calling method name.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="instance" /> is null.</exception>
    /// <exception cref="ValidationException">When <paramref name="instance" /> is found to be invalid.</exception>
    public static void Validate(
        this QueryObject instance,
        ILogger logger,
        [CallerMemberName] string caller = null)
    {
        if (instance == null)
        {
            logger?.LogDebugMessage("Query object is null.", LogHelpers.Arguments(), caller);

            return;
        }

        logger?.LogDebugMessage(
            "Using query object: {queryObject}",
            LogHelpers.Arguments(instance.SerializeToJsonString()),
            caller);

        Validator.ValidateObject(instance, new ValidationContext(instance), true);
    }

    /// <summary>
    ///     Determines of the given object instance is valid. If it is valid, it wil return <c>true</c>, otherwise <c>false</c>
    ///     .
    /// </summary>
    /// <remarks>
    ///     This method evaluates all <see cref="ValidationAttribute" />s attached to the object's type. <br />
    ///     It also validates all the object's properties.
    /// </remarks>
    /// <returns><c>true</c>, it the <paramref name="instance" /> validates.</returns>
    /// <param name="instance">The object instance to test.  It cannot be null.</param>
    /// <param name="validationResults">The results of all failed validation checks.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="instance" /> is null.</exception>
    public static bool TryValidate(this QueryObject instance, out IList<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();

        if (instance == null)
        {
            return true;
        }

        return Validator.TryValidateObject(instance, new ValidationContext(instance), validationResults, true);
    }

    /// <summary>
    ///     Generates an output string for a collection of <see cref="ValidationResult" />s.
    /// </summary>
    /// <remarks>
    ///     Is human-friendly and can be used in loggers.<br />
    ///     If <paramref name="validationResult" /> is empty, an empty string will be returned.
    /// </remarks>
    /// <param name="validationResult">The </param>
    /// <returns>A string representation of <paramref name="validationResult" />.</returns>
    /// <exception cref="ArgumentNullException">If the parameter <paramref name="validationResult" /> is null.</exception>
    public static string GetOutputString(this IList<ValidationResult> validationResult)
    {
        if (validationResult == null)
        {
            throw new ArgumentNullException(nameof(validationResult));
        }

        if (!validationResult.Any())
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder($"Validation issues detected:{Environment.NewLine}");

        foreach (ValidationResult result in validationResult)
        {
            stringBuilder.AppendLine(" * ");
            stringBuilder.Append(result.ErrorMessage);

            if (result.MemberNames != null && result.MemberNames.Any())
            {
                stringBuilder.Append(" (related members: ");
                stringBuilder.Append(string.Join(",", result.ErrorMessage));
                stringBuilder.Append(")");
            }

            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }
}
