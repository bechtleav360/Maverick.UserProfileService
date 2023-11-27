using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserProfileService.FilterHelper;
using UserProfileService.OpenApiSpec.Examples;

namespace UserProfileService.Swagger;

public class MaverickSwagger : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc(
            "v2",
            new OpenApiInfo
            {
                Version = "v2",
                Title = "UserProfileService v2 API",
                Description =
                    "Maverick user profile service that will manage user and security related information.",
                Contact = new OpenApiContact
                          {
                              Name = @"A/V 360° Solutions",
                              Email = string.Empty
                          }
            });

        options.OperationFilter<QueryFilterOperationFilter>();
        options.OperationFilter<CustomHeaderOperationFilter>();
        options.OperationFilter<AddDefaultValues>();
        options.OperationFilter<RequestBodyExampleGeneratorFilter>();
        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath, true);

        var jwtSecurityScheme = new OpenApiSecurityScheme
                                {
                                    Name = "JWT access token authentication",
                                    Description = "Enter bearer token **_only_**",
                                    In = ParameterLocation.Header,
                                    Type = SecuritySchemeType.Http,
                                    Scheme = "bearer",
                                    BearerFormat = "JWT",
                                    Reference = new OpenApiReference
                                                {
                                                    Id = JwtBearerDefaults.AuthenticationScheme,
                                                    Type = ReferenceType.SecurityScheme
                                                }
                                };

        options.AddSecurityDefinition(
            jwtSecurityScheme.Reference.Id,
            jwtSecurityScheme);

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, new List<string>() }
            });

        options.MapType<JsonArray>(
            () => new OpenApiSchema
                  {
                      Type = "array",
                      Default = new OpenApiArray(),
                      Description = "Array as JSON text (wrapped by '[', ']')",
                      Items = new OpenApiSchema
                              {
                                  Type = "object",
                                  Default = new OpenApiObject
                                            {
                                                { "prop1", new OpenApiString("value1") },
                                                { "prop2", new OpenApiInteger(4711) }
                                            }
                              }
                  });
    }
}
