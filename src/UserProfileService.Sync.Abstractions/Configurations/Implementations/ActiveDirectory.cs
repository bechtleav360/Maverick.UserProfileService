using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;


/// <summary>
///     The connection object for an active directory (AD). If specifies
///     the connection string, the service user and the password that is used to connect
///     to the specific AD.
/// </summary>
public class ActiveDirectory 
{
    /// <summary>
    ///     The active directory connection object to connect with.
    /// </summary>
    public ActiveDirectoryConnection Connection { set; get; }
    
    /// <summary>
    ///     The specific ldap queries to get the specified object.
    /// </summary>
    public LdapQueries[] LdapQueries { set; get; }
    
    /// <summary>
    ///     The mapping between the ldap attributes and the model
    ///     properties. Key: ModelMaverickObject --> Value: LdapAttribute
    /// </summary>
    public Dictionary<string, string> EntitiesMapping { get; set; }

}
