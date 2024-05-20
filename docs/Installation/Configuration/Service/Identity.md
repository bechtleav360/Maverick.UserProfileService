# Configure Identity

Here is a sample section on how to configure the identity settings:

```json
{
  "IdentitySettings": {
    "ApiName": "MyApi",
    "ApiSecret": "SuperSecretKey",
    "Authority": "https://identity.example.com",
    "EnableAnonymousImpersonation": false,
    "EnableAuthorization": true,
    "EnableCaching": true,
    "RequireHttpsMetadata": true
  }
}
```

`ApiName` - The name of the API, typically used to identify the API within your identity provider.

`ApiSecret` - The secret key associated with the API. This key is used for authentication and should be kept confidential.

`Authority` - The URL of the identity provider. This endpoint handles authentication and authorization.

`EnableAnonymousImpersonation` - Determines whether anonymous impersonation is enabled. When enabled, users can impersonate anonymous identities.

`EnableAuthorization`- Specifies whether authorization is enabled. If this option is disabled, the API will not enforce authorization rules.

`EnableCaching` - Indicates whether caching is enabled. When enabled, identity-related data is cached to improve performance.

`RequireHttpsMetadata`- Specifies whether HTTPS is required for retrieving metadata from the identity provider. If this option is disabled, HTTP is also allowed.

## Notes
- Only the `user/me` endpoint is secured in the service
- Ensure that the ApiSecret is stored securely and not exposed in public repositories.
- It is recommended to set RequireHttpsMetadata to true in production environments to ensure secure communication with the identity provider.
- The Authority URL must be correctly configured to point to your identity provider to ensure proper authentication and authorization.