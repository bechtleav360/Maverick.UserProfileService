# Configure LDAP Connector
As of now, we offer support for an LDAP Connector capable of synchronizing data from an existing Active Direcotry for example. The configuration for this feature can be found under the `LDAP` section. We will explain all sections step by step. Below is an example configuration for the Active Directory System.

??? abstract "LDAP example configuration"
    ```json
    {
      "Ldap": {
        "EntitiesMapping": {
          "DisplayName": "displayname",
          "Email": "mail",
          "FirstName": "givenName",
          "LastName": "sn",
          "Name": "Name",
          "UserName": "cn"
        },
        "LdapConfiguration": [
          {
            "Connection": {
              "AuthenticationType": "None",
              "BasePath": "dc=ad, dc=example, dc=com",
              "ConnectionString": "LDAP://ad.exmpale.com",
              "Description": "Default AD of A365 development environment",
              "IgnoreCertificate": false,
              "Port": 389,
              "ServiceUser": "CN=dev,OU=ExampleOU,OU=ExampleOU2,OU=DEVOU,DC=ad,DC=example,DC=com",
              "ServiceUserPassword": "Password",
              "UseSsl": false
            },
            "LdapQueries": [
              {
                "Filter": "(&(|(objectClass=user)(objectClass=inetOrgPerson))(!(objectClass=computer))(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))",
                "SearchBase": "OU=Users,OU=Accounts,OU=Management"
              }
            ]
          }
        ],
        "Source": {
          "users": {
            "ForceDelete": "False",
            "Operations": "Add,Update,Delete"
          }
        }
      }
    }
    ```

### EntitiesMapping Section
The entity mapping is utilized to map [LDAP attributes](https://documentation.sailpoint.com/connectors/active_directory/help/integrating_active_directory/ldap_names.html) to the user. On the left side, properties of the user model are specified. On the right side, LDAP attributes are utilized. Therefore, the **DisplayName** property will contain values stored under the attribute **displayName** in the LDAP Sytem. It's important to note that the properties of the user model must be written exactly as in the class. Refer to the [UserModel](https://github.com/bechtleav360/Maverick.UserProfileService/blob/main/src/Maverick.UserProfileService.Models/BasicModels/UserBasic.cs) for more details.

### LdapConfiguration Section
Under the **LdapConfiguration**, you can specify the LDAP system from which you want to synchronize the data. The **Connection** specifies the credentials for accessing the LDAP system. The **LdapQueries** precisely define which users should be synchronized to the user profile system. You can specify multiple LDAP systems from which you want to synchronize.

### Connection Section

The Connection object encapsulates all the necessary properties required to establish a connection with an existing LDAP system. We utilize the Novell.Directory.Ldap.NETStandard library for this purpose. For further details regarding the connection, please refer to the documentation provided here.

`AuthenticationType` - Is used to specify the type of authentication to be used when logging into an existing LDAP system. "None" is using no authentification. Only a username and a password are required.

`BasePath` - The base bath to the LDPA System.

`ConnectionString` - The connection to the LDAP System.

`Description` - The description of the used system. This property is optional.

`IgnoreCertificate` - If a certificate is to be ignored during synchronization with the LDAP System. The default value is `false`.

`Port` - The port that is used to create a connection to the LDAP-System. The port used to establish the connection is typically 389, which is the standard port for LDAP. The secured standard port is 636, and it should be used when `UseSsL` is set to true. **Please note** that these ports are standard settings. When configuring your LDAP system, you may also use **other ports**.

`ServiceUser` - The Service user that is used to connect to the LDAP System.

`ServiceUserPassword` - The password of the `ServiceUser` for logging into the LDAP system.

`UseSsl` - Indicates whether a secure connection to the LDAP system is being used. If this property is enabled, the port must be set to **the secure port**. The standard secure port is 636, but it can vary based on your LDAP system configuration.

### LdapQueries Section
LDAP queries are search requests sent to an LDAP directory to retrieve specific information. They allow for searching and retrieving data about users, groups, organizational units, and other directory objects. LDAP queries can include various parameters such as filters defining specific search criteria and attributes specifying which information to return.

`Filter` - LDAP filters are expressions used in LDAP queries to specify specific search criteria. They allow for filtering search results to return only objects that meet certain properties.

`SearchBase` - The "SearchBase" in LDAP systems refers to the base distinguished name (DN) from which LDAP searches are initiated. It serves as the starting point for search operations in the directory tree. The server begins the search from this base DN and traverses the directory tree to locate objects matching the search criteria.

You can specify as many queries as needed, ensuring they match the configuration of your LDAP system.

For more information, it's recommended to explore LDAP and familiarize yourself with the various [filters](https://ldap.com/ldap-filters/) that can be utilized.

### Source Section
This section solely describes the entities that can be synchronized from the LDAP system and whether the entities can be modified in the UserProfileService. For the LDAP Connector, only user entities can be synchronized from an LDAP system. Here is a brief configuration of the source section:
??? abstract "LDAP source example configuration"
    ```json
    {
      "Source": {
        "users": {
          "ForceDelete": "False",
          "Operations": "Add,Update,Delete"
        }
      }
    }
    ```
`Operations` - Defines the operations that can be performed on users. Possible configurations include `added`, `updated`, or `deleted`.

`ForceDelete` - Indicates whether users should be forcefully deleted if they are not present in the LDAP system.

### Recommendation
This was just a brief introduction to configuring the synchronization system using LDAP. We assume familiarity with this protocol. If not, we recommend further reading to gain understanding.