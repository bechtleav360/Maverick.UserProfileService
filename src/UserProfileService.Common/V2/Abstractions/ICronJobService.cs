using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Defines a service that will execute a task in a time triggered manner.
/// </summary>
public interface ICronJobService
{
    /// <summary>
    ///     Executes the time-triggered task.
    /// </summary>
    /// <param name="token">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns></returns>
    Task ExecuteAsync(CancellationToken token = default);
}
