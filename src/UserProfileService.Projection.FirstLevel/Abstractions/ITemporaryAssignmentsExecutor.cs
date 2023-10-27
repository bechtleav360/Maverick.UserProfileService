using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.FirstLevel.Abstractions;

internal interface ITemporaryAssignmentsExecutor
{
    /// <summary>
    ///     Checks if assignments have to be activated or deactivated. If it's the case, it will create desired second level
    ///     events and pass them to <see cref="ISagaService" />.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read/write operation.</returns>
    Task CheckTemporaryAssignmentsAsync(CancellationToken cancellationToken = default);
}
