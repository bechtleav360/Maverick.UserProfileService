using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserProfileService.Api.Common.Attributes;
using UserProfileService.Attributes;
using UserProfileService.Extensions;
using UserProfileService.Utilities;

namespace UserProfileService.FilterHelper;

/// <summary>
///     An implementation of <see cref="IOperationFilter" /> that adds an OpenAPI documentation of all UPSv2 custom headers
///     that are not mentioned in controllers (like X-UserId).
/// </summary>
public class CustomHeaderOperationFilter : IOperationFilter
{
    /// <inheritdoc cref="IOperationFilter" />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.GetCustomAttribute(typeof(AddHeaderParametersAttribute)) is
            not AddHeaderParametersAttribute addHeader)
        {
            return;
        }

        operation.AddParameters(WellKnownApiDetails.GetParameters(addHeader.HeaderNames));
    }
}
