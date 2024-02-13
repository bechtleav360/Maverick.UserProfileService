using System;
using System.Net;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing informations about the
///     (commited, aborted or beginned)  transaction <see cref="TransactionEntity" />
/// </summary>
/// <inheritdoc cref="SingleApiResponse" />
public class TransactionOperationResponse : SingleApiResponse<TransactionEntity>, IRunningTransaction
{
    private Connection _connection;

    /// <inheritdoc />
    Connection IRunningTransaction.UsedConnection => _connection;

    internal TransactionOperationResponse(
        Response response,
        TransactionEntity transaction,
        Connection usedConnection) : base(response, transaction)
    {
        if (usedConnection == null)
        {
            throw new ArgumentNullException(nameof(usedConnection));
        }

        _connection = usedConnection.Clone();
    }

    internal TransactionOperationResponse(Response response, Exception exception) : base(response, exception)
    {
    }

    /// <summary>
    ///     Disposes of resources asynchronously.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_connection != null)
        {
            var transaction = new ATransaction(_connection);

            TransactionOperationResponse response =
                await transaction.CommitTransactionAsync(GetTransactionId()).ConfigureAwait(false);

            if (response.Error
                && response.Code
                != HttpStatusCode
                    .NotFound) // NOT_FOUND means that the transaction has been already deleted (maybe timeout) => no exception, just ignore
            {
                throw response.Exception;
            }
        }

        _connection = null;
    }

    /// <summary>
    ///     Disposes of the <see cref="Connection"/> used by this object.
    /// </summary>
    /// <param name="disposing">True if called from the <see cref="IDisposable.Dispose"/> method, false if called from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        _connection = null;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IRunningTransaction" />
    public TransactionStatus GetTransactionStatus()
    {
        return Result?.Status ?? TransactionStatus.Unknown;
    }

    /// <inheritdoc cref="IRunningTransaction" />
    public string GetTransactionId()
    {
        return Result?.Id;
    }
}
