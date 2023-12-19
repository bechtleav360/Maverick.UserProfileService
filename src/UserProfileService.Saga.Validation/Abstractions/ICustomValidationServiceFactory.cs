using System;

namespace UserProfileService.Saga.Validation.Abstractions;

/// <summary>
///     Interface for a factory that creates instances of <see cref="ICustomValidationService" />.
/// </summary>
public interface ICustomValidationServiceFactory
{
    /// <summary>
    ///     Creates an instance of <see cref="ICustomValidationService" /> based on the type of the message.
    /// </summary>
    /// <param name="message">The message for which to create the custom validation service.</param>
    /// <returns>An instance of <see cref="ICustomValidationService" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message" /> is null.</exception>
    /// <remarks>
    ///     This factory method creates an appropriate implementation of <see cref="ICustomValidationService" />
    ///     based on the type of the provided message. Implementations should handle custom validation logic
    ///     specific to each message type.
    /// </remarks>
    ICustomValidationService CreateCustomValidationService<TMessage>(TMessage message);
}
