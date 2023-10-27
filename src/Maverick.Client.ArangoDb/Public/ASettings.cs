using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Configuration;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

[Obsolete]
public static class ASettings
{
    internal static readonly Regex KeyRegex = new Regex(@"^[a-zA-Z0-9_\-:.@()+,=;$!*'%]*$");
    private static readonly Dictionary<string, Connection> _connections = new Dictionary<string, Connection>();

    /// <summary>
    ///     Determines driver name.
    /// </summary>
    public const string DriverName = "ArangoDB-NET";

    /// <summary>
    ///     Determines driver version.
    /// </summary>
    public const string DriverVersion = "0.10.2";

    /// <summary>
    ///     Determines JSON serialization options.
    /// </summary>
    public static JsonParameters JsonParameters { get; set; }

    static ASettings()
    {
        JsonParameters = new JsonParameters
        {
            UseEscapedUnicode = false,
            UseFastGuid = false,
            UseExtensions = false
        };
    }

    internal static Connection GetConnection(string alias, bool useCopy = true)
    {
        if (_connections.ContainsKey(alias))
        {
            return useCopy ? _connections[alias].Clone() : _connections[alias];
        }

        return null;
    }

    public static void RemoveConnection(string alias)
    {
        if (_connections.ContainsKey(alias))
        {
            _connections.Remove(alias);
        }
    }

    public static bool HasConnection(string alias)
    {
        return _connections.ContainsKey(alias);
    }

    [Obsolete(
        "This method is deprecated, we recommand to use AddConnection(string alias, string connectionString, IHttpClientFactory clientFactory, bool useWebProxy = false)")]
    public static void AddConnection(
        string alias,
        string hostname,
        int port,
        bool isSecured,
        ArangoExceptionOptions exceptionOptions = null,
        bool useWebProxy = false,
        bool debug = false)
    {
        AddConnection(
            alias,
            hostname,
            port,
            isSecured,
            "",
            "",
            exceptionOptions,
            useWebProxy,
            debug);
    }

    public static void AddConnection(
        string alias,
        string connectionString,
        IHttpClientFactory clientFactory = null,
        ArangoExceptionOptions exceptionOptions = null)
    {
        var connection = new Connection(alias, connectionString, exceptionOptions, null, clientFactory);
        _connections.Add(alias, connection);
    }

    [Obsolete(
        "This method is deprecated, we recommand to use AddConnection(string alias, string connectionString, IHttpClientFactory clientFactory, bool useWebProxy = false)")]
    public static void AddConnection(
        string alias,
        string hostname,
        int port,
        bool isSecured,
        string username,
        string password,
        ArangoExceptionOptions exceptionOptions = null,
        bool useWebProxy = false,
        bool debug = false)
    {
        var connection = new Connection(
            alias,
            hostname,
            port,
            isSecured,
            username,
            password,
            exceptionOptions,
            useWebProxy,
            debug);

        _connections.Add(alias, connection);
    }

    public static void AddConnection(
        string alias,
        string connectionString,
        string databaseName,
        IHttpClientFactory clientFactory,
        ArangoExceptionOptions exceptionOptions = null)
    {
        var connection = new Connection(
            alias,
            connectionString,
            databaseName,
            exceptionOptions,
            null,
            clientFactory);

        _connections.Add(alias, connection);
    }

    [Obsolete]
    public static void AddConnection(
        string alias,
        string hostname,
        int port,
        bool isSecured,
        string databaseName,
        ArangoExceptionOptions exceptionOptions = null,
        bool useWebProxy = false,
        bool debug = false)
    {
        AddConnection(
            alias,
            hostname,
            port,
            isSecured,
            databaseName,
            "",
            "",
            exceptionOptions,
            useWebProxy,
            debug);
    }

    [Obsolete(
        "This method is deprecated, we recommand to use: AddConnection(string alias, string connectionString, string databaseName, IHttpClientFactory clientFactory)")]
    public static void AddConnection(
        string alias,
        string hostname,
        int port,
        bool isSecured,
        string databaseName,
        string username,
        string password,
        ArangoExceptionOptions exceptionOptions = null,
        bool useWebProxy = false,
        bool debug = false)
    {
        var connection = new Connection(
            alias,
            hostname,
            port,
            isSecured,
            databaseName,
            username,
            password,
            exceptionOptions,
            useWebProxy,
            debug);

        _connections.Add(alias, connection);
    }
}
