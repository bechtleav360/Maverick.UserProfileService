using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Models.Results;

/// <summary>
///     The result for a batch returned from the source system.
///     It stores the result and extra properties so where
///     the start and current position from the batch is.
///     Additionally it is known if a next batch is available.
/// </summary>
/// <typeparam name="T">
///     The generic parameter that the result is type of. The generic type
///     must be type of <see cref="ISyncModel" />.
/// </typeparam>
public interface IBatchResult<T> where T : ISyncModel

{
    /// <summary>
    ///     The batch size that was returned (current-start).
    /// </summary>
    int BatchSize { set; get; }

    /// <summary>
    ///     The current position of the batch that is returned.
    /// </summary>
    int CurrentPosition { set; get; }

    /// <summary>
    ///     If a next batch of results is available.
    /// </summary>
    bool NextBatch { set; get; }

    /// <summary>
    ///     The Result that the source system returns.
    /// </summary>
    IList<T> Result { set; get; }

    /// <summary>
    ///     The start position of the batch that is returned.
    /// </summary>
    int StartedPosition { set; get; }
}
