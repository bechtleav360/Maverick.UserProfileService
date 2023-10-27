using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.DependencyInjection;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.PerformanceLogging.Abstractions;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Maverick.Client.ArangoDb.Public.Models.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HttpMethod = Maverick.Client.ArangoDb.Protocol.HttpMethod;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     The Central class for interaction with ArangoDB endpoints
/// </summary>
public class ADatabase : IDisposable, IADatabase
{
    private readonly Connection _connection;
    private readonly SemaphoreSlim _parameterLock = new SemaphoreSlim(1, 1);
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    /// <summary>
    ///     Provides access to administration operations in current database context.
    /// </summary>
    public Administration Administration => new Administration(_connection);

    /// <summary>
    ///     Provides access to collection operations in current database context.
    /// </summary>
    public ACollection Collection => new ACollection(_connection);

    /// <summary>
    ///     Default serializer settings to be used during deserialization.
    /// </summary>
    public JsonSerializerSettings DefaultSerializerSettings { get; }

    /// <summary>
    ///     Provides access to document operations in current database context.
    /// </summary>
    public ADocument Document => new ADocument(_connection);

    /// <summary>
    ///     Provides access to foxx services in current database context.
    /// </summary>
    public AFoxx Foxx => new AFoxx(_connection);

    /// <summary>
    ///     Provides access to AQL user function management operations in current database context.
    /// </summary>
    public AFunction Function => new AFunction(_connection);

    /// <summary>
    ///     Provides access to index operations in current database context.
    /// </summary>

    public AIndex Index => new AIndex(_connection);

    /// <summary>
    ///     Provides access to query operations in current database context.
    /// </summary>
    public AQuery Query => new AQuery(_connection);

    /// <summary>
    ///     Provides access to transaction operations in current database context.
    /// </summary>
    public ATransaction Transaction => new ATransaction(_connection);

    /// <summary>
    ///     Initializes new database context to perform operations on remote database identified by the connectionString
    /// </summary>
    /// <param name="connectionString">
    ///     Connection string that contains information to establish the connection to an ArangoDB
    ///     server.
    /// </param>
    /// <param name="clientFactory">
    ///     Client factory that can be used to generate httpClient <see cref="IHttpClientFactory" />
    /// </param>
    /// <param name="exceptionOptions">Options, how to handle exceptions within the client.</param>
    /// <param name="logger">
    ///     The logger instance to be used to write logging messages to. Default: null, that means no logger
    ///     is used.
    /// </param>
    /// <param name="defaultSerializerSettings">Default serializer settings as optional parameter.</param>
    /// <param name="clientName">Name of the http client when it is requested from <see cref="IHttpClientFactory" />.</param>
    /// <param name="performanceLogSettings">
    ///     Contains the settings for the performance logger. If <c>null</c>, performance
    ///     logging will stay deactivated.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="clientName" /> is an empty string or only contains whitespaces.</exception>
    public ADatabase(
        string connectionString,
        IHttpClientFactory clientFactory,
        ArangoExceptionOptions exceptionOptions = null,
        ILogger logger = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        string clientName = AConstants.ArangoClientName,
        IPerformanceLogSettings performanceLogSettings = null)
    {
        clientName ??= AConstants.ArangoClientName;

        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentException(
                $"Parameter {nameof(clientName)} cannot be empty or whitespace.",
                nameof(clientName));
        }

        if (performanceLogSettings != null
            && typeof(IPerformanceLogger).IsAssignableFrom(performanceLogSettings.GetImplementationType()))
        {
            Ioc.Default.ConfigureServices(
                services => services.AddSingleton(
                    typeof(IPerformanceLogger),
                    Activator.CreateInstance(
                        performanceLogSettings.GetImplementationType(),
                        performanceLogSettings)));
        }

        DefaultSerializerSettings = defaultSerializerSettings;

