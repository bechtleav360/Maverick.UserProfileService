using System.Threading.Tasks;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     Represents a store that contains information about the current user.
    /// </summary>
    public interface IUserContextStore
    {
        /// <summary>
        ///     Gets the id of the current user.
        /// </summary>
        /// <returns>A task representing the asynchronous read operation. It wraps a user id as a string value.</returns>
        Task<string> GetIdOfCurrentUserAsync();
    }
}
