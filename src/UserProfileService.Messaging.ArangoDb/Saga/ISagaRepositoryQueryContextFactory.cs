using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;

namespace UserProfileService.Messaging.ArangoDb.Saga;

/// <summary>
///     Describes a factory to execute query through <see cref="ISagaRepositoryQueryContext{TSaga}" />
/// </summary>
/// <typeparam name="TSaga">Type of the managed entity.</typeparam>
public interface ISagaRepositoryQueryContextFactory<TSaga> : ISagaRepositoryContextFactory<TSaga>
    where TSaga : class, ISaga
{
    /// <summary>
    ///     Create a <see cref="ISagaRepositoryQueryContext{TSaga}" /> and send it to the next pipe.
    /// </summary>
    /// <param name="asyncMethod">Method to execute through context.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <typeparam name="T">Type of response.</typeparam>
    /// <returns>A <see cref="Task{T}" /></returns>
    Task<T> ExecuteQuery<T>(
        Func<ISagaRepositoryQueryContext<TSaga>, Task<T>> asyncMethod,
        CancellationToken cancellationToken = default)
        where T : class;
}
