# Configure Sync Validation

Here is a brief section on how the validation can be configured

??? abstract "Sync validation example configuration"
    ```json
    {
      "Validation": {
        "Commands": {
          "External": {
            "profile-deleted": false
          }
        },
        "Internal": {
          "User": {
            "DuplicateEmailAllowed": false
          }
        }
      }
    }
    ```

`profile-deleted` - Specifies whether the profile-deleted messages will be validated by an external system or not.

`DuplicateEmailAllowed` - Indicates whether a duplicate check is performed for email addresses.