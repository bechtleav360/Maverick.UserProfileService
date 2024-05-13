# Base-Path Handling

Additionally, you can opt-in to automatically handle routing base-paths.  
This is useful to make your service available behind a reverse-proxy or similar setup.
To do this, you need to use this method:

- Full Path: `UserProfileService.Hosting.ApplicationBuilderExtensions.UseReverseProxyPathBases`
- As extension: `appBuilder.UseReverseProxyPathBases(Configuration)`

This extension will look for these settings, and configure your application accordingly:
```json
{
    "Routing": {
        "PathBase": "",
        "DiscardResponsePathBase": ""
    }
}
```

When setting `Routing:PathBase`, your application will accept requests to all of your usual endpoints (`/api/foo`), but also those that have the configured Prefix (`/service/api/foo`).  
This ensures compatibility with or without reverse-proxy, and pushes consumers of your API to use endpoints with their respective `PathBase`.  
You can check `HttpContext.Request.PathBase` to see if the current request was handled with or without base-path.  
If `Routing:PathBase` is set, all redirects will be relative to the configured `PathBase`, even for those that were handled without it.

- `/api/foo` redirecting to the `api/bar`-endpoint will instead be redirected to `/service/api/bar`
- `/service/api/foo` redirecting to the `api/bar`-endpoint will also redirect to `/service/api/bar`

When setting `Routing:DiscardResponsePathBase` your application will behave as if `Routing:PathBase` was set, but instead of changing all redirects to use the configured prefix, it will be removed from all redirects.

- `/api/foo` redirecting to the `api/bar`-endpoint will still be redirected to `/api/bar`
- `/service/api/foo` redirecting to the `api/bar`-endpoint will instead redirect to `/api/bar`

## Technical notes to Base-Path handling

There are three components to the Base-Path handling, one of which is hidden.
- `PathBase`
  - Endpoints will also be bound with this as prefix
- `ResponsePrefix`
  - Ensures outgoing responses use this prefix in their path
- `DiscardResponsePathBase`
  - Removes this path from the start of all outgoing responses

For simplicity's sake we chose to omit `RequestPrefix` and always use it together with `PathBase`, which leaves only Options 0 (no settings), 2 (`PathBase`), and 4 (`DiscardResponsePathBase`).

Using these three settings in different configurations results in these Use-Cases:

## Option 0 - Fallback
- No option used
- Endpoints bound using `Base=/`
- Redirects always use `PathBase=/`

Without any configuration services will use **Option-0**, they will only be bound and respond to the endpoints defined in the code.  
To use this configuration each service needs its own hostname or ip to respond to.

## Option 1 - Not Supported
- `PathBase=/service`
- Endpoints bound using `Base=/` and `Base=/service`
- Redirects keep the Base they originally arrived with

Basically the same as **Option-2**, but doesn't show or encourage use of endpoints with a defined Base.

## Option 2 - Target-Default
- `PathBase=/service` + `RequestPrefix=/service`
- Endpoints bound using `Base=/` and `Base=/service`
- Redirects will always use `Base=/service`

Basically the same as **Options-1**, but pushes consumers to use endpoints with a defined Base.  
We chose to implicitly use **Option-2** and hide **Option-1**, to hopefully reduce bugs related to path-mapped services.

## Option 3 - Not Supported
- `RequestPrefix=/service`
- Endpoints bound using `Base=/`
- Redirects will always use `Base=/service`

While writing this extension we could find no real use-case for **Option-3**.

## Option 4 - Supported just in case
- `DiscardResponsePathBase=/service` + (`RequestPrefix=""` & `PathBase=""`)
  - Using `DiscardResponsePathBase` with any other option did not lead to usable results
- Endpoints bound using `Base=/` and `Base=/service`
- Redirects will always use `Base=/`

While we don't expect usage of **Option-4**, we can see scenarios where this configuration makes sense, so we added it to this extension.  
Using `DiscardResponsePathBase` with any other setting did not produce usable results, so we chose to treat its use with other settings as an error.


## Tracing

Basic configuration requires you to set at least a service name. The service name will be displayed in the trace graph and used to correlate the logs
If you also provide an OtlpEndpoint URI the OtlpExporter will be setup to send traces via GRPC in the OTLP format to the provided endpoint