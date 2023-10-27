using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     Defines an object as a result of a started transaction.
/// </summary>
public interface IDatabaseTransaction
{
    /// <summary>
    ///     Contains information about the calling instance.
    /// </summary>
    public CallingServiceContext CallingService { get; }
}
