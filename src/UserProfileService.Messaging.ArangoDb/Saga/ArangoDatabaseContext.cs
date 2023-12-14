using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Messaging.ArangoDb.Configuration;
using UserProfileService.Messaging.ArangoDb.Extensions;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Saga;

/// <summary>
///     Arango implementation of <see cref="IDatabaseContext{TSaga}" />
/// </summary>
/// <typeparam name="TSaga"></typeparam>
public class ArangoDatabaseContext<TSaga> :
    IDatabaseContext<TSaga>
    where TSaga : class, ISagaVersion
{
    private readonly IArangoDbClient _arangoClient;
    private readonly ILogger<ArangoDatabaseContext<TSaga>> _logger;
    private readonly ArangoSagaRepositoryOptions<TSaga> _options;

    /// <summary>
    ///     Create an instance of <inheritdoc cref="ArangoDatabaseContext{TSaga}" />
    /// </summary>
    /// <param name="arangoClient">Client for arango database.</param>
    /// <param name="options">Options for arango saga repository.</param>
    /// <param name="loggerFactory">Factory to create logger.</param>
    public ArangoDatabaseContext(
        IArangoDbClient arangoClient,
        ArangoSagaRepositoryOptions<TSaga> options,
        ILoggerFactory loggerFactory)
    {
        _arangoClient = arangoClient;
        _options = options;
        _logger = loggerFactory.CreateLogger<ArangoDatabaseContext<TSaga>>();
    }

    private string BuildSystemKey(Guid correlationId)
    {
        _logger.EnterMethod();

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("Guid can not be empty.", nameof(correlationId));
        }

        string collectionName = _options.CollectionName;

        var systemId = $"{collectionName}/{_options.FormatSagaKey(correlationId)}";

        return _logger.ExitMethod<string>(systemId);
    }

    private async Task CheckMultiResponseAsync(
        MultiApiResponse response,
        string caller,
        bool throwException = true,
        bool throwExceptionIfNotFound = false,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        _logger.LogDebugMessage("Checking multi api response object in behalf.", LogHelpers.Arguments());

        List<BaseApiResponse> responseList =
            response.Responses as List<BaseApiResponse> ?? response.Responses?.ToList();

        if (responseList == null)
        {
            if (throwException)
            {
                throw new ArgumentNullException(nameof(response), "Multi api response object cannot be null.");
            }

            _logger.ExitMethod();

            return;
        }

        var exceptionList = new List<Exception>();

        var firstPage = true;

        foreach (BaseApiResponse apiResponse in responseList)
        {
            try
            {
                await CheckBaseApiResponseAsync(
                    apiResponse,
                    caller,
                    throwException,
                    throwExceptionIfNotFound,
                    firstPage ? AResponseType.CursorFirstResponse : AResponseType.Cursor,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebugMessage("Task/operation has been cancelled.", LogHelpers.Arguments());

                throw;
            }
            catch (Exception e)
            {
                exceptionList.Add(e);
            }
            finally
            {
                firstPage = false;
            }
        }

        // logging should be done by CheckBaseApiResponseAsync()
        if (!throwException)
        {
            _logger.ExitMethod();

            return;
        }

        if (exceptionList.Any())
        {
            throw new AggregateException("Errors occurred during processing requests.", exceptionList);
        }

        _logger.ExitMethod();
    }

    private async Task CheckBaseApiResponseAsync(
        BaseApiResponse response,
        string caller,
        bool throwException = true,
        bool throwExceptionIfNotFound = false,
        AResponseType responseType = AResponseType.Other,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (response?.DebugInfos != null && _logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebugMessage(
                "Response debugging info (during call of {methodName}): {debugInformation}.",
                LogHelpers.Arguments(caller, response.DebugInfos));
        }

        if (!throwException)
        {
            _logger.LogDebugMessage(
                "Result of {methodName}(): Request operation finished. Pushing up response without further checking.",
                LogHelpers.Arguments(caller));

            _logger.ExitMethod();

            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (response == null)
        {
            throw new Exception($"{GetType().Name}.{caller}(): Unknown exception occurred. Response object is null.");
        }

        if (response.Code == HttpStatusCode.NotFound && !throwExceptionIfNotFound)
        {
            _logger.LogDebugMessage(
                "Result of {methodName}(): ArangoDB service responded http code 'Not found', but pushing up response object without further checking.",
                LogHelpers.Arguments(caller));

            _logger.ExitMethod();

            return;
        }

        if (response.Code == HttpStatusCode.NotFound
            && responseType == AResponseType.Cursor)
        {
            throw new SagaException("AQL cursor was not found.", typeof(TSaga), Guid.Empty);
        }

        _logger.LogTraceMessage("Checking response object.", LogHelpers.Arguments());

        await CheckAResponseAsync(
                response,
                responseType: responseType,
                cancellationToken: cancellationToken,
                caller: caller)
            .ConfigureAwait(false);

        _logger.ExitMethod();
    }

    private static string CreateErrorMessage(
        BaseApiResponse response,
        AResponseType responseType,
        string caller)
    {
        if (response.Code == HttpStatusCode.NotFound
            && responseType == AResponseType.CursorFirstResponse)
        {
            return
                $"{caller}(): AQL related error during cursor request: A non-existing collection is accessed in the query string.";
        }

        if (response.Code == HttpStatusCode.BadRequest
            && responseType == AResponseType.CursorFirstResponse)
        {
            return
                $"{caller}(): Error during processing cursor request: ";
        }

        return $"{caller}(): Exception occurred during request ({response.Code:D}). {response.Exception?.Message}.";
    }

    private async Task UpdateInternalAsync(TSaga instance, CancellationToken token = default)
    {
        _logger.EnterMethod();

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        try
        {
            instance.Version++;

            TSaga existingInstance =
                await LoadAsync(instance.CorrelationId, token).ConfigureAwait(false);

            if (existingInstance.Version >= instance.Version)
            {
                // workaround: This should be an exception, but by now the state is saved twice
                // although the incoming events are marked as ignored
                _logger.LogInfoMessage(
                    "Saga version conflict (current: {currentVersion}; newVersion {newVersion}; sage type {sagaType}) - skipping storing saga state [saga id: {sagaId}].",
                    LogHelpers.Arguments(
                        instance.Version,
                        existingInstance.Version,
                        typeof(TSaga),
                        instance.CorrelationId));

                return;
            }

            string collectionName = _options.CollectionName;

            UpdateDocumentResponse<JObject> response = await _arangoClient.UpdateDocumentAsync(
                collectionName,
                _options.FormatSagaKey(instance.CorrelationId),
                instance.InjectDocumentKey(
                    t => _options.FormatSagaKey(t.CorrelationId),
                    _arangoClient.UsedJsonSerializerSettings));

            await CheckAResponseAsync(response, true, cancellationToken: token);
        }
        catch (Exception exception)
        {
            throw new SagaException("Saga update failed.", typeof(TSaga), instance.CorrelationId, exception);
        }
        finally
        {
            _logger.ExitMethod();
        }
    }

    /// <summary>
    ///     Checks the response of ArangoDB and will throw exceptions if it is not valid or contains errors.
    /// </summary>
    /// <remarks>
    ///     The flags <paramref name="throwConverterException" /> and <paramref name="ignoreNotFoundError" /> control the
    ///     specific exception handling of JSON conversion and responses with HTTP code 404 (not-found).<br/>
    ///     This method will try to get further error information by converting the body of response. Therefore it is an
    ///     asynchronous method.
    /// </remarks>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="response">The response to check</param>
    /// <param name="throwConverterException">If <c>true</c> JSON conversion errors will lead to exceptions, otherwise, they will be ignored.</param>
    /// <param name="responseType">Determines the type of the original request that returns this <paramref name="response"/>.</param>
    /// <param name="cancellationToken">A token to propagate cancellation requests.</param>
    /// <param name="ignoreNotFoundError">If <c>true</c> "not-found" responses will be ignored, otherwise, they will lead to exceptions.</param>
    /// <param name="caller">The calling member of the current call.</param>
    /// <returns>A task representing the asynchronous read operation.</returns>
    /// <exception cref="Exception">If an unknown exception occurred during processing the request.</exception>
    /// <exception cref="AggregateException">If the converter threw one-to-many exceptions</exception>
    /// <exception cref="SagaException"></exception>
    protected virtual Task CheckAResponseAsync<TResponse>(
        TResponse response,
        bool throwConverterException = false,
        AResponseType responseType = AResponseType.Other,
        CancellationToken cancellationToken = default,
        bool ignoreNotFoundError = false,
        [CallerMemberName] string caller = null)
        where TResponse : BaseApiResponse
    {
        _logger.EnterMethod();

        if (response == null)
        {
            throw new Exception($"{GetType().Name}.{caller}(): Unknown exception occurred during processing request.");
        }

        if (ResponseIsSuccessful() && throwConverterException && response.Exception != null)
        {
            // log debug infos with the exceptions themselves.
            _logger.LogDebugMessage(
                "Arango client reported exceptions although response code indicate successful operation. {exception}; Debug infos: {debug}",
                LogHelpers.Arguments(
                    response.Exception.Message,
                    GetDebugInfos()),
                caller);

            throw new AggregateException("Exception during processing response.", response.Exception);
        }

        if (ResponseIsSuccessful())
        {
            _logger.LogDebugMessage(
                "Result of {methodName}(): Response valid.",
                LogHelpers.Arguments(nameof(CheckAResponseAsync)));

            return _logger.ExitMethod(Task.CompletedTask);
        }

        if (response.Code == HttpStatusCode.NotFound && ignoreNotFoundError)
        {
            _logger.LogInfoMessage(
                "Result of {methodName}(): Result of attempt to fetch object: Not found - skipping method",
                LogHelpers.Arguments(caller));

            return _logger.ExitMethod(Task.CompletedTask);
        }

        cancellationToken.ThrowIfCancellationRequested();

        string debugInfo = GetDebugInfos();

        string message = CreateErrorMessage(
            response,
            responseType,
            caller);

        if (debugInfo != null)
        {
            _logger.LogErrorMessage(
                response.Exception,
                "Error occurred during request. Returned http code {responseCode}. Related debugging info: {debugInfo}.",
                LogHelpers.Arguments(response.Code, debugInfo));
        }
        else
        {
            _logger.LogErrorMessage(
                null,
                "Error occurred during request. Returned http code ({responseCode}. {responseException}.",
                LogHelpers.Arguments(response.Code, response.Exception?.Message));
        }

        Exception databaseException;

        try
        {
            databaseException = response.Exception != null
                ? new SagaException(message, typeof(TSaga), Guid.Empty, response.Exception)
                : new SagaException(message, typeof(TSaga), Guid.Empty);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "An error occurred while trying to convert arango exception to database exception. Exception message: '{errorMessage}'",
                e.Message.AsArgumentList());

            throw new SagaException(
                $"An error occurred while trying to convert arango exception to database exception. Message '{e.Message}'",
                typeof(TSaga),
                Guid.Empty);
        }

        throw databaseException;

        string GetDebugInfos()
        {
            if (response.DebugInfos != null)
            {
                return string.Concat(
                    $"{response.DebugInfos.RequestHttpMethod?.ToUpperInvariant()} {response.DebugInfos.RequestUri}",
                    !string.IsNullOrEmpty(response.DebugInfos.RequestJsonBody)
                        ? $": {response.DebugInfos.RequestJsonBody}"
                        : ".");
            }

            return null;
        }

        bool ResponseIsSuccessful()
        {
            return (int)response.Code >= 200 && (int)response.Code <= 299;
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        TSaga instance = context.Saga;

        await InsertAsync(instance, context.CancellationToken);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task InsertAsync(TSaga instance, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        string collectionName = _options.CollectionName;

        CreateDocumentResponse response = await _arangoClient.CreateDocumentAsync(
            collectionName,
            instance.InjectDocumentKey(
                t => _options.FormatSagaKey(t.CorrelationId),
                _arangoClient.UsedJsonSerializerSettings));

        await CheckAResponseAsync(response, true, cancellationToken: cancellationToken);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<(int count, IList<TSaga> results)> LoadAsync(
        int limit,
        int offset,
        Expression<Func<TSaga, object>> sortExpression = null,
        Expression<Predicate<TSaga>> filterExpression = null,
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        var sortPart = "";
        var filterPart = "";

        if (sortExpression != null)
        {
            string order = sortOrder == SortOrder.Asc ? "asc" : "desc";
            sortPart = $"sort {sortExpression.GetName("x")} {order}";
        }

        if (filterExpression != null)
        {
            filterPart = $"filter {filterExpression.GetName("x")}";
        }

        var query = $"for x in {_options.CollectionName} {filterPart} {sortPart} limit {offset}, {limit} return x";

        var countingQuery =
            $"for x in {_options.CollectionName} {filterPart} {sortPart} COLLECT WITH COUNT INTO length return length";

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "The following queries will be send to arango: Result Query: {query}, Counting Query: {secondQuery} ",
                LogHelpers.Arguments(query, countingQuery));
        }

        var cursor = new CreateCursorBody
        {
            Query = query,
            Options = new PostCursorOptions()
        };

        MultiApiResponse<TSaga> response;
        MultiApiResponse<int> countingResponse;

        try
        {
            response =
                await _arangoClient.ExecuteQueryWithCursorOptionsAsync<TSaga>(
                    cursor,
                    cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogErrorMessage(ex, "Error happened by executing arango query", LogHelpers.Arguments());

            throw;
        }

        try
        {
            _logger.LogInfoMessage("Executing a query to get the total amount of elements", LogHelpers.Arguments());

            countingResponse = await _arangoClient.ExecuteQueryAsync<int>(
                countingQuery,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(e, "Error happened by executing arango (counting) query", LogHelpers.Arguments());

            throw;
        }

        await CheckMultiResponseAsync(response, null, cancellationToken: cancellationToken);
        await CheckMultiResponseAsync(countingResponse, null, cancellationToken: cancellationToken);

        (int Count, List<TSaga>) result = (countingResponse.QueryResult.FirstOrDefault(),
            response.QueryResult.ToList());

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<TSaga> LoadAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("Guid can not be empty.", nameof(correlationId));
        }

        string systemId = BuildSystemKey(correlationId);

        GetDocumentResponse<TSaga> response =
            await _arangoClient.GetDocumentAsync<TSaga>(systemId);

        await CheckAResponseAsync(
            response,
            true,
            ignoreNotFoundError: true,
            cancellationToken: cancellationToken);

        return _logger.ExitMethod(response.Result);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        await UpdateInternalAsync(context.Saga, context.CancellationToken);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TSaga instance, CancellationToken token)
    {
        _logger.EnterMethod();

        await UpdateInternalAsync(instance, token);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        string systemId = BuildSystemKey(context.Saga.CorrelationId);

        DeleteDocumentResponse response = await _arangoClient.DeleteDocumentAsync(systemId);

        await CheckAResponseAsync(response, true, cancellationToken: context.CancellationToken);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<bool> TryDeleteAsync(SagaConsumeContext<TSaga> context)
    {
        _logger.EnterMethod();

        string systemId = BuildSystemKey(context.Saga.CorrelationId);

        DeleteDocumentResponse response = await _arangoClient.DeleteDocumentAsync(systemId);

        await CheckAResponseAsync(response,
            throwConverterException: true,
            ignoreNotFoundError: true,
            cancellationToken: context.CancellationToken);

        return _logger.ExitMethod((int)response.Code >= 200 && (int)response.Code < 300);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _logger.EnterMethod();

        return _logger.ExitMethod<ValueTask>(default);
    }
}
