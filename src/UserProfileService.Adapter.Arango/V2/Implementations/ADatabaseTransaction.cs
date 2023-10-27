using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     An Implementation of <see cref="IDatabaseTransaction" /> for the ArangoDB.
/// </summary>
public class ADatabaseTransaction : IDatabaseTransaction
{
    /// <inheritdoc />
    public CallingServiceContext CallingService { get; set; }

    /// <summary>
    ///     The Id of the transaction.
    /// </summary>
    public string TransactionId { get; set; }
}
