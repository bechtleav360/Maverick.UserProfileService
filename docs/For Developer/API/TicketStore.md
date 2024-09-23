# Tickestore
The ticket store manages requests to the UPS API and oversees their progress. It specifically handles create, delete, and update operations. For instance, when an entity is created, a corresponding ticket is generated in the Ticket Store under `Service_tickets`. Here's an example of how a ticket that has been created might appear:

??? abstract "Ticket example output"
    ```json
    {
      "additionalQueryParameter": null,
      "details": null,
      "initiator": "",
      "objectIds": [],
      "operation": "CreateGroupProfile",
      "correlationId": "00-889bac6d8676c2135354f2d66d9e2dc3-8d76f793dc0310f7-01",
      "errorCode": 0,
      "errorMessage": null,
      "finished": "0001-01-01T00:00:00Z",
      "id": "960a0bf3-cb3c-4d44-88c7-f8f243e15838",
      "started": "2024-04-25T11:51:38.3868863Z",
      "status": "Pending",
      "type": "OperationTicket"
    }
    ```

- `additionalQueryParameter`: Sometimes needed for special query parameters (for redirecting to specific entities).

- `details`: Specifies the details of the problem that occurred.

- `initiator`: Specifies the ID of the user who initiated the operation. Can be <c>null</c> if the initiator is unknown.

- `objectIds`: The IDs of the objects that will be processed.

- `operation`: The operation that is currently being processed.

- `correlationId`: The correlation ID for the request. This ID is needed to trace the operation if an error occurs.

- `errorCode`: A code indicating the error that occurred. It will be 0 if none occurred.

- `errorMessage`: A message describing the error that occurred. It will be <c>null</c> if none occurred.

- `finished`: The timestamp when the ticket was finished.
 
- `id`: The ID of the ticket.
 
- `started`: The timestamp when the ticket was started.

- `status`: The status of the current ticket.
 
- `type`: The type of the ticket.

When the ticket is completed without any errors, the `status` property changes to `completed`, and `finished` receives the timestamp when the operation was finished.

## ErrorHandling
Typically, most operations are processed without errors. However, as with most systems, errors can occur from time to time. When an error occurs, the operation is not processed, and the manipulation with the entity does not take place. The ticket then contains information about the type of error that occurred, allowing you to investigate it further. An error might look like this:

??? failure "Ticket error handling example output"
    ```json
      {
        "ObjectIds": [
          "8c63141d-109b-47d0-ab06-f57b9833c481"
        ],
        "Operation": "CreateCustomPropertiesForProfile",
        "Details": {
          "Type": null,
          "Title": "Unable to complete the operation",
          "Status": 400,
          "Detail": "Validation failed.",
          "Instance": null,
          "Extensions": {
            "StatusCode": "BadRequest",
            "ValidationResults": [
              {
                "Member": "ResourceId",
                "ErrorMessage": 
                "Profile with id '8c63141d-109b-47d0-ab06-f57b9833c481' and 
                kind 'Unknown' does not exists.",
                "AdditionalInformation": null
              }
            ]
          }
        },
        "Initiator": "",
        "AdditionalQueryParameter": "(Or,[{\"Key\",==,[\"eVorlagenFavorites\"],Or}])",
        "Id": "abfb5d10-3ad6-45a3-9f55-3c52d58dff9f",
        "CorrelationId": "00-9501e53c1c12b75e777dea15d86359cb-8599a7b91e3d1ad1-00",
        "Type": "OperationTicket",
        "Status": "Failure",
        "Started": "2023-08-22T12:06:18.6140738Z",
        "Finished": "2023-08-22T12:06:18.7112435Z",
        "ErrorCode": 400,
        "ErrorMessage": "Validation failed."
      }
    ```
In this case, the validation failed because we attempted to add a custom property to a profile with an unknown type.