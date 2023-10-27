using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserProfileService.Attributes;

namespace UserProfileService.FilterHelper;

public class AddDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation schema, OperationFilterContext context)
    {
        if (schema.Parameters == null)
        {
            return;
        }

        List<SwaggerDefaultValueAttribute> defaultValueAttributes = context
            .MethodInfo
            .GetCustomAttributes<
                SwaggerDefaultValueAttribute>()
            .ToList();

        if (defaultValueAttributes.Count == 0)
        {
            return;
        }

        foreach (OpenApiParameter param in schema.Parameters)
        {
            SwaggerDefaultValueAttribute defaultValueAttribute =
                defaultValueAttributes.FirstOrDefault(p => p.ParameterName == param.Name);

            if (defaultValueAttribute?.Value is string s)
            {
                param.Schema.Default = new OpenApiString(s);

                continue;
            }

            if (defaultValueAttribute?.Value is int i)
            {
                param.Schema.Default = new OpenApiInteger(i);

                continue;
            }

            if (defaultValueAttribute?.Value is double d)
            {
                param.Schema.Default = new OpenApiDouble(d);

                continue;
            }

            if (defaultValueAttribute?.Value is bool b)
            {
                param.Schema.Default = new OpenApiBoolean(b);
            }
        }
    }
}
