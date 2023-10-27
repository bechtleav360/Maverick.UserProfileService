using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     The base container profile interface for entities like <see cref="GroupBasic" /> or
    ///     <see cref="OrganizationBasic" />.
    /// </summary>
    public interface IContainerProfile : IProfile
    {
        /// <summary>
        ///     If true, the organization is system-relevant, that means it will be treated as read-only.
        /// </summary>
        bool IsSystem { set; get; }
    }
}
