using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Results;

namespace UserProfileService.Sync.Models;

/// <summary>
///     Implementation of <see cref="IBatchResult{T}" />
/// </summary>
/// <typeparam name="T">
///     The generic parameter that the result is type of. The generic type
///     must be type of <see cref="ISyncModel" />.
/// </typeparam>
public class BatchResult<T> : IBatchResult<T> where T : ISyncModel
{
    /// <summary>
    ///     The batch size that was returned (current-start).
    /// </summary>
    public int BatchSize { set; get; }

    /// <summary>
    ///     The current position of the batch that is returned.
    /// </summary>
    public int CurrentPosition { set; get; }

    /// <summary>
    ///     Contains an error message when some error occurred.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public string ErrorMessage { set; get; }

    /// <summary>
    ///     If a next batch of results is available.
    /// </summary>
    public bool NextBatch { set; get; }

    /// <summary>
    ///     The Result that the source system returns.
    /// </summary>
    public IList<T> Result { set; get; }

    /// <summary>
    ///     The start position of the batch that is returned.
    /// </summary>
    public int StartedPosition { set; get; }

    /// <summary>
    ///     Create an instance of <see cref="BatchResult{T}" />
    /// </summary>
    /// <param name="result">
    ///     <see cref="Result" />
    /// </param>
    /// <param name="startedPosition">
    ///     <see cref="StartedPosition" />
    /// </param>
    /// <param name="currentPosition">
    ///     <see cref="CurrentPosition" />
    /// </param>
    /// <param name="batchSize">
    ///     <see cref="BatchSize" />
    /// </param>
    /// <param name="nextBatch">
    ///     <see cref="NextBatch" />
    /// </param>
    public BatchResult(IList<T> result, int startedPosition, int currentPosition, int batchSize, bool nextBatch)
    {
        Result = result;
        StartedPosition = startedPosition;
        CurrentPosition = currentPosition;
        BatchSize = batchSize;
        NextBatch = nextBatch;
    }

    /// <summary>
    ///     Create an instance of <see cref="BatchResult{T}" />
    /// </summary>
    public BatchResult()
    {
    }
}
