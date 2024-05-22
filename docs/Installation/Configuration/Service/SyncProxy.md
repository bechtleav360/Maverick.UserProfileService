# Configure SyncProxy

Here is a sample section on how to configure the sync proxy:
```json
{
  "SyncProxyConfiguration": {
    "Endpoint": "http://example.com/sync-api"
  }
}
```

`Endpoint` - Specifies the endpoint of the service that will control the Sync API. This endpoint should be set to the URL of the sync that manages the API requests. This allows for centralized control and management of the Sync API via the designated service.



