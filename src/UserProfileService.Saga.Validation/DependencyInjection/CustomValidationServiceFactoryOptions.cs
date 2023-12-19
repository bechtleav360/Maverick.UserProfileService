using System;
using System.Collections.Generic;
using UserProfileService.Saga.Validation.Abstractions;

namespace UserProfileService.Saga.Validation.DependencyInjection;

/// <summary>
/// Options class for configuring the behavior of <see cref="ICustomValidationServiceFactory"/>.
/// </summary>
public class CustomValidationServiceFactoryOptions
{
    /// <summary>
    ///     Gets or sets the mapping from message types to custom validation service types.
    /// </summary>
    /// <remarks>
    ///     Each entry in the dictionary represents a mapping from a message type to the corresponding custom validation
    ///     service type.
    /// </remarks>
    public Dictionary<Type, Type> MessageTypeToValidationServiceMap { get; set; } = new Dictionary<Type, Type>();
}
