using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Validation.Abstractions;

/// <summary>
///     Describes a service that serves as an abstraction between the internal services of the package and external users.
/// </summary>
public interface IValidationService
{
    /// <summary>
    ///     Validates the given content using various validators.
    /// </summary>
    /// <typeparam name="TPayload">Type of payload to validate.</typeparam>
    /// <param name="payload">Payload to validate.</param>
    /// <param name="initiator">Initiator of the command to validate.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <exception cref="ValidationException">Exception thrown when validation fails.</exception>
    /// <returns>Task that wraps the async operation.</returns>
    public Task ValidatePayloadAsync<TPayload>(
        TPayload payload,
        Initiator initiator = null,
        CancellationToken cancellationToken = default);
}
