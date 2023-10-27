using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Projection.Common.Abstractions;

/// <summary>
///     Service to process events in the outbox table.
/// </summary>
internal interface IOutboxProcessorService
{
    /// <summary>
    ///     Check the outbox table for new processable events.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns></returns>
    public Task CheckAndProcessEvents(CancellationToken cancellationToken = default);
}
