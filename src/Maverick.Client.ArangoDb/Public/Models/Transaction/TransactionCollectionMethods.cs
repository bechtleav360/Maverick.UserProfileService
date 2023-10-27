using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Collection;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains methods that can be run on an ArangoDB collection within a transaction.
/// </summary>
public class TransactionCollectionMethods : TransactionMethods
{
    internal TransactionCollectionMethods(IRunningTransaction transaction) : base(transaction)
    {
    }

    /// <summary>
    ///     Retrieves basic information with additional properties and document count in specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing information about the
    ///     number of documents in the specified collection or possibly occurred errors
    ///     <see cref="GetCollectionCountResponse" />.
    /// </returns>
    public Task<GetCollectionCountResponse> GetCollectionCountAsync(string collectionName)
    {
        return new ACollection(Transaction.UsedConnection).GetCollectionCountAsync(
            collectionName,
            Transaction.GetTransactionId());
    }

    /// <summary>
    ///     Removes all documents from specified collection.
    /// </summary>
    /// <param name="collectionName">Collection name.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an object containing details about the
    ///     truncate operation, like information about possibly occurred errors <see cref="TruncateCollectionResponse" />.
    /// </returns>
    public Task<TruncateCollectionResponse> TruncateCollectionAsync(string collectionName)
    {
        return new ACollection(Transaction.UsedConnection).TruncateCollectionAsync(
            collectionName,
            Transaction.GetTransactionId());
    }
}
