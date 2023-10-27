using System.Reflection;
using UserProfileService.Queries.Language.Models;

namespace UserProfileService.Queries.Language.Validators;

/// <summary>
///     The validator contains methods to check if the request for a
///     object ist valid.
/// </summary>
public static class ValidatorHelper
{
    /// <summary>
    ///     Checks if the retrieved attribute is part of the result object.
    ///     It will also checked if the attribute has the right datatype.
    /// </summary>
    /// <param name="property">The property that should be checked if it is part of the <typeparamref name="TResult" /></param>
    /// <param name="dataTypeAllowed">The data type that are allowed. If the value is null, all data types are allowed.</param>
    /// <typeparam name="TResult">The object that is the result and the attribute should be part of.</typeparam>
    /// <returns>
    ///     Sets the validation to <see cref="PropertyValidationResult.PropertyValidationSuccess" /> if the property is part
    ///     of the
    ///     <typeparamref name="TResult" />. Set the validation to <see cref="PropertyValidationResult.PropertyNotInResultObject" />
    ///     if the given <paramref name="property" />
    ///     is part of the <typeparamref name="TResult" />. Returns <see cref="PropertyValidationResult.PropertyNotOfGivenType" />
    ///     when the given property is part of the <typeparamref name="TResult" /> but
    ///     the <paramref name="dataTypeAllowed" /> is not allowed.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     If the
    ///     <paramref name="property" />
    ///     is null, empty or contains whitespaces.
    /// </exception>
    public static PropertyValidationResult ValidatePropertyInResult<TResult>(
        string property,
        List<Type>? dataTypeAllowed = null)
    {
        if (string.IsNullOrWhiteSpace(property))
        {
            throw new ArgumentException(nameof(property));
        }

        Type objectType = typeof(TResult);

        PropertyInfo[] properties = objectType.GetProperties();

        List<string> propertiesNames = properties.Select(prop => prop.Name).ToList();

        Type? propertyDateType = objectType.GetProperty(property)?.PropertyType;

        if (dataTypeAllowed == null)
        {
            return propertiesNames.Contains(property)
                ? PropertyValidationResult.PropertyValidationSuccess
                : PropertyValidationResult.PropertyNotInResultObject;
        }

        if (!propertiesNames.Contains(property))
        {
            return PropertyValidationResult.PropertyNotInResultObject;
        }

        if (propertyDateType != null && !dataTypeAllowed.Contains(propertyDateType))
        {
            return PropertyValidationResult.PropertyNotOfGivenType;
        }

        return PropertyValidationResult.PropertyValidationSuccess;
    }
}
