using System;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Validation.Abstractions.Message;

/// <summary>
///     Response of validation.
/// </summary>
[Message(ServiceName = "event-collector", ServiceGroup = "user-profile", Version = "v1")]
public class ValidationResponse : ValidationResult, IEventCollectorMessage
{
    /// <inheritdoc cref="IEventCollectorMessage" />
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global => The set modifier will be used.
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     Indicates whether an error occurred during the validation.
    /// </summary>
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set method is needed.
    public bool HasErrorOccurred { get; set; }

    /// <inheritdoc cref="ValidationResult" />
    public override bool IsValid { get; }

    /// <summary>
    ///     Create an instance of <see cref="ValidationResponse" />.
    /// </summary>
    /// <param name="isValid">Whether validation succeeded.</param>
    /// <param name="hasErrorOccurred">Indicates whether an error occurred during the validation.</param>
    public ValidationResponse(bool isValid, bool hasErrorOccurred = false)
    {
        IsValid = isValid;
        HasErrorOccurred = hasErrorOccurred;
    }
}
