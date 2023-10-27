using System;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     Handles the entire synchronization process.
/// </summary>
/// <typeparam name="TSyncEntity"> The sync entity that will be synchronized. </typeparam>
public interface ISagaEntityProcessor<TSyncEntity> where TSyncEntity : class, ISyncModel
{
    /// <summary>
    ///     Handles the entities that should be synchronized.
    /// </summary>
    /// <param name="syncProcess">Contains the sync states that the sync is into. </param>
    /// <param name="correlationId"> The correlation id. </param>
    /// <param name="saveAction">
    ///     Action that can be executed to write the current status to the database. During the process in
    ///     order to keep the data as up-to-date as possible.
    /// </param>
    /// <param name="ctx">Propagates notification that operations should be canceled.</param>
    /// <returns></returns>
    public Task HandleEntitySync(
        Process syncProcess,
        string correlationId,
        Func<Process, Task> saveAction = null,
        CancellationToken ctx = default);
}
