using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Abstractions;

/// <summary>
///     Defines a base class for all repository classes that uses ArangoDb as backend.
/// </summary>
public abstract class ArangoRepositoryBase
{
    /// <summary>
    ///     The locking object, that can is used for concurrent calls.
    /// </summary>
    protected static readonly SemaphoreSlim ProfileLockObject = new SemaphoreSlim(1, 1);

    /// <summary>
    ///     The ArangoDB client factory that manages <see cref="IArangoDbClient" />s.
    /// </summary>
    protected IArangoDbClientFactory ArangoClientFactory =>
        ServiceScope.ServiceProvider.GetRequiredService<IArangoDbClientFactory>();

    /// <summary>
    ///     The name that will be used to create an ArangoDbClient from <see cref="IArangoDbClientFactory" />.
    /// </summary>
    protected virtual string ArangoDbClientName => ArangoConstants.DatabaseClientNameUserProfileStorage;

    /// <summary>
    ///     The logger that will accept the logging messages produced by <c>this</c> instance.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    ///     The instance of <see cref="IServiceProvider" /> that is used to get required services.
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     The instance of <see cref="IServiceScope" /> that is used to get required scoped services.
    /// </summary>
    protected IServiceScope ServiceScope { get; }

    /// <summary>
    ///     Initializes a new instance of the ArangoNestedQueryEnumerable repository.
    /// </summary>
    /// <param name="logger">The logger instance to be used for writing logging messages to.</param>
    /// <param name="serviceProvider">
    ///     The service provider is needed to create an <see cref="IArangoDbClientFactory" /> that
    ///     manages <see cref="IArangoDbClient" />s.
    /// </param>
    protected ArangoRepositoryBase(
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        ServiceScope = serviceProvider.CreateScope();
    }

    private async Task CheckMultiResponseAsync(
        MultiApiResponse response,
        string caller,
        bool throwException = true,
        CallingServiceContext context = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage("Checking multi api response object in behalf.", LogHelpers.Arguments());

        List<BaseApiResponse> responseList =
            response.Responses as List<BaseApiResponse> ?? response.Responses?.ToList();

        if (responseList == null)
        {
            if (throwException)
            {
                throw new ArgumentNullException(nameof(response), "Multi api response object cannot be null.");
            }

            Logger.ExitMethod();

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
                    true,
                    firstPage ? AResponseType.CursorFirstResponse : AResponseType.Cursor,
                    context,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebugMessage("Task/operation has been cancelled.", LogHelpers.Arguments());

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
            Logger.ExitMethod();

            return;
        }

        if (exceptionList.Any())
        {
            throw new AggregateException("Errors occurred during processing requests.", exceptionList);
        }

        Logger.ExitMethod();
    }

