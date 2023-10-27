using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Query;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     An interface for interacting with ArangoDB Cursors endpoints.
/// </summary>
public interface IAQuery
{
    /// <summary>
    ///     Execute an AQL query, creating a cursor which can be used to page query results.
    /// </summary>
    /// <typeparam name="T">Generic type of the elements fetched by the cursor.</typeparam>
    /// <param name="body">Object encapsulating options and parameters of the query.</param>
    /// <param name="transactionId">>Transaction id (only if query has to be executed inside a stream transaction).</param>
    /// <param name="timeout">
    ///     sets the timespan to wait before the request times out (default: null, that means default value
    ///     of HTTP clients will be taken.)
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     Object containing information about the created cursor or possibly occurred errors
    ///     <see cref="CursorResponse{T}" />.
    /// </returns>
    Task<CursorResponse<T>> CreateCursorAsync<T>(
        CreateCursorBody body,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Execute an AQL query, creating a cursor which can be used to page query results.
    /// </summary>
    /// <typeparam name="T">Generic type of the elements fetched by the cursor.</typeparam>
    /// <param name="query"> Contains the query string to be executed</param>
    /// <param name="bindVars">Key/value pairs representing the bind parameters.</param>
    /// <param name="options">Object containing some Parameters that can be set when creating a cursor.</param>
    /// <param name="transactionId">>Transaction id (only if query has to be executed inside a stream transaction).</param>
    /// <param name="timeout">
    ///     sets the timespan to wait before the request times out (default: null, that means default value
    ///     of HTTP clients will be taken.)
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     Object containing information about the created cursor or possibly occurred errors
    ///     <see cref="CursorResponse{T}" />.
    /// </returns>
    Task<CursorResponse<T>> CreateCursorAsync<T>(
        string query,
        Dictionary<string, object> bindVars = null,
        PostCursorOptions options = null,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Advances an existing query cursor and gets the next set of results.
    /// </summary>
    /// <typeparam name="T">Generic type of the elements fetched by the cursor.</typeparam>
    /// <param name="cursorId">Cursor Id.</param>
    /// <param name="timeout">
    ///     sets the timespan to wait before the request times out (default: null, that means default value
    ///     of HTTP clients will be taken.)
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     Object Containing attributes of the next batch from cursor <see cref="PutCursorResponse{T}" /> or possibly
    ///     occurred errors.
    /// </returns>
    Task<PutCursorResponse<T>> PutCursorAsync<T>(
        string cursorId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes specified AQL query cursor.
    /// </summary>
    /// <param name="cursorId">Cursor Id.</param>
    /// <param name="timeout">
    ///     sets the timespan to wait before the request times out (default: null, that means default value
    ///     of HTTP clients will be taken.)
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     Object Containing information about the deleted cursor <see cref="DeleteCursorResponse" /> or possibly
    ///     occurred errors..
    /// </returns>
    Task<DeleteCursorResponse> DeleteCursorAsync(
        string cursorId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Parse an AQL query (only for query validation)
    /// </summary>
    /// <param name="query">The AQL query</param>
    /// <returns>Object containing an object with the attributes of the parsed query <see cref="ParseQueryResponse" />.</returns>
    Task<ParseQueryResponse> ParseAsync(string query);

    /// <summary>
    ///     Method to execute a simple AQl Query.
    /// </summary>
    /// <typeparam name="T">Generic type of the request results</typeparam>
    /// <param name="query">The AQL Query as string</param>
    /// <param name="transactionId">transaction id (only if query has to be executed inside a stream transaction).</param>
    /// <param name="timeout">
    ///     sets the timespan to wait before the request times out (default: null, that means default value
    ///     of HTTP clients will be taken.)
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     Object contains the request result and some debug information <see cref="MultiApiResponse{T}" /> or possibly
    ///     occurred errors.
    /// </returns>
    public Task<MultiApiResponse<T>> ExecuteQueryAsync<T>(
        string query,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Method to execute an AQL Query with some options
    /// </summary>
    /// <typeparam name="T">Generic type of the request results.</typeparam>
    /// <param name="cursorBody">Object encapsulating options and parameters of the query.</param>
    /// <param name="transactionId">transaction id (only if query has to be executed inside a stream transaction).</param>
    /// <param name="timeout">
    ///     sets the timespan to wait before the request times out (default: null, that means default value
    ///     of HTTP clients will be taken.)
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     Object contains the request result and some debug information <see cref="MultiApiResponse{T}" /> or possibly
    ///     occurred errors.
    /// </returns>
    public Task<MultiApiResponse<T>> ExecuteQueryWithCursorOptionsAsync<T>(
        CreateCursorBody cursorBody,
        string transactionId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}
