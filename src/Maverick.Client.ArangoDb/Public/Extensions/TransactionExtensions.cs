using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models;
using Maverick.Client.ArangoDb.Public.Models.Transaction;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Contains extension methods for <see cref="IRunningTransaction" /> items.
/// </summary>
public static class TransactionExtensions
{
    /// <summary>
    ///     Returns an object that is used to run transaction methods on specified collections.
    /// </summary>
    /// <param name="transaction">An object containing information about the transaction to be used.</param>
    /// <returns>The wrapping object that will be used to execute operation within the specified transaction.</returns>
    /// <exception cref="TransactionNotRunningException">If the transaction is not valid.</exception>
    public static TransactionCollectionMethods Collections(this IRunningTransaction transaction)
    {
        return new TransactionCollectionMethods(transaction);
    }

    /// <summary>
    ///     Returns an object that is used to run transaction methods on specified collections.
    /// </summary>
    /// <param name="transaction">An object containing information about the transaction to be used.</param>
    /// <returns>The wrapping object that will be used to execute operation within the specified transaction.</returns>
    /// <exception cref="TransactionNotRunningException">If the transaction is not valid.</exception>
    public static TransactionDocumentMethods Documents(this IRunningTransaction transaction)
    {
        return new TransactionDocumentMethods(transaction);
    }

    /// <summary>
    ///     Returns an object that is used to run query methods  n CURSOR api inside an transaction.
    /// </summary>
    /// <param name="transaction">An object containing information about the transaction to be used.</param>
    /// <returns>The wrapping object that will be used to execute operation within the specified transaction.</returns>
    /// <exception cref="TransactionNotRunningException">If the transaction is not valid.</exception>
    public static TransactionQueryMethods Query(this IRunningTransaction transaction)
    {
        return new TransactionQueryMethods(transaction);
    }
}
