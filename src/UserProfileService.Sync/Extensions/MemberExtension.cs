using System.Linq;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using ResolvedMember = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="Member" />s.
/// </summary>
public static class MemberExtension
{
    /// <summary>
    ///     Return the <see cref="KeyProperties" /> of the given <see cref="Member" />.
    /// </summary>
    /// <param name="member">Member the key properties returned for.</param>
    /// <returns><see cref="KeyProperties" /> of given member.</returns>
    public static KeyProperties GetKeyProperties(this Member member)
    {
        return new KeyProperties(member.ExternalIds.FirstOrDefaultUnconverted()?.Id, string.Empty);
    }

    /// <summary>
    ///     Return the <see cref="KeyProperties" /> of the given <see cref="Member" />.
    /// </summary>
    /// <param name="member">Member the key properties returned for.</param>
    /// <returns><see cref="KeyProperties" /> of given member.</returns>
    public static KeyProperties GetKeyProperties(this ResolvedMember member)
    {
        return new KeyProperties(
            member?.ExternalIds?.FirstOrDefault()?.Id,
            member?.ExternalIds?.FirstOrDefault()?.Source);
    }
}
