using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Contains methods to retrieve data from volatile data sets.
/// </summary>
public interface IVolatileDataReadStore : IUserStore
{
    /// <summary>
    ///     Checks whether the <see cref="UserSettingObject" /> with the id <paramref name="objectId" /> of the user with id
    ///     equals
    ///     <paramref name="userId" /> that is contained by a section named <paramref name="sectionName" /> exists.
    /// </summary>
    /// <remarks>
    ///     An <see cref="UserSettingObject" /> is identified by the id of it's related user, the name of the section, that
    ///     contains the user settings object and it's own id.
    /// </remarks>
    /// <param name="userId">The id of the user whose <see cref="UserSettingObject" /> shall be returned.</param>
    /// <param name="sectionName">
    ///     The name of the <see cref="UserSettingSection" /> that contains the requested
    ///     <see cref="UserSettingObject" />.
    /// </param>
    /// <param name="objectId">The id of the requested <see cref="UserSettingObject" />.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. The value of the <c>TResult</c> parameter contains a
    ///     boolean value whether the <see cref="UserSettingObject" /> exists or not. It is <c>true</c>, it it exists.
    /// </returns>
    Task<bool> CheckUserSettingObjectExistsAsync(
        string userId,
        string sectionName,
        string objectId,
        CancellationToken cancellationToken);
}