        _connection = new Connection(
            connectionString,
            clientFactory,
            exceptionOptions,
            DefaultSerializerSettings,
            logger,
            clientName);
    }

    /// <summary>
    ///     Initializes new database context to perform operations on remote database identified by the connectionString
    /// </summary>
    /// <param name="connectionString">
    ///     Connection string that contains information to establish the connection to an ArangoDB
    ///     server.
    /// </param>
    /// <param name="httpClient">Http client that will be used to send requests to the ArangoDB service.</param>
    /// <param name="exceptionOptions">Options, how to handle exceptions within the client.</param>
    /// <param name="logger">
    ///     The logger instance to be used to write logging messages to. Default: null, that means no logger
    ///     is used.
    /// </param>
    /// <param name="defaultSerializerSettings">Default serializer settings as optional parameter.</param>
    /// <param name="performanceLogSettings">
    ///     Contains the settings for the performance logger. If <c>null</c>, performance
    ///     logging will stay deactivated.
    /// </param>
    public ADatabase(
        string connectionString,
        HttpClient httpClient,
        ArangoExceptionOptions exceptionOptions = null,
        ILogger logger = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        IPerformanceLogSettings performanceLogSettings = null)
    {
        DefaultSerializerSettings = defaultSerializerSettings;

        _connection = new Connection(
            connectionString,
            httpClient,
            exceptionOptions,
            DefaultSerializerSettings,
            logger);

        if (performanceLogSettings != null
            && typeof(IPerformanceLogger).IsAssignableFrom(performanceLogSettings.GetImplementationType()))
        {
            Ioc.Default.ConfigureServices(
                services => services.AddSingleton(
                    typeof(IPerformanceLogger),
                    Activator.CreateInstance(
                        performanceLogSettings.GetImplementationType(),
                        performanceLogSettings)));
        }
    }

    /// <summary>
    ///     Initializes new database context to perform operations on remote database identified by the connectionString
    /// </summary>
    /// <param name="connectionString">
    ///     Connection string that contains information to establish the connection to an ArangoDB
    ///     server.
    /// </param>
    /// <param name="exceptionOptions">Options, how to handle exceptions within the client.</param>
    /// <param name="logger">
    ///     The logger instance to be used to write logging messages to. Default: null, that means no logger
    ///     is used.
    /// </param>
    /// <param name="defaultSerializerSettings">Default serializer settings as optional parameter.</param>
    /// <param name="performanceLogSettings">
    ///     Contains the settings for the performance logger. If <c>null</c>, performance
    ///     logging will stay deactivated.
    /// </param>
    public ADatabase(
        string connectionString,
        ArangoExceptionOptions exceptionOptions = null,
        ILogger logger = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        IPerformanceLogSettings performanceLogSettings = null)
    {
        DefaultSerializerSettings = defaultSerializerSettings;

        _connection = new Connection(
            connectionString,
            null as IHttpClientFactory,
            exceptionOptions,
            DefaultSerializerSettings,
            logger);

        if (performanceLogSettings != null
            && typeof(IPerformanceLogger).IsAssignableFrom(performanceLogSettings.GetImplementationType()))
        {
            Ioc.Default.ConfigureServices(
                services => services.AddSingleton(
                    typeof(IPerformanceLogger),
                    Activator.CreateInstance(
                        performanceLogSettings.GetImplementationType(),
                        performanceLogSettings)));
        }
    }

    /// <inheritdoc />
    public async Task<GetCurrentDatabaseResponse> GetCurrentDatabaseInfoAsync()
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Database, "/current");
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            DatabaseInfoEntity body = response.ParseBody<Body<DatabaseInfoEntity>>()?.Result;

            return new GetCurrentDatabaseResponse(response, body);
        }

        return new GetCurrentDatabaseResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetDatabasesResponse> GetAccessibleDatabasesAsync()
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Database, "/user");
        await _parameterLock.WaitAsync().ConfigureAwait(false);

        try
        {
            Response response = await RequestHandler.ExecuteAsync(_connection, request);

            if (response.IsSuccessStatusCode)
            {
                List<string> body = response.ParseBody<Body<List<string>>>()?.Result;

                return new GetDatabasesResponse(response, body);
            }

            return new GetDatabasesResponse(response, response.Exception);
        }
        finally
        {
            _parameters.Clear();
            _parameterLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<GetDatabasesResponse> GetAllDatabasesAsync()
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Database);
        await _parameterLock.WaitAsync().ConfigureAwait(false);

        try
        {
            Response response = await RequestHandler.ExecuteAsync(_connection, request);

            if (response.IsSuccessStatusCode)
            {
                List<string> body = response.ParseBody<Body<List<string>>>()?.Result;

                return new GetDatabasesResponse(response, body);
            }

            return new GetDatabasesResponse(response, response.Exception);
        }
        finally
        {
            _parameters.Clear();
            _parameterLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<DropDbResponse> DropDatabaseAsync(string databaseName)
    {
        var request = new Request(HttpMethod.Delete, ApiBaseUri.Database, "/" + databaseName);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            bool? body = response.ParseBody<Body<bool>>()?.Result;

            return new DropDbResponse(response, body.GetValueOrDefault());
        }

        return new DropDbResponse(response, response.Exception);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    /// <summary>
    ///     Determines whether system collections should be excluded from the result.
    /// </summary>
    public ADatabase ExcludeSystem(bool value)
    {
        _parameterLock.Wait();

        try
        {
            // string because value will be stored in query string
            _parameters.String(ParameterName.ExcludeSystem, value.ToString().ToLower());

            return this;
        }
        finally
        {
            _parameterLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<CreateDbResponse> CreateDatabaseAsync(
        string databaseName,
        DatabaseInfoEntityOptions creationOptions = null)
    {
        return await CreateDatabaseAsync(databaseName, null, creationOptions).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CreateDbResponse> CreateDatabaseAsync(
        string databaseName,
        IList<AUser> users,
        DatabaseInfoEntityOptions creationOptions = null)
    {
        var bodyDocument = new Dictionary<string, object>();
        // required: database name
        bodyDocument.String("name", databaseName);

        // optional: list of users
        if (users != null && users.Count > 0)
        {
            var userList = new List<Dictionary<string, object>>();

            foreach (AUser user in users)
            {
                var userItem = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(user.Username))
                {
                    userItem.String("username", user.Username);
                }

                if (!string.IsNullOrEmpty(user.Password))
                {
                    userItem.String("passwd", user.Password);
                }

                userItem.Bool("active", user.Active);

                if (user.Extra != null)
                {
                    userItem.Document("extra", user.Extra);
                }

                userList.Add(userItem);
            }

            bodyDocument.List("users", userList);
        }

        if (creationOptions != null)
        {
            bodyDocument.Add("options", creationOptions);
        }

        var request = new Request<Dictionary<string, object>>(HttpMethod.Post, ApiBaseUri.Database, bodyDocument);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            bool? body = response.ParseBody<Body<bool>>()?.Result;

            return new CreateDbResponse(response, body.GetValueOrDefault());
        }

        return new CreateDbResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetAllCollectionsResponse> GetAllCollectionsAsync(bool excludeSystem)
    {
        ExcludeSystem(excludeSystem); // to be on the safe side

        return await GetAllCollectionsAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<GetAllCollectionsResponse> GetAllCollectionsAsync()
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Collection);
        await _parameterLock.WaitAsync().ConfigureAwait(false);

        try
        {
            Response response = await RequestHandler.ExecuteAsync(_connection, request);

            if (response.IsSuccessStatusCode)
            {
                var res = response.ParseBody<Body<IList<CollectionEntity>>>();

                return new GetAllCollectionsResponse(response, res?.Result);
            }

            return
                new GetAllCollectionsResponse(response, response.Exception);
        }
        finally
        {
            _parameters.Clear();
            _parameterLock.Release();
        }
    }
}
