using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Administration;

/// <summary>
///     Model class containing information about the server and the running arango instance
/// </summary>
public class ServerInfos
{
    /// <summary>
    ///     some details about the arango running instance (server-versions,host-ID,compiler,boost-version etc.)
    /// </summary>
    public Dictionary<string, object> Details { get; set; }

    /// <summary>
    ///     License Type as string (ex: community)
    /// </summary>
    public string License { get; set; }

    /// <summary>
    ///     Server name (arango)
    /// </summary>
    public string Server { get; set; }

    /// <summary>
    ///     Server version as string
    /// </summary>
    public string Version { get; set; }
}
