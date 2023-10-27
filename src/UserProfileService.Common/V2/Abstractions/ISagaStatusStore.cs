using System.Threading.Tasks;
using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     The saga store is for tracing the saga status.
///     Everyone can see in which  state his saga is.
/// </summary>
public interface ISagaStatusStore
{
    /// <summary>
    ///     Gets the current state of the sage.
    /// </summary>
    /// <param name="sagaId">The saga id for getting the saga stage.</param>
    /// <param name="state">The explicit state of the saga (optional).</param>
    /// <returns>The current state of the saga <see cref="SagaStatus" />.</returns>
    Task<SagaStatus> GetSagaStatusBySagaId(string sagaId, SagaState? state = null);

    /// <summary>
    ///     Saves the saga status in the status message.
    /// </summary>
    /// <param name="sagaStatus">The saga status message.</param>
    /// <returns>Nothing.</returns>
    Task SaveSagaStatus(SagaStatus sagaStatus);
}
