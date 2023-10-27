using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.ResponseModels;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Includes all read operations regarding administration of data set.
/// </summary>
public interface IAdminReadService
{
    /// <summary>
    ///     Gets the projection state related to the API service.
    /// </summary>
    /// <param name="streamNames">Names of streams that should be retrieved. If not provided, all streams will be returned.</param>
    /// <param name="paginationSettings">Includes options to set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation that wraps the requested state as a dictionary of state
    ///     entries grouped by stream name.
    /// </returns>
    Task<GroupedProjectionState> GetServiceProjectionStateAsync(
        IList<string> streamNames = null,
        PaginationQueryObject paginationSettings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the projection state related to the first-level-projection.
    /// </summary>
    /// <param name="paginationSettings">Includes options to set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation that wraps the requested state as list.</returns>
    Task<IPaginatedList<ProjectionState>> GetFirstLevelProjectionStateAsync(
        PaginationQueryObject paginationSettings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of statistic entries about the current projection state.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation that wraps the requested entries as list.</returns>
    Task<IList<ProjectionStateStatisticEntry>> GetProjectionStateStatisticAsync(
        CancellationToken cancellationToken = default);
}
