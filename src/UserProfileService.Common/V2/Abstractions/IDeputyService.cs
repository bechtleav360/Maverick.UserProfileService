using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     The interface defines deputy related operations.
/// </summary>
public interface IDeputyService
{
    /// <summary>
    ///     Get the deputy profile for the given profile id.
    /// </summary>
    /// <param name="profileId">An unique id of the profile to get the deputy for.</param>
    /// <param name="expectedKind">The profile kind of each deputy profile to be retrieved.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the requested <typeparamref name="TProfile" />.</returns>
    public Task<TProfile> GetDeputyOfProfileAsync<TProfile>(
        string profileId,
        RequestedProfileKind expectedKind,
        CancellationToken cancellationToken = default) where TProfile : IProfile;
}
