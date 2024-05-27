# Configure Seeding Service

The seeding service is responsible for seeding configurations at startup. It can seed the following entities:

 * Functions
 * Groups
 * Organizations
 * Roles
 * Users 

A brief section to create a User at the start looks like this:

```json
{
  "SeedingService": {
    "Users": {
      "f2e51ef1-57bc-4c11-931f-079b4303d657": {
        "DisplayName": "John Doe",
        "Email": "johndoe@example.com",
        "ExternalIds": [
          {
            "Id": "f2e51ef1-57bc-4c11-931f-079b4303d657",
            "Source": "SeedingService",
            "IsConverted": false
          }
        ],
        "FirstName": "John",
        "LastName": "Doe",
        "Name": "John Doe",
        "UserName": "johnd"
      }
    },
    "Roles": {
      "fa6ee24b-0fc4-4632-9c0d-5b943d8a8ac8": {
        "ExternalIds": [
          {
            "Id": "fa6ee24b-0fc4-4632-9c0d-5b943d8a8ac8",
            "Source": "SeedingService",
            "IsConverted": false
          }
        ],
        "DeniedPermissions": [
          "Delete",
          "Edit"
        ],
        "Description": "This role provides limited access to certain features.",
        "IsSystem": false,
        "Name": "LimitedAccessRole",
        "Permissions": [
          "Read",
          "Write"
        ]
      }
    },
    "Groups": {
      "a6ee24b0fc446329c0d5b943d8a8ac8": {
        "DisplayName": "Marketing Team",
        "ExternalIds": [
          {
            "Id": "fa6ee24b-0fc4-4632-9c0d-5b943d8a8ac8",
            "Source": "SeedingService",
            "IsConverted": false
          }
        ],
        "Name": "Marketing",
        "Weight": 2.0
      }
    },
    "Organizations": {
      "8a5fd2b1-6e17-4c35-9314-7d13f67f3278": {
        "DisplayName": "Acme Corporation",
        "ExternalIds": [
          {
            "Id": "8a5fd2b1-6e17-4c35-9314-7d13f67f3278",
            "Source": "SeedingService",
            "IsConverted": false
          }
        ],
        "IsSystem": false,
        "Name": "Acme",
        "Weight": 2.0
      }
    },
    "Functions": {
      "d6d78a90-e5c2-47fb-ba82-71f3cb5f4dc2": {
        "ExternalIds": [
          {
            "Id": "d6d78a90-e5c2-47fb-ba82-71f3cb5f4dc2",
            "Source": "SeedingService",
            "IsConverted": false
          }
        ],
        "Name": "John Doe",
        "OrganizationId": "8a5fd2b1-6e17-4c35-9314-7d13f67f3278",
        "RoleId": "fa6ee24b-0fc4-4632-9c0d-5b943d8a8ac8"
      }
    }
  },
  "Disabled": false
}
```
`Users` - This describes the users that need to be created. It is represented as a dictionary. Each user is identified by a unique ID which serves as the key, followed by the user's properties that need to be filled out.

`Roles` - This describes the roles that need to be created. It is represented as a dictionary. Each role is identified by a unique ID which serves as the key, followed by the roles's properties that need to be filled out.

`Groups` - This describes the groups that need to be created. It is represented as a dictionary. Each group is identified by a unique ID which serves as the key, followed by the group's properties that need to be filled out.

`Organizations` - This describes the organizations that need to be created. It is represented as a dictionary. Each organization is identified by a unique ID which serves as the key, followed by the organizations's properties that need to be filled out.

`Functions` - This describes the functions that need to be created. It is represented as a dictionary. Each functions is identified by a unique ID which serves as the key, followed by the functions's properties that need to be filled out. 

**Please Note**: The function consists of an organization and a role ID. The role and organization ID must be present; otherwise, the function won't be created. "Present" means that the role and organization object must already exist or be defined in the seeding service. If it's not present, the function won't be created.

`Disabled` - Indicates whether the seeding service should be disabled. If the seeding service is disabled, none of the creation objects should be configured. The configuration can than look like this:

```json
{
  "Seeding": {
    "Disabled": true
  }
}
```

## Notes
The seeding service checks every time the saga worker is started whether the seeding objects already exist. If they do not exist, they will be created; otherwise, they will not be created. This is relevant when the seeding service has objects to seed.


