# Configure SwaggerUI

Here is a sample section on how to configure the SwaggerUI:

```json
{
  "Features": {
    "UseSwaggerUI": true
  }
}
```

`UseSwaggerUI` - The UseSwaggerUI setting controls the visibility of the Swagger UI. When enabled, users can access and interact with the API documentation through the Swagger UI. Conversely, when disabled, the Swagger UI is not accessible.

**Development Mode Only**: It's important to note that the Swagger UI functionality is only available in **development** mode. This means that it will be active during development and testing phases but is disabled or inaccessible in **production** environments.