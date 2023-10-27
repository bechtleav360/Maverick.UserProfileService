using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Transaction;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     An interface for interacting with ArangoDB Transactions endpoints.
/// </summary>
public interface IATransaction
{
    /// <summary>
    ///     Get the status of the specified transaction.
    /// </summary>
    /// <param name="transactionId">Transaction Id.</param>
    /// <returns>
    ///     Object containing the transaction status and some debug information or possibly occurred errors
    ///     <see cref="GetTransactionStatusResponse" />.
    /// </returns>
    Task<GetTransactionStatusResponse> GetTransactionStatusAsync(string transactionId);

    /// <summary>
    ///     Get currently running transactions
    /// </summary>
    /// <returns>
    ///     Object containing a list of all running transactions and some debug information or possibly occurred errors
    ///     <see cref="GetAllRunningTransactionsResponse" />.
    /// </returns>
    Task<GetAllRunningTransactionsResponse> GetAllRunningTransactionsAsync();

    /// <summary>
    ///     Begin a stream transaction
    /// </summary>
    /// <param name="writeCollections">
    ///     Collections that will be written to in the
    ///     transaction
    /// </param>
    /// <param name="readCollections">
    ///     Collections that will be only read to in the
    ///     transaction
    /// </param>
    /// <param name="options">Contains options that can be set by starting a transaction.</param>
    /// <returns>
    ///     Object containing information about the transaction and some debug information or possibly occurred errors
    ///     <see cref="TransactionOperationResponse" />.
    /// </returns>
    Task<TransactionOperationResponse> BeginTransactionAsync(
        IEnumerable<string> writeCollections,
        IEnumerable<string> readCollections,
        TransactionOptions options = null);

    /// <summary>
    ///     Commits the transaction with the given transaction ID.
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns>
    ///     Object containing information about the transaction and some debug informations or possibly occurred errors
    ///     <see cref="TransactionOperationResponse" />.
    /// </returns>
    Task<TransactionOperationResponse> CommitTransactionAsync(string transactionId);

    /// <summary>
    ///     Abort a running server-side transaction corresponding to the given transaction ID. Aborting is an idempotent
    ///     operation.
    /// </summary>
    /// <param name="transactionId">Transaction ID.</param>
    /// <returns>
    ///     Object containing information about the transaction and some debug information or possibly occurred errors
    ///     <see cref="TransactionOperationResponse" />
    /// </returns>
    Task<TransactionOperationResponse> AbortTransactionAsync(string transactionId);

    /// <summary>
    ///     Executes JavaScript transaction.
    /// </summary>
    /// <typeparam name="T">Return type of the transaction function.</typeparam>
    /// <param name="action">
    ///     The actual transaction operations to be executed, in the
    ///     form of stringified JavaScript code
    /// </param>
    /// <returns>
    ///     Object containing the return value(s) of the transaction and some debug information
    ///     <see cref="ExecuteJsTransactionResponse{T}" />
    /// </returns>
    Task<ExecuteJsTransactionResponse<T>> ExecuteJsTransactionAsync<T>(string action);

    /// <summary>
    ///     Executes JavaScript transaction.
    /// </summary>
    /// <typeparam name="T">Return type of the transaction function.</typeparam>
    /// <param name="action">
    ///     The actual transaction operations to be executed, in the
    ///     form of stringified JavaScript code
    /// </param>
    /// <param name="writeCollections">Collections that will be written to in the transaction.</param>
    /// <param name="readCollections">Collections that will be only read to in the transaction.</param>
    /// <param name="options">
    ///     Contains options that can be set by executing a JavaScript transaction
    ///     <see cref="JsTransactionOptions" />
    /// </param>
    /// <returns>
    ///     Object containing the return value(s) of the transaction and some debug information
    ///     <see cref="ExecuteJsTransactionResponse{T}" />
    /// </returns>
    Task<ExecuteJsTransactionResponse<T>> ExecuteJsTransactionAsync<T>(
        string action,
        IEnumerable<string> writeCollections,
        IEnumerable<string> readCollections,
        JsTransactionOptions options = null);
}
