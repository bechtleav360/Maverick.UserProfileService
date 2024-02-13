using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Context;
using MassTransit.Logging;
using MassTransit.Middleware;
using MassTransit.Saga;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration

namespace UserProfileService.Messaging.ArangoDb.Saga;

/// <summary>
///     Arango implementation of <see cref="ConsumeContextScope{TMessage}" /> and
///     <see cref="SagaRepositoryContext{TSaga,TMessage}" />.
/// </summary>
/// <typeparam name="TSaga">Type of saga of given context.</typeparam>
/// <typeparam name="TMessage">Type of start message of <see cref="SagaRepositoryContext{TSaga,TMessage}" />.</typeparam>
public class ArangoSagaRepositoryContext<TSaga, TMessage> :
    ConsumeContextScope<TMessage>,
    SagaRepositoryContext<TSaga, TMessage>,
    IAsyncDisposable
    where TSaga : class, ISagaVersion
    where TMessage : class
{
    private readonly ConsumeContext<TMessage> _context;
    private readonly IDatabaseContext<TSaga> _dbContext;
    private readonly ISagaConsumeContextFactory<IDatabaseContext<TSaga>, TSaga> _factory;
    private readonly ILogger<ArangoSagaRepositoryContext<TSaga, TMessage>> _logger;

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaRepositoryContext{TSaga,TMessage}" />.
    /// </summary>
    /// <param name="dbContext">Database context for <typeparamref name="TSaga" />.</param>
    /// <param name="context">Consume context for <typeparamref name="TMessage" />.</param>
    /// <param name="factory">Consume context factory to create <see cref="IDatabaseContext{TSaga}" />.</param>
    /// <param name="loggerFactory">Factory to create logger.</param>
    public ArangoSagaRepositoryContext(
        IDatabaseContext<TSaga> dbContext,
        ConsumeContext<TMessage> context,
        ISagaConsumeContextFactory<IDatabaseContext<TSaga>, TSaga> factory,
        ILoggerFactory loggerFactory)
        : base(context)
    {
        _dbContext = dbContext;
        _context = context;
        _factory = factory;
        _logger = loggerFactory.CreateLogger<ArangoSagaRepositoryContext<TSaga, TMessage>>();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _logger.EnterMethod();

        await _dbContext.DisposeAsync();

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<SagaConsumeContext<TSaga, T>> CreateSagaConsumeContext<T>(
        ConsumeContext<T> consumeContext,
        TSaga instance,
        SagaConsumeContextMode mode)
        where T : class
    {
        _logger.EnterMethod();

        if (consumeContext == null)
        {
            throw new ArgumentNullException(nameof(consumeContext));
        }

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        SagaConsumeContext<TSaga, T> responseContext =
            await _factory.CreateSagaConsumeContext(_dbContext, consumeContext, instance, mode);

        return _logger.ExitMethod(responseContext);
    }

    /// <inheritdoc />
    public async Task<SagaConsumeContext<TSaga, TMessage>> Add(TSaga instance)
    {
        _logger.EnterMethod();

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        SagaConsumeContext<TSaga, TMessage> responseContext = await _factory.CreateSagaConsumeContext(
            _dbContext,
            _context,
            instance,
            SagaConsumeContextMode.Add);

        return _logger.ExitMethod(responseContext);
    }

    /// <inheritdoc />
    public async Task<SagaConsumeContext<TSaga, TMessage>> Insert(TSaga instance)
    {
        _logger.EnterMethod();

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        try
        {
            await _dbContext.InsertAsync(instance).ConfigureAwait(false);

            _context.LogInsert<TSaga, TMessage>(instance.CorrelationId);

            return await _factory.CreateSagaConsumeContext(
                    _dbContext,
                    _context,
                    instance,
                    SagaConsumeContextMode.Insert)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _context.LogInsertFault<TSaga, TMessage>(ex, instance.CorrelationId);

            return default;
        }
        finally
        {
            _logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public async Task<SagaConsumeContext<TSaga, TMessage>> Load(Guid correlationId)
    {
        _logger.EnterMethod();

        if (correlationId == Guid.Empty)
        {
            _logger.LogWarnMessage(
                "Guid can not be empty (TSaga = {TSaga}; TMessage = {TMessage})",
                LogHelpers.Arguments(
                    typeof(TSaga).FullName,
                    typeof(TMessage).Name));

            return _logger.ExitMethod<SagaConsumeContext<TSaga, TMessage>>(default);
        }

        TSaga instance = await _dbContext.LoadAsync(correlationId).ConfigureAwait(false);

        if (instance == null)
        {
            return _logger.ExitMethod<SagaConsumeContext<TSaga, TMessage>>(default);
        }

        SagaConsumeContext<TSaga, TMessage> result = await _factory
            .CreateSagaConsumeContext(
                _dbContext,
                _context,
                instance,
                SagaConsumeContextMode.Load)
            .ConfigureAwait(false);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task Save(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _dbContext.AddAsync(context);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task Update(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _dbContext.UpdateAsync(context);

        _logger.EnterMethod();
    }

    /// <inheritdoc />
    public async Task Delete(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // sometimes the sage will deleted twice
        // we don't know why
        // to avoid too much warning/error logging we decided to do it this way
        bool deleted = await _dbContext.TryDeleteAsync(context);

        if (!deleted)
        {
            _logger.LogDebugMessage("Could not delete saga instance [id = {correlationId}], because it does not exist any more.",
                LogHelpers.Arguments(context.CorrelationId));
        }

        _logger.EnterMethod();
    }

    /// <inheritdoc />
    public Task Discard(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return _logger.ExitMethod(Task.CompletedTask);
    }

    /// <inheritdoc />
    public Task Undo(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return _logger.ExitMethod(Task.CompletedTask);
    }
}

/// <summary>
///     Arango implementation of <see cref="SagaRepositoryContext{TSaga,TMessage}" /> and <see cref="BasePipeContext" />.
/// </summary>
/// <typeparam name="TSaga">Type of saga.</typeparam>
public class ArangoSagaRepositoryContext<TSaga> :
    BasePipeContext,
    ISagaRepositoryQueryContext<TSaga>,
    IAsyncDisposable
    where TSaga : class, ISagaVersion
{
    private readonly IDatabaseContext<TSaga> _context;
    private readonly ILogger<ArangoSagaRepositoryContext<TSaga>> _logger;

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaRepositoryContext{TSaga}" />.
    /// </summary>
    /// <param name="context">Database context for <typeparamref name="TSaga" />.</param>
    /// <param name="loggerFactory">Create factory for logger.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    public ArangoSagaRepositoryContext(
        IDatabaseContext<TSaga> context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
        : base(cancellationToken)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger<ArangoSagaRepositoryContext<TSaga>>();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _logger.EnterMethod();

        await _context.DisposeAsync();

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public Task<SagaRepositoryQueryContext<TSaga>> Query(
        ISagaQuery<TSaga> query,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedByDesignException("Arango saga repository does not support queries");
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TSaga instance, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        await _context.UpdateAsync(instance, cancellationToken);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<TSaga> Load(Guid correlationId)
    {
        _logger.EnterMethod();

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("Guid can not be empty.", nameof(correlationId));
        }

        TSaga saga = await _context.LoadAsync(correlationId, CancellationToken);

        return _logger.ExitMethod(saga);
    }

    /// <inheritdoc />
    public async Task<Tuple<int, IList<TSaga>>> QueryAsync(
        int limit,
        int offset,
        Expression<Func<TSaga, object>> sortExpression = null,
        Expression<Predicate<TSaga>> filterExpression = null,
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        (int count, IList<TSaga> results) result = await _context.LoadAsync(
            limit,
            offset,
            sortExpression,
            filterExpression,
            sortOrder,
            CancellationToken);

        return _logger.ExitMethod(new Tuple<int, IList<TSaga>>(result.count, result.results));
    }
}
