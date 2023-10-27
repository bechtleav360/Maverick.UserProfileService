using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UserProfileService.OpenApiSpec.Examples;

/// <summary>
///     An <see cref="IOperationFilter" /> that will set example data of the body of a request.
/// </summary>
public class RequestBodyExampleGeneratorFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        SetRequestBodyExampleAttribute exampleAttribute =
            context.ApiDescription.CustomAttributes().OfType<SetRequestBodyExampleAttribute>().FirstOrDefault();

        if (exampleAttribute?.GeneratorType == null)
        {
            return;
        }

        if (!typeof(IExampleProvider).IsAssignableFrom(exampleAttribute.GeneratorType))
        {
            throw new NotSupportedException(
                $"This OpenAPI example generator cannot create us provided generator type, because it does not inherit from {nameof(IExampleProvider)}.");
        }

        if (!operation.RequestBody.Content.TryGetValue("application/json", out OpenApiMediaType body))
        {
            throw new NotSupportedException(
                "This OpenAPI example generator only supports generating JSON body examples.");
        }

        var generator = (IExampleProvider)Activator.CreateInstance(exampleAttribute.GeneratorType);

        if (generator == null)
        {
            throw new TypeLoadException($"Could not create an instance of type {exampleAttribute.GeneratorType}");
        }

        object sampleData = generator.GetExample();

        body.Example = new OpenApiString(JsonConvert.SerializeObject(sampleData, Formatting.Indented));
    }
}
