using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using Maverick.UserProfileService.Models.Abstraction;

namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;

/// <summary>
///     The connection object for an active directory (AD). If specifies
///     the connection string, the service user and the password that is used to connect
///     to the specific AD.
/// </summary>
public class ActiveDirectoryConnection
{
    /// <summary>
    ///     The authentication type that is used to connect to the AD.
    /// </summary>
   public string AuthenticationType { set; get; }

    /// <summary>
    ///     The base bath to the AD.
    /// </summary>
    public string BasePath { set; get; }

    /// <summary>
    ///     The specific connection string to the AD.
    /// </summary>
    public string ConnectionString { set; get; }

    /// <summary>
    ///     A small description of the active directory connection.
    /// </summary>
    public string Description { set; get; }

    /// <summary>
    ///     If a certificate will be always ignored.
    /// </summary>
    public bool IgnoreCertificate { get; set; }

    /// <summary>
    ///     The port for connecting to LDAP.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    ///     Defines the mapping between the properties of ldap entries and internal <see cref="User" /> objects.
    ///     If no mapping is specified for a property, a default mapping is used.
    ///     Non-valid properties are ignored.
    ///     <br></br><br></br>
    ///     <b>Currently only the mapping for <see cref="IProfile.Id" /> is implemented.</b>
    ///     <br></br><br></br>
    ///     Key -> Target <see cref="IProfile" /> object
    ///     <br></br>
    ///     Value -> Source <see cref="Novell.Directory.Ldap.LdapEntry" /> or <see cref="DirectoryEntry" />
    /// </summary>
    public IDictionary<string, string> ProfileMapping { get; set; }

    /// <summary>
    ///     The service user that is used to login to the AD.
    /// </summary>
    public string ServiceUser { set; get; }

    /// <summary>
    ///     The password that is used to login to the AD.
    /// </summary>
    public string ServiceUserPassword { set; get; }

    /// <summary>
    ///     Describes if a secure connection to the LDAP is used.
    /// </summary>
    public bool UseSsl { set; get; }
}
