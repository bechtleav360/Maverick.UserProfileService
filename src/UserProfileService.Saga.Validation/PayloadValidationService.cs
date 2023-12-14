using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentValidation;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Events.Payloads.Contracts;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Validation.Abstractions;
using FluentValidationResult = FluentValidation.Results.ValidationResult;
using ValidationAttribute = UserProfileService.Validation.Abstractions.ValidationAttribute;
using ValidationResult = UserProfileService.Validation.Abstractions.ValidationResult;

namespace UserProfileService.Saga.Validation;

/// <summary>
///     The service validates the payloads of the messages.
/// </summary>
public class PayloadValidationService : IPayloadValidationService
{
    private readonly ILogger<PayloadValidationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Create an instance of <see cref="PayloadValidationService" />.
    /// </summary>
    /// <param name="serviceProvider">Service for retrieving service objects.</param>
    /// <param name="logger">The logger.</param>
    public PayloadValidationService(IServiceProvider serviceProvider, ILogger<PayloadValidationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///     Check if the given properties are party of the modifiable entity.
    /// </summary>
    /// <param name="properties">Properties to be checked.</param>
    /// <returns>
    ///     Invalid properties as collection of
    ///     <see cref="UserProfileService.Validation.Abstractions.ValidationAttribute" />.
    /// </returns>
    private IList<ValidationAttribute> CheckPropertiesAreModifiable<
        TModifiableEntity>(
        IDictionary<string, object> properties) where TModifiableEntity : class, new()
    {
        const string errorMessage =
            "The specified property cannot be changed because it is not part of the entity or is not allowed to change.";

        List<ValidationAttribute> invalidProperties = properties
            .Where(
                p =>
                {
                    PropertyInfo propertyInfo =
                        typeof(TModifiableEntity).GetProperty(p.Key);

                    return propertyInfo == null
                        || GetCustomAttributeValue<ModifiableAttribute,
                            bool>(
                            propertyInfo,
                            cp => !cp?.AllowEdit ?? false);
                })
            .Select(p => new ValidationAttribute(p.Key, errorMessage))
            .ToList();

        return invalidProperties;
    }

    /// <summary>
    ///     Check if the given property values have the correct type.
    /// </summary>
    /// <typeparam name="TModifiableEntity">Type of modifiable entity to be checked and the properties belongs to.</typeparam>
    /// <param name="properties">Properties to be checked.</param>
    /// <returns>
    ///     Invalid properties as collection of
    ///     <see cref="UserProfileService.Validation.Abstractions.ValidationAttribute" />.
    /// </returns>
    private static IList<ValidationAttribute> CheckPropertiesValueType<TModifiableEntity>(
        IDictionary<string, object> properties) where TModifiableEntity : class, new()
    {
        // Check if the given property values have the correct type.
        var modifiableEntity = new TModifiableEntity();

        List<ValidationAttribute> invalidProperties = properties
            .Where(p => typeof(TModifiableEntity).GetProperty(p.Key) != null)
            .Where(
                p => !ValidatePropertyType(
                    modifiableEntity,
                    p.Key,
                    p.Value))
            .Select(
                p => new ValidationAttribute(
                    p.Key,
                    "The specified property cannot be changed because it has the wrong data type"))
            .ToList();

        var ctx = new ValidationContext(modifiableEntity);
        var validatorResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        bool validModelState = Validator.TryValidateObject(
            modifiableEntity,
            ctx,
            validatorResults,
            true);

        List<ValidationAttribute> invalidModelStateProperties = validatorResults.SelectMany(
                t =>
                    t.MemberNames
                        .Select(m => new ValidationAttribute(m, t.ErrorMessage)))
            .ToList();

        if (!validModelState)
        {
            // Only property errors are added if the property has been changed and is not already marked as invalid.
            invalidProperties.AddRange(
                invalidModelStateProperties.Where(
                    x =>
                        invalidProperties.All(y => y.Member != x.Member) && properties.ContainsKey(x.Member)));
        }

        return invalidProperties;
    }

    private static bool ValidatePropertyType<TModifiableEntity>(
        TModifiableEntity modifiableEntity,
        string key,
        object value)
    {
        PropertyInfo propertyInfo = modifiableEntity.GetType().GetProperty(key);

        try
        {
            Type valueType = value?.GetType();

            if (propertyInfo?.PropertyType == valueType
                || propertyInfo?.PropertyType.IsAssignableFrom(valueType) == true)
            {
                propertyInfo?.SetValue(modifiableEntity, value, null);

                return true;
            }

            // Check if value is an object like JArray and is convertible to property type.
            if (value is JContainer jsonValue)
            {
                if (propertyInfo?.PropertyType != null)
                {
                    jsonValue.ToObject(propertyInfo.PropertyType);
                }

                return true;
            }

            propertyInfo?.SetValue(modifiableEntity, Convert.ChangeType(value, propertyInfo.PropertyType), null);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public ValidationResult ValidateObject<TPayload>(
        TPayload payload,
        [CallerMemberName] string caller = null)
    {
        if (payload == null)
        {
            var nullValidationResult = new ValidationAttribute(string.Empty, "Payload is null or empty.");

            return new ValidationResult(nullValidationResult);
        }

        var validator = _serviceProvider.GetService<IValidator<TPayload>>();

        if (validator == null)
        {
            // The dependency to the common package should not be created.
            var message = "{caller}(): No validator defined for entity '{entityName}'.";
            _logger.LogWarning(message, caller, payload.GetType().Name);
        }

        FluentValidationResult validatorResults = validator?.Validate(payload) ?? new FluentValidationResult();

        List<ValidationAttribute> results = validatorResults.Errors.Select(
                t =>
                    new ValidationAttribute(
                        t.PropertyName,
                        t.ErrorMessage))
            .ToList();

        return new ValidationResult(results);
    }

    /// <inheritdoc />
    public ValidationResult ValidateUpdateObjectProperties<TModifiableEntity>(
        PropertiesUpdatedPayload payload)
        where TModifiableEntity : class, new()
    {
        if (payload?.Properties == null || !payload.Properties.Any())
        {
            var nullValidationResult = new ValidationAttribute(string.Empty, "Payload is null or empty.");

            return new ValidationResult(nullValidationResult);
        }

        IList<ValidationAttribute> invalidProperties =
            CheckPropertiesAreModifiable<TModifiableEntity>(payload.Properties);

        IList<ValidationAttribute> invalidTypeProperties =
            CheckPropertiesValueType<TModifiableEntity>(payload.Properties);

        invalidProperties = invalidProperties.Concat(
                invalidTypeProperties.Where(x => invalidProperties.All(y => y.Member != x.Member)))
            .ToList();

        return new ValidationResult(invalidProperties);
    }

    /// <inheritdoc />
    public ValidationResult ValidateAssignment<TObjectIdent>(
        string propertyName,
        AssignmentPayload assignmentPayload,
        Func<AssignmentPayload, TObjectIdent[]> assignmentsSelector)
        where TObjectIdent: IObjectIdent
    {
        ObjectIdent objectIdent = assignmentPayload.Resource;
        TObjectIdent[] assignments = assignmentsSelector.Invoke(assignmentPayload);

        if (WellKnownAssignments.Dict.TryGetValue(
                objectIdent.Type,
                out ICollection<(ObjectType objectType, AssignmentType assType)> objectCombination))
        {
            if (!assignments.All(
                    a =>
                        objectCombination.Any(o => o.objectType == a.Type && assignmentPayload.Type == o.assType)))
            {
                var validationResult = new ValidationAttribute(
                    propertyName,
                    $"The assignment is not allowed. For type {objectIdent.Type} you can only assign {string.Join(" , ", objectCombination.Select(t => t.ToString()))}.");

                return new ValidationResult(validationResult);
            }
        }
        else
        {
            var validationResult = new ValidationAttribute(
                propertyName,
                $"No assignment for type '{objectIdent.Type}' allowed.");

            return new ValidationResult(validationResult);
        }

        return new ValidationResult();
    }

    /// <summary>
    ///     Returns the value of a custom attribute defined by the parameter <paramref name="valueFunc" />.
    /// </summary>
    /// <typeparam name="TAttribute">Type of custom attribute.</typeparam>
    /// <typeparam name="TValue">Return value type of parameter <paramref name="valueFunc" />.</typeparam>
    /// <param name="type">Type from which the attribute is to be returned. </param>
    /// <param name="valueFunc">Function that defines which property should be returned.</param>
    /// <returns>
    ///     Value of custom property by the parameter <paramref name="valueFunc" />. If it does not exist the default
    ///     value is returned.
    /// </returns>
    private TValue GetCustomAttributeValue<TAttribute, TValue>(
        MemberInfo type,
        Func<TAttribute, TValue> valueFunc) where TAttribute : Attribute
    {
        var attribute = type.GetCustomAttribute(typeof(TAttribute)) as TAttribute;

        return attribute == null ? default : valueFunc.Invoke(attribute);
    }
}
