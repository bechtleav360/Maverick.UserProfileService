using System;
using System.Runtime.CompilerServices;
using Maverick.UserProfileService.Models.Abstraction;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Validation.Abstractions;

/// <summary>
///     Describe a service to validate payloads and saga entities.
/// </summary>
public interface IPayloadValidationService
{
    /// <summary>
    ///     Validate model state of entity.
    /// </summary>
    /// <typeparam name="TPayload">Type of payload to be validated.</typeparam>
    /// <param name="payload">Payload to be validated.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    /// <returns>Results of validation.</returns>
    /// <returns></returns>
    public ValidationResult ValidateObject<TPayload>(
        TPayload payload,
        [CallerMemberName] string caller = null);

    //TODO: Think about validation versioning 
    /// <summary>
    ///     Validates properties against the original entity class.
    ///     It is checked whether properties may be changed and correspond to the property types.
    /// </summary>
    /// <typeparam name="TModifiableEntity">Type of modifiable entity type.</typeparam>
    /// <param name="payload">Properties to be checked.</param>
    /// <returns>Results of validation.</returns>
    public ValidationResult ValidateUpdateObjectProperties<TModifiableEntity>(
        PropertiesUpdatedPayload payload) where TModifiableEntity : class, new();

    /// <summary>
    ///     Validate the type of assignment between source and target objects.
    /// </summary>
    /// <param name="propertyName">Property name the validation result belongs to.</param>
    /// <param name="assignmentPayload">Payload to validate.</param>
    /// <param name="assignmentsSelector">Selector to get the assignments from payload.</param>
    /// <typeparam name="TObjectIdent">
    ///     The type of the <see cref="IObjectIdent" /> that will be used in the
    ///     <paramref name="assignmentsSelector" />
    /// </typeparam>
    /// <returns>Results of validation.</returns>
    public ValidationResult ValidateAssignment<TObjectIdent>(
        string propertyName,
        AssignmentPayload assignmentPayload,
        Func<AssignmentPayload, TObjectIdent[]> assignmentsSelector)
        where TObjectIdent : IObjectIdent;
}
