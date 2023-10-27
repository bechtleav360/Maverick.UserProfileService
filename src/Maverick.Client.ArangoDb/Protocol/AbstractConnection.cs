using System;
using System.Net.Http;

namespace Maverick.Client.ArangoDb.Protocol;

internal class AbstractConnection
{
    internal IHttpClientFactory ClientFactory;
    internal bool ActiveFailOver { get; set; }

    internal string Alias { get; set; }

    internal Uri BaseUri { get; set; }

    internal string DatabaseName { get; set; }

    internal bool Debug { get; set; }

    internal string Hostname { get; set; }

    internal bool IsSecured { get; set; }

    internal string Password { get; set; }

    internal int Port { get; set; }

    internal string Username { get; set; }

    internal bool UseWebProxy { get; set; }

    internal AbstractConnection()
    {
    }

    internal AbstractConnection(string alias, bool useProxy = false)
    {
        Alias = alias;
        UseWebProxy = useProxy;
    }

    internal AbstractConnection(string alias, string databaseName, bool useProxy = false) : this(alias, useProxy)
    {
        DatabaseName = databaseName;
    }

    internal AbstractConnection(
        string alias,
        string hostname,
        int port,
        bool isSecured,
        string userName,
        string password,
        IHttpClientFactory clientFactory,
        bool debug = false,
        bool useWebProxy = false)
    {
        Alias = alias;
        Hostname = hostname;
        Port = port;
        IsSecured = isSecured;
        ClientFactory = clientFactory;
        Debug = debug;
        Password = password;
        UseWebProxy = useWebProxy;
    }
}
