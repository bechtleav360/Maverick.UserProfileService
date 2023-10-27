using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Query;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains methods of the CURSOR API (aka queries) than can be run inside a transaction.
/// </summary>
public class TransactionQueryMethods : TransactionMethods
{
    internal TransactionQueryMethods(IRunningTransaction transaction) : base(transaction)
    {
    }

    /// <summary>
    ///     Method to execute a simple AQl Query.
    /// </summary>
    /// <typeparam name="TResult">Type of the request results.</typeparam>
    /// <param name="query">The AQL query as string.</param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps an object contains the request result and some
    ///     debug information <see cref="MultiApiResponse{T}" /> or possibly occurred errors.
    /// </returns>
    public Task<MultiApiResponse<TResult>> ExecuteQueryAsync<TResult>(string query) where TResult : class
    {
        return new AQuery(Transaction.UsedConnection).ExecuteQueryAsync<TResult>(
            query,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Method to execute a simple AQl Query.
    /// </summary>
    /// <typeparam name="TResultElements">Type of the request results.</typeparam>
    /// <param name="cursorBody">Object encapsulating options and parameters of the query.</param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps an object contains the request result and some
    ///     debug information <see cref="MultiApiResponse{T}" /> or possibly occurred errors.
    /// </returns>
    public Task<MultiApiResponse<TResultElements>> ExecuteQueryAsync<TResultElements>(CreateCursorBody cursorBody)
        where TResultElements : class
    {
        return new AQuery(Transaction.UsedConnection).ExecuteQueryWithCursorOptionsAsync<TResultElements>(
            cursorBody,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Execute an AQL query, creating a cursor which can be used to page query results.
    /// </summary>
    /// <typeparam name="TResultElements">Generic type of the elements fetched by the cursor.</typeparam>
    /// <param name="cursorBody">Object encapsulating options and parameters of the query.</param>
    /// <returns>
    ///     Object containing information about the created cursor or possibly occurred errors
    ///     <see cref="CursorResponse{T}" />.
    /// </returns>
    public Task CreateCursorAsync<TResultElements>(CreateCursorBody cursorBody) where TResultElements : class
    {
        return new AQuery(Transaction.UsedConnection).CreateCursorAsync<TResultElements>(
            cursorBody,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Execute an AQL query, creating a cursor which can be used to page query results.
    /// </summary>
    /// <typeparam name="TResultElements">Generic type of the elements fetched by the cursor.</typeparam>
    /// <param name="query"> Contains the query string to be executed</param>
    /// <param name="bindVars">Key/value pairs representing the bind parameters.</param>
    /// <param name="options">Object containing some Parameters that can be set when creating a cursor.</param>
    /// <returns>
    ///     Object containing information about the created cursor or possibly occurred errors
    ///     <see cref="CursorResponse{T}" />.
    /// </returns>
    public Task<CursorResponse<TResultElements>> CreateCursorAsync<TResultElements>(
        string query,
        Dictionary<string, object> bindVars = null,
        PostCursorOptions options = null) where TResultElements : class
    {
        return new AQuery(Transaction.UsedConnection).CreateCursorAsync<TResultElements>(
            query,
            bindVars,
            options,
            Transaction.GetTransactionId());
    }
}
