using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Validation.Abstractions.Message;

/// <summary>
///     Message to aggregate response message.
/// </summary>
[Message(Version = "v1")]
public class ValidationCompositeResponse : ValidationResult, IEventCollectorMessage
{
    /// <inheritdoc cref="IEventCollectorMessage" />
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     Indicates whether an error occurred during the merging of the validation results.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => get modifier will be used.
    public bool HasErrorOccurred { get; set; }

    /// <summary>
    ///     Whether validation succeeded
    /// </summary>
    public override bool IsValid { get; }

    /// <summary>
    ///     Creates a new <see cref="ValidationCompositeResponse" />.
    /// </summary>
    [JsonConstructor]
    public ValidationCompositeResponse(bool isValid)
    {
        IsValid = isValid;
    }

    /// <summary>
    ///     Creates a new <see cref="ValidationCompositeResponse" /> from a collection of failures.
    /// </summary>
    /// <param name="collectingId">Id of collecting validation process.</param>
    /// <param name="isValid">Indicates whether the result is valid regardless of <see cref="ValidationResult.Errors" />.</param>
    /// <param name="failures">
    ///     List of <see cref="ValidationAttribute" /> which is later available through <see cref="ValidationResult.Errors" />.
    ///     This list get's copied.
    /// </param>
    /// <remarks>
    ///     Every caller is responsible for not adding <c>null</c> to the list.
    /// </remarks>
    public ValidationCompositeResponse(
        Guid collectingId,
        bool isValid,
        IEnumerable<ValidationAttribute> failures) : base(failures)
    {
        CollectingId = collectingId;
        IsValid = isValid;
    }

    /// <summary>
    ///     Creates a new <see cref="ValidationCompositeResponse" /> from a collection of failures.
    /// </summary>
    /// <param name="collectingId">Id of related validation process.</param>
    /// <param name="isValid">Indicates whether the result is valid regardless of <see cref="ValidationResult.Errors" />.</param>
    /// <param name="hasErrorOccurred">Error occurred while validation composition.</param>
    public ValidationCompositeResponse(Guid collectingId, bool isValid, bool hasErrorOccurred)
    {
        CollectingId = collectingId;
        IsValid = isValid;
        HasErrorOccurred = hasErrorOccurred;
    }
}
