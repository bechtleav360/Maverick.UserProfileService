# Service
The Service or the API-Service is the interface for you to interact with the UPS. It provides a wide range of CRUD operations to manipulate currently available entities.

## Handling Entity Operations
The Service utilizes the **Asynchronous Request-Reply Pattern**.The benefit of this pattern is that requests are handled asynchronously. Only operations that manipulate a entity are handled by this pattern.

When an entity is created, deleted, or updated, a **202 (Accepted) HTTP** response code is received, along with a location link in the header. This link allows tracking the progress of the operation. If the entity is not yet ready, a **202 (Accepted) HTTP** status code is returned. Upon completion of the operation, the location link is redirected using a **302 (Redirect) HTTP** code to the corresponding resource. This approach offers a significant advantage: the service can efficiently handle a high volume of requests simultaneously, while GET requests are processed as usual.

## Operation progess

Here we are creating a group. **When the operation is triggered, the outcome appears as follows:**
```json
{
"additionalQueryParameter": null,
"details": null,
"initiator": "",
"objectIds": [],
"operation": "CreateGroupProfile",
"correlationId": "00-6b1bd2f59cecbaf31bde3734916e75ef-0eea74be7239fe11-01",
"errorCode": 0,
"errorMessage": null,
"finished": "0001-01-01T00:00:00Z",
"id": "6978c11f-b670-4a19-a139-994f4f423c25",
"started": "2024-04-12T12:21:17.9407143Z",
"status": "Pending",
"type": "OperationTicket"
}
```
The operation has been registered in the service and will be processed shortly.

**When the operation has been processed:**

```json
{
"memberOf": [],
"members": [],
"customPropertyUrl": "https://userprofile.de/api/v2/profiles/6368956b-56b0-4878-8afc-874c2aadf521/customProperties",
"createdAt": "2024-04-12T12:21:20.7407143Z",
"displayName": "AdministrationGroup",
"externalIds": [],
"id": "6368956b-56b0-4878-8afc-874c2aadf521",
"imageUrl": "https://userprofile.de/api/v2/profiles/6368956b-56b0-4878-8afc-874c2aadf521/image",
"isMarkedForDeletion": false,
"isSystem": false,
"kind": "Group",
"name": "AdministrationGroup",
"source": "Api",
"synchronizedAt": null,
"tagUrl": "https://userprofile.de/api/v2/groups/6368956b-56b0-4878-8afc-874c2aadf521/tags",
"updatedAt": "2024-04-12T12:21:20.7407143Z",
"weight": 0
}
```

The operation has been processed, and the entity is now stored in the database. The link is now directing to the current state of the entity stored in the database.

## GET Operations
The GET operations are handled synchronously. If you need to request a large amount of data, the results may be paginated. This means that the result will contain a '**next**' link, allowing you to retrieve the next batch or page of items. This approach ensures that you don't need to request a large number of items all at once.

**Result Page for groups:**

```json
{
  "response": {
    "count": 1499,
    "nextLink": "https://userprofile.de/api/v2/groups?Limit=10&Offset=10",
    "previousLink": ""
  },
  "result": [
    {
      "createdAt": "2023-03-24T13:45:00.4246881Z",
      "displayName": "0001_Gruppe",
      "externalIds": [
        {
          "id": "a517f195-1975-4e79-8890-4c32db938cb5",
          "isConverted": false,
          "source": "Bonnea"
        }
      ]
    }
  ]
}
```
In this scenario, the '**next**' link will fetch the next 10 groups of items. The batch size can be chosen according to preference.

## Dependencies
The Service relies on the Saga Worker to perform entity creation, updating, or deletion operations. Without the Service, no manipulation operations are possible. It's crucial to avoid this common error by ensuring the availability and proper functioning of the Saga Worker.










