using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Common.V2.Implementations;

/// <summary>
///     Represents the default implementation of <see cref="ICursorApiProvider" />.
/// </summary>
public class DefaultCursorApiProvider : ICursorApiProvider
{
    private const string _CacheListPrefix = "UpsCursorApi/List/";

    /// <summary>
    ///     Gets the prefix of each cache store key.
    /// </summary>
    private const string _CachePrefix = "UpsCursorApi/";

    private const int _ExpirationTimeSeconds = 600;
    private readonly ILogger<DefaultCursorApiProvider> _Logger;
    private readonly IJsonSerializerSettingsProvider _SerializerSettingsProvider;
    private readonly ITempStore _Store;

    /// <summary>
    ///     Initializes a new instance of <see cref="DefaultCursorApiProvider" /> with specified <see cref="ICacheStore" /> and
    ///     <see cref="ILogger{TCategoryName}" />.
    /// </summary>
    /// <param name="store">The cache store to be used.</param>
    /// <param name="logger">The logger instance that will accept logging messages of this instance.</param>
    /// <param name="serializerSettingsProvider">Provides settings for json serialization.</param>
    public DefaultCursorApiProvider(
        ITempStore store,
        ILogger<DefaultCursorApiProvider> logger,
        IJsonSerializerSettingsProvider serializerSettingsProvider)
    {
        _Store = store;
        _Logger = logger;
        _SerializerSettingsProvider = serializerSettingsProvider;
    }

    private async Task CleanupSpecifiedCursorAsync(
        CursorState cursor,
        ICacheTransaction transaction,
        CancellationToken token = default)
    {
        if (cursor == null || cursor.HasMore)
        {
            _Logger.LogDebugMessage(
                "No more results found. Deleting result set from cache.",
                LogHelpers.Arguments());

            return;
        }

        try
        {
            await _Store.DeleteAsync(GetCacheId(cursor.Id), transaction, token);
            await _Store.DeleteAsync(GetListCacheId(cursor.Id), transaction, token);
        }
        catch (OperationCanceledException)
        {
            _Logger.LogDebugMessage("Operation cancelled.", LogHelpers.Arguments());
        }
        catch (Exception e)
        {
            _Logger.LogWarnMessage(
                e,
                "Error occurred during cleanup / deletion of cursor {id} from cache store. {message}",
                Arguments(cursor.Id, e.Message));
        }
    }

    private static string GetListCacheId(string cursorId)
    {
        return $"{_CacheListPrefix}{cursorId}";
    }

    private static string GetCacheId(string cursorId)
    {
        return $"{_CachePrefix}{cursorId}";
    }

    private static object[] Arguments(params object[] items)
    {
        return items;
    }

