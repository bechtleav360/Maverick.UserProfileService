namespace Maverick.Client.ArangoDb.Protocol;

/// <summary>
///     A Class representing an host with the address and the port
/// </summary>
public class HostDescription
{
    internal string Host { get; set; }
    internal bool IsSecured { get; set; }
    internal int Port { get; set; }

    /// <summary>
    ///     Gets the <see cref="string"/> representation of the host.
    /// </summary>
    /// <returns>A <see cref="string"/> representing the host.</returns>
    public string GetHost()
    {
        string result = Host + ":" + Port + "/";

        return IsSecured ? "https://" + result : "http://" + result;
    }
}
