using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UserProfileService.Utilities;

/// <summary>
///     Implementation of <see cref="IModelBinder" /> that binds the whole request body to a <see cref="JsonNode" />
///     parameter.
/// </summary>
public class JsonNodeModelBinder : IModelBinder
{
    private static string GetTypedString(Type modelType)
    {
        if (modelType == typeof(JsonArray))
        {
            return " array";
        }

        if (modelType == typeof(JsonObject))
        {
            return " document";
        }

        if (modelType == typeof(JsonValue))
        {
            return " value";
        }

        return string.Empty;
    }

    /// <inheritdoc />
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        await Task.Yield();

        if (bindingContext.ModelType != typeof(JsonNode)
            && bindingContext.ModelType != typeof(JsonArray)
            && bindingContext.ModelType != typeof(JsonObject)
            && bindingContext.ModelType != typeof(JsonValue))
        {
            throw new InvalidCastException($"Cannot bind plain to type {bindingContext.ModelType}");
        }

        bindingContext.HttpContext.Request.EnableBuffering();

        try
        {
            using var bufferStream = new MemoryStream();
            await bindingContext.HttpContext.Request.Body.CopyToAsync(bufferStream);
            byte[] jsonBytes = bufferStream.ToArray();

            JsonNode parsed = JsonNode.Parse(jsonBytes);

            if (parsed == null)
            {
                bindingContext.ModelState.AddModelError(
                    "JsonBodyModelBinder",
                    "Could not bind request body to a JSON document");

                bindingContext.Result = ModelBindingResult.Failed();

                return;
            }

            if (bindingContext.ModelType != parsed.GetType())
            {
                bindingContext.ModelState.AddModelError(
                    "Invalid JSON body",
                    $"The JSON body is not of a JSON{GetTypedString(bindingContext.ModelType)} as expected.");

                bindingContext.Result = ModelBindingResult.Failed();
            }

            bindingContext.Result = ModelBindingResult.Success(parsed);
        }
        catch (JsonException jsonExc)
        {
            bindingContext.ModelState.AddModelError("Invalid JSON body", jsonExc.Message);
            bindingContext.Result = ModelBindingResult.Failed();
        }
        catch (Exception exc)
        {
            bindingContext.ModelState.AddModelError("JsonBodyModelBinder", exc, bindingContext.ModelMetadata);
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}
