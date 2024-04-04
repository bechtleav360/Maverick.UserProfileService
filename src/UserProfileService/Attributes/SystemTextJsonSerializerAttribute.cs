using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace UserProfileService.Attributes;

/// <summary>
///     Set the serializer to system text json as serializer and serialized  the result with in.
/// </summary>
public class SystemTextJsonSerializerAttribute : ActionFilterAttribute
{
    /// <inheritdoc cref="ActionFilterAttribute" />
    public override void OnActionExecuted(ActionExecutedContext ctx)
    {
        if (ctx.Result is AcceptedAtRouteResult)
        {
            return;
        }

        if (ctx.Result is not ObjectResult objectResult)
        {
            return;
        }

        objectResult.Formatters.Add(
            new SystemTextJsonOutputFormatter(new JsonSerializerOptions(JsonSerializerDefaults.General)
                                              {
                                                  TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                                              }));
    }
}
