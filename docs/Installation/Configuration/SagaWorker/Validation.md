# Configure SagWorker Validation
Here is a brief section on how the validation can be configured:

??? abstract "SagaWorker example validation configuration"
    ```json
    {
      "Validation": {
        "Commands": {
          "External": {
            "profile-deleted": false
          }
        },
        "Internal": {
          "Function": {
            "DuplicateAllowed": false
          },
          "Group": {
            "Name": {
              "Duplicate": false,
              "IgnoreCase": true,
              "Regex": "^[a-zA-Z0-9ÄÖÜäöüß_\\]\\[\\-\\.\\\\ @]+$"
            }
          },
          "User": {
            "DuplicateEmailAllowed": false
          }
        }
      }
    }
    ```

`profile-deleted` - Specifies whether the profile-deleted messages will be validated by an external system or not.

`DuplicateAllowed` - Specifies if duplicate functions are permissible.

`Duplicate` - Determines whether a duplicate check is performed for the (display) name.

`IgnoreCase` - Specifies if the duplicate check should be case-insensitive. This is applicable only if `DuplicateAllowed` is set to false.

`Regex` - Defines the regular expression used to validate the name and displayName.

`DuplicateEmailAllowed` - Indicates whether a duplicate check is performed for email addresses.