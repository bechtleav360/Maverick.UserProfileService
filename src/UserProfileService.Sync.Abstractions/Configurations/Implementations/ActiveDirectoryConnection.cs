using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using Maverick.UserProfileService.Models.Abstraction;

namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;

/// <inheritdoc />
public class ActiveDirectoryConnection : ILdapConnection
{
    /// <inheritdoc />
    public string AuthenticationType { get; set; }

    /// <inheritdoc />
    public string BasePath { get; set; }

    /// <inheritdoc />
    public string ConnectionString { get; set; }

    /// <inheritdoc />
    public string Description { set; get; }

    /// <inheritdoc />
    public bool IgnoreCertificate { get; set; }

    /// <inheritdoc />
    public int? Port { get; set; }

    /// <summary>
    ///     Defines the mapping between the properties of ldap entries and internal <see cref="IProfile" /> objects.
    ///     If no mapping is specified for a property, a default mapping is used.
    ///     Non-valid properties are ignored.
    ///     <br></br><br></br>
    ///     <b>Currently only the mapping for <see cref="IProfile.Id" /> is implemented.</b>
    ///     <br></br><br></br>
    ///     Key -> Target <see cref="IProfile" /> object
    ///     <br></br>
    ///     Value -> Source <see cref="DirectoryEntry" />
    /// </summary>
    public IDictionary<string, string> ProfileMapping { get; set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public string ServiceUser { get; set; }

    /// <inheritdoc />
    public string ServiceUserPassword { get; set; }

    /// <inheritdoc />
    public bool UseSsl { get; set; } = true;
}
