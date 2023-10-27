using Npgsql;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Marten.Helpers;

internal class MartenAdapterDatabaseTransaction : IDatabaseTransaction, IDisposable, IAsyncDisposable
{
    public CallingServiceContext? CallingService { get; set; }

    public NpgsqlTransaction CurrentTransaction { get; }

    public MartenAdapterDatabaseTransaction(NpgsqlTransaction currentTransaction)
    {
        CurrentTransaction = currentTransaction;
    }

    public void Dispose()
    {
        // if the connection is not closed the projection getting after
        // a while an error "53300: sorry, too many clients already"
        // So the conclusion for now is: the connection while disposing the
        // transaction object is not close! This should be evaluated in the future.
        CurrentTransaction.Connection?.Close();
        CurrentTransaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        // if the connection is not closed the projection getting after
        // a while an error "53300: sorry, too many clients already"
        // So the conclusion for now is: the connection while disposing the
        // transaction object is not close! This should be evaluated in the future.
        await CurrentTransaction.Connection?.CloseAsync()!;
        await CurrentTransaction.DisposeAsync();
    }
}