    /// <inheritdoc cref="DefaultCursorApiProvider" />
    public async Task<CursorState<TEntity>> CreateCursorAsync<TService, TEntity, TResult>(
        TService service,
        Func<TService, CancellationToken, Task<TResult>> readMethod,
        int pageSize,
        CancellationToken token = default)
        where TService : IReadService
        where TResult : IList<TEntity>
    {
        _Logger.EnterMethod();

        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (readMethod == null)
        {
            throw new ArgumentNullException(nameof(readMethod));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        TResult response = await readMethod.Invoke(service, token);

        if (response == null || !response.Any())
        {
            _Logger.LogInfoMessage("Could not find any objects using read service.", LogHelpers.Arguments());

            return _Logger.ExitMethod(CursorState<TEntity>.Empty);
        }

        token.ThrowIfCancellationRequested();

        var cursorId = Guid.NewGuid().ToString();

        try
        {
            var cursorResult = new CursorState
            {
                Id = cursorId,
                PageSize = pageSize,
                LastItem = -1,
                ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(_ExpirationTimeSeconds),
                HasMore = response.Count > pageSize,
                TotalAmount = response.Count,
                PayloadType = typeof(TEntity).Name
            };

            if (!cursorResult.HasMore)
            {
                _Logger.LogDebugMessage(
                    "Page size bigger than result sets. Page size {pageSize}. Result amount: {resultSize}",
                    Arguments(pageSize, response.Count));

                return _Logger.ExitMethod(new CursorState<TEntity>(cursorResult, response));
            }

            await using ICacheTransaction transaction = await _Store.CreateTransactionAsync(token);

            await _Store.SetAsync(
                GetCacheId(cursorId),
                cursorResult,
                _ExpirationTimeSeconds,
                transaction,
                _SerializerSettingsProvider,
                token);

            await _Store.AddListAsync(
                GetListCacheId(cursorId),
                response.Skip(pageSize),
                _ExpirationTimeSeconds,
                transaction,
                _SerializerSettingsProvider,
                token);

            _Logger.LogDebugMessage(
                "Stored cursor id {id} in cache store. Page size: {pageSize} elements.",
                Arguments(cursorResult.Id, pageSize));

            if (transaction != null)
            {
                await transaction.CommitAsync(token);
            }

            return _Logger.ExitMethod(
                new CursorState<TEntity>(
                    cursorResult,
                    response.Take(pageSize)));
        }
        catch (OperationCanceledException)
        {
            _Logger.LogDebugMessage(
                "{nameof(CreateCursorAsync)}<{typeof(TEntity).Name}>(): Operation cancelled.",
                LogHelpers.Arguments(nameof(CreateCursorAsync), typeof(TEntity).Name));

            return new CursorState<TEntity>();
        }
    }

    /// <inheritdoc cref="DefaultCursorApiProvider" />
    public async Task<CursorState<TEntity>> GetNextPageAsync<TEntity>(string id, CancellationToken token = default)
    {
        _Logger.EnterMethod();

        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        }

        try
        {
            await using ICacheTransaction transaction = await _Store.CreateTransactionAsync(token);

            var cursor = await _Store.GetAsync<CursorState>(GetCacheId(id), _SerializerSettingsProvider, token);

            if (cursor == null || cursor.PayloadType != typeof(TEntity).Name)
            {
                _Logger.LogDebugMessage(
                    "No cursor object found (cursor id: {id}, entity type: {type}).",
                    Arguments(id, typeof(TEntity).Name));

                throw new InstanceNotFoundException(
                    WellKnownErrorCodes.CursorNotFound,
                    $"Cursor '{id}' could not be found (entity type: '{typeof(TEntity).Name}').");
            }

            IList<TEntity> list = await _Store.GetListAsync<TEntity>(
                GetListCacheId(id),
                cursor.LastItem + 1,
                cursor.PageSize + cursor.LastItem,
                _SerializerSettingsProvider,
                token);

            cursor.AdjustLastItemCount();

            await _Store.SetAsync(
                GetCacheId(cursor.Id),
                cursor,
                _ExpirationTimeSeconds,
                transaction,
                _SerializerSettingsProvider,
                token);

            _Logger.LogDebugMessage(
                "Updated cursor {id} in cache store. Page size: {pageSize} elements. Last item: no. {lastItem}.",
                Arguments(cursor.Id, cursor.PageSize, cursor.LastItem));

            var result = new CursorState<TEntity>(cursor, list);

            await CleanupSpecifiedCursorAsync(result, transaction, token);

            if (transaction != null)
            {
                await transaction.CommitAsync(token);
            }

            return _Logger.ExitMethod(result);
        }
        catch (OperationCanceledException)
        {
            _Logger.LogDebugMessage(
                "{nameof(GetNextPageAsync)}<{typeof(TEntity).Name}>(): Operation cancelled.",
                LogHelpers.Arguments(nameof(GetNextPageAsync), typeof(TEntity).Name));

            return new CursorState<TEntity>();
        }
    }

    /// <inheritdoc cref="DefaultCursorApiProvider" />
    public async Task DeleteCursorAsync(string id, CancellationToken token = default)
    {
        _Logger.EnterMethod();

        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        }

        _Logger.LogDebugMessage("Deleting cursor objects with id {id}", Arguments(id));

        try
        {
            await _Store.DeleteAsync(
                new[] { GetCacheId(id), GetListCacheId(id) },
                token);

            _Logger.ExitMethod();
        }
        catch (OperationCanceledException)
        {
            _Logger.LogDebugMessage("Operation cancelled.", LogHelpers.Arguments());
        }
    }
}
