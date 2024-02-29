## Docker Build with JSON Files

In these examples, Docker will build the specific Dockerfile located within the service/worker folder. Here, you are free to modify the Dockerfile as needed. For instance, you can alter the base image or adjust other components. All configurations are stored in JSON files. For guidance on configuring the main components, please refer to the [README.md](https://github.com/bechtleav360/Maverick.UserProfileService/blob/main/README.md) file. To initiate the build for the ups-environment, you can use the following command:

```ps1
docker compose -f .\docker-compose-ups-environment.yml --env-file docker-compose-env-vars.env  up
```

## The docker-compose-env-var file
The file is utilized to configure the primary storage components. You can configure parameters such as the user, password, and database, for example. However, please note that if you modify the password or user of any components, you must also update them in the configuration accordingly. Prior to startup, ensure to modify the **WORKING_DIRECTORY_CONTEXT** variable. This variable contains the absolute path to the UserProfileService.

For example, if your UserProfileService code is located under E:\Repos\Maverick.UserProfileService, then your variable should appear as follows:

```env
WORKING_DIRECTORY_CONTEXT='E:\Repos\Maverick.UserProfileService'
```

## The configuration files

In this example, configuration is performed using JSON files. JSON files offer greater readability and understanding compared to env files. These JSON files are mapped to the required services container in the **appsettings.Development.json** file. It's important to note that all examples are intended to run in a development environment.

## Configure the UPS-Sync with LDAP Connector
As of now, we offer support for an LDAP Connector capable of synchronizing data from an existing Active Direcotry for example. The configuration for this feature can be found under the `LDAP` section. We will explane all section step by step. Below is an example configuration for the Active Directory System.


```json
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
```

### EntitiesMapping Section
The entity mapping is utilized to map [LDAP attributes](https://documentation.sailpoint.com/connectors/active_directory/help/integrating_active_directory/ldap_names.html) to the user. On the left side, properties of the user model are specified. On the right side, LDAP attributes are utilized. Therefore, the **DisplayName** property will contain values stored under the attribute **displayName** in the LDAP Sytem. It's important to note that the properties of the user model must be written exactly as in the class. Refer to the [UserModel](https://github.com/bechtleav360/Maverick.UserProfileService/blob/main/src/Maverick.UserProfileService.Models/BasicModels/UserBasic.cs) for more details.

### LdapConfiguration Section
Under the **LdapConfiguration**, you can specify the LDAP system from which you want to synchronize the data. The **Connection** specifies the credentials for accessing the LDAP system. The **LdapQueries** precisely define which users should be synchronized to the user profile system. You can specify multiple LDAP systems from which you want to synchronize.

### Connection Section

The Connection object encapsulates all the necessary properties required to establish a connection with an existing LDAP system. We utilize the Novell.Directory.Ldap.NETStandard library for this purpose. For further details regarding the connection, please refer to the documentation provided here.

`AuthenticationType` - Is used to specify the type of authentication to be used when logging into an existing LDAP system. "None" is using no authentification. Only a username and a password are required.

`BasePath` - The base bath to the LDPA System.

`ConnectionString` - The conenction to the LDPA System.

`Description` - The description of the used system. This property is optional.

`IgnoreCertificate` - If a certificate is to be ignored during synchronization with the LDPA System.

`Port` - The port used to establish the connection. Port 389 is the standard port, while port 636 is used when SSL is enabled.

`ServiceUser` - The Service user that is used to connect to the LDAP System.

`ServiceUserPassword` - The password of the `ServiceUser``for logging into the LDAP system.

`UseSsl` -  Describes if a secure connection to the LDAP System is used. If this propery is enabled that the `Port` must be 636. And the `IgnoreCertificate` must be set to true. Otherwise the `IgnoreCertificate` should be set to false.


## LdapQueries Section
LDAP queries are search requests sent to an LDAP directory to retrieve specific information. They allow for searching and retrieving data about users, groups, organizational units, and other directory objects. LDAP queries can include various parameters such as filters defining specific search criteria and attributes specifying which information to return.

`Filter` - LDAP filters are expressions used in LDAP queries to specify specific search criteria. They allow for filtering search results to return only objects that meet certain properties.

`SearchBase` - The "SearchBase" in LDAP systems refers to the base distinguished name (DN) from which LDAP searches are initiated. It serves as the starting point for search operations in the directory tree. The server begins the search from this base DN and traverses the directory tree to locate objects matching the search criteria.

You can specify as many queries as needed, ensuring they match the configuration of your LDAP system.

For more information, it's recommended to explore LDAP and familiarize yourself with the various [filters](https://ldap.com/ldap-filters/) that can be utilized.

## Source Section
This section solely describes the entities that can be synchronized from the LDAP system and whether the entities can be modified in the UserProfileService. For the LDAP Connector, only user entities can be synchronized from an LDAP system. In our example above, users can be added, updated, or deleted from our system if changes occur in the LDAP system.

## Recommendation
This was just a brief introduction to configuring the synchronization system using LDAP. We assume familiarity with this protocol. If not, we recommend further reading to gain understanding.