    private async Task CheckBaseApiResponseAsync(
        BaseApiResponse response,
        string caller,
        bool throwException = true,
        bool throwExceptionIfNotFound = false,
        AResponseType responseType = AResponseType.Other,
        CallingServiceContext context = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (response?.DebugInfos != null && Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebugMessage(
                "Response debugging info (during call of {methodName}): {debugInformation}.",
                LogHelpers.Arguments(caller, response.DebugInfos));
        }

        if (!throwException)
        {
            Logger.LogDebugMessage(
                "Result of {methodName}(): Request operation finished. Pushing up response without further checking.",
                LogHelpers.Arguments(caller));

            Logger.ExitMethod();

            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (response == null)
        {
            throw new Exception($"{GetType().Name}.{caller}(): Unknown exception occurred. Response object is null.");
        }

        if (response.Code == HttpStatusCode.NotFound && !throwExceptionIfNotFound)
        {
            Logger.LogDebugMessage(
                "Result of {methodName}(): ArangoDB service responded http code 'Not found', but pushing up response object without further checking.",
                LogHelpers.Arguments(caller));

            Logger.ExitMethod();

            return;
        }

        if (response.Code == HttpStatusCode.NotFound
            && responseType == AResponseType.Cursor)
        {
            throw new DatabaseException("AQL cursor was not found.", ExceptionSeverity.Error);
        }

        Logger.LogTraceMessage("Checking response object.", LogHelpers.Arguments());

        await CheckAResponseAsync(
                response,
                responseType: responseType,
                cancellationToken: cancellationToken,
                context: context,
                caller: caller)
            .ConfigureAwait(false);

        Logger.ExitMethod();
    }

    private static string CreateErrorMessage(
        BaseApiResponse response,
        AResponseType responseType,
        string caller,
        CallingServiceContext context)
    {
        string contextMessage = context == null || context.IsEmpty
            ? string.Empty
            : $"[initially requested by {context}]";

        if (response.Code == HttpStatusCode.NotFound
            && responseType == AResponseType.CursorFirstResponse)
        {
            return
                $"{caller}(){contextMessage}: AQL related error during cursor request: A non-existing collection is accessed in the query string.";
        }

        if (response.Code == HttpStatusCode.BadRequest
            && responseType == AResponseType.CursorFirstResponse)
        {
            return
                $"{caller}(){contextMessage}: Error during processing cursor request: ";
        }

        return
            $"{caller}(){contextMessage}: Exception occurred during request ({response.Code:D}). {response.Exception?.Message}.";
    }

    /// <summary>
    ///     Returns an instance of <see cref="IArangoDbClient" /> that will handle requests.
    /// </summary>
    /// <returns></returns>
    protected virtual IArangoDbClient GetArangoDbClient()
    {
        return ArangoClientFactory.Create(ArangoDbClientName);
    }

    /// <summary>
    ///     Sends a command to the ArangoDB backend and throws exception depending on the HTTP status code of the response and
    ///     the parameter <paramref name="throwException" />.<br />
    ///     If <paramref name="throwException" /> is set to <c>true</c>, it will check the response item using the virtual
    ///     method <see cref="CheckAResponseAsync{TResponse}" />.
    /// </summary>
    /// <typeparam name="TResponse">Type of the embedded data object.</typeparam>
    /// <param name="throwExceptionIfNotFound">
    ///     If set to <c>true</c>, an exception will be thrown, if
    ///     ArangoNestedQueryEnumerable responded with a code 404 (default: <c>true</c>).
    /// </param>
    /// <param name="caller">
    ///     Method or property name of the caller to the method. If <c>null</c>, the system will set it
    ///     itself.
    /// </param>
    /// <param name="command">An asynchronous function that will be processed.</param>
    /// <param name="throwException">
    ///     If set to <c>true</c>, an exception will be thrown, if ArangoNestedQueryEnumerable
    ///     returned an error code (default: false).
    /// </param>
    /// <param name="context">The context that contains information about the calling service.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation which wrap the result item of <paramref name="command" />
    ///     function.
    /// </returns>
    protected virtual async Task<TResponse> SendRequestAsync<TResponse>(
        Func<IArangoDbClient, Task<TResponse>> command,
        bool throwException = true,
        bool throwExceptionIfNotFound = false,
        CallingServiceContext context = default,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = null)
        where TResponse : IApiResponse
    {
        Logger.EnterMethod();

        Logger.LogTraceMessage(
            "Connection built. Sending request in behalf of {caller}().",
            LogHelpers.Arguments(caller));

        TResponse response = await command.Invoke(GetArangoDbClient()).ConfigureAwait(false);

        if (response is BaseApiResponse baseApiResponse)
        {
            await CheckBaseApiResponseAsync(
                baseApiResponse,
                caller,
                throwException,
                throwExceptionIfNotFound,
                context: context,
                cancellationToken: cancellationToken);

            return Logger.ExitMethod(response);
        }

        if (response is MultiApiResponse multi)
        {
            await CheckMultiResponseAsync(
                multi,
                caller,
                throwException,
                context,
                cancellationToken);

            return Logger.ExitMethod(response);
        }

        if (response is PaginationApiResponse paginated)
        {
            await CheckMultiResponseAsync(
                paginated.OriginalCountingResponse,
                caller,
                throwException,
                context,
                cancellationToken);

            await CheckMultiResponseAsync(
                paginated.OriginalSelectionResponse,
                caller,
                throwException,
                context,
                cancellationToken);

            return Logger.ExitMethod(response);
        }

        Logger.LogDebugMessage(
            "No checking method for {typeOfTResponse} set. Skipping.",
            LogHelpers.Arguments(typeof(TResponse).Name));

        return Logger.ExitMethod(response);
    }

    /// <summary>
    ///     Checks the response item coming from ArangoDb client. If some errors occurred, it will throw exceptions.
    /// </summary>
    /// <typeparam name="TResponse">Type of the response object wrapped by an ArangoDb response object.</typeparam>
    /// <param name="response">The response item which was returned by ArangoDB client.</param>
    /// <param name="throwConverterException">
    ///     Throw an <see cref="AggregateException" /> if Arango rest client reported some
    ///     exceptions during deserialization.
    /// </param>
    /// <param name="responseType">The type of the response to be checked.</param>
    /// <param name="context">Information about the service that initially send the request.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <param name="caller">
    ///     Method or property name of the caller to the method. If <c>null</c>, the system will set it
    ///     itself.
    /// </param>
    protected virtual Task CheckAResponseAsync<TResponse>(
        TResponse response,
        bool throwConverterException = false,
        AResponseType responseType = AResponseType.Other,
        CallingServiceContext context = default,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = null)
        where TResponse : BaseApiResponse
    {
        Logger.EnterMethod();

        if (response == null)
        {
            throw new Exception($"{GetType().Name}.{caller}(): Unknown exception occurred during processing request.");
        }

        if (ResponseIsSuccessful() && throwConverterException && response.Exception != null)
        {
            // log debug infos with the exceptions themselves.
            Logger.LogDebugMessage(
                "Arango client reported exceptions although response code indicate successful operation. {exception}; Debug infos: {debug}",
                Arguments(
                    response.Exception.Message,
                    GetDebugInfos()),
                caller);

            throw new AggregateException("Exception during processing response.", response.Exception);
        }

        if (ResponseIsSuccessful())
        {
            Logger.LogDebugMessage(
                "Result of {methodName}(): Response valid.",
                Arguments(nameof(CheckAResponseAsync)));

            return Logger.ExitMethod(Task.CompletedTask);
        }

        cancellationToken.ThrowIfCancellationRequested();

        string debugInfo = GetDebugInfos();

        string message = CreateErrorMessage(
            response,
            responseType,
            caller,
            context);

        if (debugInfo != null)
        {
            Logger.LogErrorMessage(
                response.Exception,
                "Error occurred during request. Returned http code {responseCode}. Related debugging info: {debugInfo}.",
                Arguments(response.Code, debugInfo));
        }
        else
        {
            Logger.LogErrorMessage(
                null,
                "Error occurred during request. Returned http code ({responseCode}. {responseException}.",
                Arguments(response.Code, response.Exception?.Message));
        }

        Exception databaseException;

        try
        {
            databaseException = response.Exception.ToDatabaseException(message);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "An error occurred while trying to convert arango exception to database exception. Exception message: '{errorMessage}'",
                e.Message.AsArgumentList());

            throw new DatabaseException(
                $"An error occurred while trying to convert arango exception to database exception. Message '{e.Message}'",
                ExceptionSeverity.Error);
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

    /// <summary>
    ///     Locks the access to profile and security objects.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async Task LockProfileRepoAsync(CancellationToken cancellationToken = default)
    {
        await ProfileLockObject.WaitAsync(cancellationToken);
    }

    /// <summary>
    ///     Releases the lock of access to profiles and security objects.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected Task UnlockProfileRepoAsync(CancellationToken cancellationToken = default)
    {
        ProfileLockObject.Release();
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Trims only one character at the beginning and one at the end of an <paramref name="input" />.
    /// </summary>
    protected static string TrimQuotationMarkOnce(string input)
    {
        if (input == null)
        {
            return null;
        }

        int start = input.StartsWith('"') ? 1 : 0;
        int end = input.EndsWith('"') ? input.Length - 1 - start : input.Length - start;

        return input.Substring(start, end);
    }

    /// <summary>
    ///     Trims only one character at the beginning and one at the end of an <paramref name="input" /> of each element in the
    ///     sequence.
    /// </summary>
    protected static IEnumerable<string> TrimQuotationMarkOnce(IEnumerable<string> input)
    {
        return input?.Select(TrimQuotationMarkOnce);
    }

    /// <summary>
    ///     Converts several input arguments to an object array (can be helpful for logging messages).
    /// </summary>
    /// <param name="arguments">The arguments to be combined in an object array.</param>
    /// <returns>The object array containing all <paramref name="arguments" />.</returns>
    protected static object[] Arguments(params object[] arguments)
    {
        return LogHelpers.Arguments(arguments);
    }
}
