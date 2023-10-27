using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.TicketStore.Abstractions;

/// <summary>
///     Represents a type used to store tickets used to manage asynchronous requests/responses.
/// </summary>
public interface ITicketStore
{
    /// <summary>
    ///     Adds or updates a ticket entry that will contain the information about the current progress of an asynchronous
    ///     permission operation.
    /// </summary>
    /// <param name="entry">The entry to be added or updated.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous write operation. It wraps the new created instance of
    ///     <see cref="TicketBase" />.
    /// </returns>
    Task<TicketBase> AddOrUpdateEntryAsync(TicketBase entry, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a ticket entry
    /// </summary>
    /// <param name="id">The ticket id</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The ticket entry</returns>
    Task<TicketBase> GetTicketAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a set of <see cref="TicketBase" />s. An optional filter can be used.
    /// </summary>
    /// <param name="filter">
    ///     A function to test each object for a condition. It will be used to filter set of tickets to be
    ///     deleted. If it is not set, everything will be deleted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous write operation. It contains the number of deleted
    ///     entries.
    /// </returns>
    Task<int> DeleteTicketsAsync(
        Expression<Func<TicketBase, bool>> filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a collection of <see cref="TicketBase" />s. An optional filter can be used.
    /// </summary>
    /// <param name="filter">
    ///     A function to test each object for a condition. It will be used to filter returned result set. If
    ///     it is not set, everything will be returned.
    /// </param>
    /// <param name="pageSize">Amount of entries per page of the result set.</param>
    /// <param name="page">Zero-based page number of the page to be returned.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous read operation. It wraps a collection of
    ///     <see cref="TicketBase" />s.
    /// </returns>
    Task<IList<TicketBase>> GetTicketsAsync(
        Expression<Func<TicketBase, bool>> filter = null,
        int page = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
