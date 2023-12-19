namespace UserProfileService.Saga.Validation.Abstractions;

/// <summary>
/// Represents a custom validation service that extends the functionality of an existing validation service.
/// </summary>
/// <remarks>
/// This interface is designed to customize and enhance the capabilities of the underlying validation service (<see cref="IValidationService"/>).
/// Implement this interface to provide additional validation features or behavior.
/// </remarks>
public interface ICustomValidationService : IValidationService
{
}
