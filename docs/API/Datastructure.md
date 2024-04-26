# Datastructure
The UPS data is currently stored in [ArangoDB](http://arangodb.com), a graph database that supports graph queries. Data is stored as JSON strings, and ArangoDB offers two types of data structures for storage: Collections and EdgeCollections. Collections store JSON documents. Each document can have varying fields and structures. EdgeCollections are a specialized data store in ArangoDB used for storing relationships between documents in a graph. They enable the definition of directed or undirected edges between nodes.

## Used Collections / EdgeCollections
The service utilizes all collections with the suffix *Service_*. All data accessible via the API is stored in these collections. To comprehend the collections and their models, refer to the Data Model and managed entities documentation. Further details about these collections will be provided in the documentation.


### Service_clientSettingsQuery
The client settings are also represented as key-value pairs. In addition to custom properties, client settings can be inherited. They can only be used for users and groups. Groups are containers that can be assigned to each other, allowing you to create a hierarchy. Let's first take a look at this example.
![ClientSettings](.attachment/ClientSettings.svg)

The example illustrates groups that are assigned to each other, with users being assigned to these groups. Both entities contain client settings, all of which are inherited in this example. The group `Bechtle` has client settings that define the `IDE` used by users in the company. Similarly, the group 'SH Bonn' also has client settings with the same key (`IDE`). Consequently, the client settings from the 'Bechtle' group are overwritten. As a result, the user `Andreas Minz`, inheriting client settings from the 'SH Bonn' group, will use the Vim editor as their IDE. 

In general, client settings are combined when there are multiple instances. Therefore, both the IDE and the OS client settings will be merged. For example, the user 'Max Mustermann' will use Linux as his  OS, as the client settings from the 'AVS' group will overwrite others. However, Max inherits the IDE settings from the 'SH-Bonn' group. On the other hand, the user 'Sandy Musterfrau' will use Windows 11 as her OS because she inherits it from the 'AVS' group. Nonetheless, her IDE setting will be overridden by her specific client settings, so she uses Visual Studio as her IDE.

In summary, inherited client settings are sourced from higher levels and can be overwritten. When multiple client settings exist, they are combined.
 
The client settings may appear like this:
```json
{
  "IsInherited": true,
  "Kind": "User",
  "ProfileId": "b2aba1cc-0d86-4056-9358-594a560baae9",
  "SettingsKey": "OS",
  "UpdatedAt": "0001-01-01T00:00:00Z",
  "Value": {
    "data": ["Windows"]
  }
}
```

- `IsInherited`: Indicates whether the client setting is inherited.
- `Kind`: Denotes the type of profile associated with the client setting.
- `ProfileId`: Represents the Id of the profile associated with the client setting.
- `SettingsKey`: Identifies the key of the client setting.
- `UpdatedAt`: Specifies when the client setting was last updated.
- `Value`: Represents the value of the client setting.


### Service_customPropertiesQuery
The collections store custom properties for specific users, group or an organization. A custom property consists of a key-value pair, which can be used to add additional information to an entity. The data model for this can look like following:

```json
{
  "Related": "Service_profilesQuery/6ad843e6-b3cd-457f-b205-33a5813c3c53",
  "Key": "FavoriteColor",
  "Value": "Green"
}
```
- `Related`- The profile the custom property is added to (user, group or organization)

- `Key` - The key of the custom property

- `Value` - The value of the custom property

### Service_pathTree
The path tree contains every profile that has assignments to another profile. This means that for a single profile, we have all the assignments necessary to retrieve its members.
Nowadays, we are not using the Path-Tree. It is used to compute the Path of an profile. An Example how an item can look like:

```json
{
  "RelatedProfileId": "b5f59454-be26-4035-b1d8-5cfacbc518a4",
  "ObjectId": "b5f59454-be26-4035-b1d8-5cfacbc518a4",
  "Tags": []
}
```

- `RelatedProfileId` - The Id of the profile for which the member will be retrieved.

- `ObjectId` - The object to which the item is assigned.

- `Tags` - The list of tags associated with the profile.

If the `RelatedProfileId` and `ObjectId` have the same value, it represents the profile itself. If both values differ, it means that the profile with the `RelatedProfileId` has a member with an ID shown in the `ObjectId`. Both documents, whether their Ids are the same or different, are connected via an edge, with the connection stored in the `Service_pathTreeEdges` collection.

### Service_pathTreeEdges
The collection stores the edges that connect the pathTree documents. Each edge collection contains a `_to` and `_from` attribute that are fixed. The `From` and `To` attributes contain the exact IDs of a pathTree. Each document has a `_key` property that uniquely identifies a document within the collection. The `_id` is also a property specific to ArangoDB; it completely identifies a document in a database. Typically, it consists of the collection name and the `_key` property, such as "**Service_pathTree/10de9702-990a-4cec-91b7-91e622a23ba1**". In the body of the edge, we have conditions, also known as ranged conditions, that will be explained here. The _from and _to fields connect two path documents. As an example, we list the two pathTree documents and the edge. The IDs are typically GUIDs, but for simplicity, we chose easy-to-obtain numbers:

#### PathTree-Documents

```json
{
  "RelatedProfileId": "123",
  "ObjectId": "123",
  "Tags": [],
  "_id:":"Service_pathTree/45",
  "_key":"45"
}

{
  "RelatedProfileId": "123",
  "ObjectId": "456",
  "Tags": [],
  "_id:":"Service_pathTree/46",
  "_key":"46"
}
```

#### Edge Connection the documents 
- `_from` - Service_pathTree/45

- `_to` - Service_pathTree/46


```json
{
  "Conditions": [
    {
      "Start": null,
      "End": null
    }
  ],
  "RelatedProfileId": "123"
}
```
So, the two documents `Service_pathTree/45` and `Service_pathTree/46` are now connected via the edge. Through a graph query across these collections, we can determine which profile has assignments. This can be used to retrieve all users who have active assignments to a group or a function.

### Service_profilesQuery

The `Service_profiles` collection stores profiles along with their relationships to other profiles. Within this collection, users, organizations, and groups are stored. An example user entry might look like this:

```json

{
  "FunctionalAccessRights": null,
  "MemberOf": [],
  "SecurityAssignments": [],
  "Paths": [],
  "Tags": [],
  "CustomPropertyUrl": null,
  "UserName": "andreas.mustermann",
  "FirstName": "Andreas",
  "LastName": "Mustermann",
  "Email": "andreas.mustermann@gmx.de",
  "ImageUrl": null,
  "UserStatus": null,
  "Id": "6d78353b-5ff6-4314-aae6-50700afa7295",
  "Name": "Andreas Mustermann",
  "DisplayName": "Andreas Hinz",
  "Kind": "User",
  "CreatedAt": "2023-01-02T19:15:09.9954893Z",
  "UpdatedAt": "2024-02-29T10:51:12.7416251Z",
  "TagUrl": null,
  "SynchronizedAt": null,
  "Source": "Ldap",
  "Domain": "ad.example.org",
  "ExternalIds": [
    {
      "Id": "S-1-5-21-966539559-2079964620-3194842515-3311",
      "IsConverted": false,
      "Source": "Ldap"
    }
  ]
}
```
Most attributes are self-explanatory, such as `UserName`, `FirstName`, `LastName`, `Email`, `Id`, `Name`, `DisplayName`, `CreatedAt`, and `UpdatedAt`.

- `MemberOf`: Represents the groups or organizations to which the profile belongs. For example, a user can be a member of a group.
  
- `SecurityAssignments`: Refers to the functions or roles assigned to the profile for security purposes.
  
- `Paths`: Contains the path to all active assignments associated with the profile.
  
- `Tags`: Represents the tags assigned to the profile.
  
- `CustomPropertyUrl`: Provides a link where custom properties associated with the profile can be found.
  
- `ImageUrl`: Stores a link to where the user's image is stored, although this functionality is currently not implemented.
  
- `UserStatus`: Indicates the status for a specific profile.
  
- `Kind`: Specifies the type of profile, such as user, group, or organization.
  
- `Source`: Defines the source from which the profile was created, such as an LDAP source in this case.
  
- `Domain`: Represents the domain of the user, particularly useful when synchronizing users from LDAP.
  
- `ExternalIds`: Holds the external IDs from an external system, such as LDAP. The `IsConverted` parameter is optional and indicates whether the profile has been converted. A profile may have multiple external IDs if stored in multiple sources.

This entities can be retrieved via Endpoints that the user profile service provides.

### Service_rolesFunctionsQuery
This collection stores the roles and functions. A function can be looked like this:

```json
{
  "Conditions": null,
  "LinkedProfiles": [],
  "Id": "65525461-aad5-4a79-95df-b64ce54fe978",
  "Name": "Z20 Administration",
  "Type": "Function",
  "OrganizationId": "629c6a09-7c5f-40bd-b1aa-a31667c26511",
  "Organization": {
      ...
  },
  "Role": {
     ...
  },
  "RoleId": "83fdde77-a78c-4b04-80c6-2221991cf909",
  "CreatedAt": "2021-10-04T09:41:53.0954287Z",
  "UpdatedAt": "2021-10-04T09:41:53.0954287Z",
  "SynchronizedAt": null,
  "Source": "Api",
  "ExternalIds": []
}
```

A function consists of a role and an organization, although the organization is not shown in the JSON.

- `LinkedProfiles`: Represents the linked profiles associated with the function.
- `Type`: Indicates the type of entity stored in the collection, with possible values being `Function` or `Role`.
- `Source`: Specifies the source from which the function was created.


### Service_tagsQuery
This collections store tags that are associated with an entity. Tags can be look like this:

```json
{
  "Id": "916f7364-bf20-4086-84bd-34033d3a5011",
  "Name": "Green",
  "Type": "Custom"
}
```

- `Id`: The ID of the tag.
- `Name`: The name of the tag.
- `Type`: Specifies the type of the tags, which can be:
    - `Security`: Used for security-related tags that determine permissions.
    - `Custom`: Used for custom tags that have no functional impact but are for marking purposes.
    - `FunctionalAccessRights`: Primarily for internal use and not critical for functionality.
    - `Color`: Stores color information, typically inheritable and stored as tags.


### Service_tickets
The collection stores the service ticket when an entity is created, updated, or deleted. This will be explained [here]().

  
