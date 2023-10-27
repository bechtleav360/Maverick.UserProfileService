using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UserProfileService.FilterHelper;

public class QueryFilterOperationFilter : IOperationFilter
{
    private static bool TryGetParameter<TParam>(
        IEnumerable<TParam> parameters,
        Func<TParam, bool> filter,
        out TParam parameter) where TParam : class
    {
        parameter = parameters?.FirstOrDefault(filter);

        return parameter != null;
    }

    public void Apply(OpenApiOperation schema, OperationFilterContext context)
    {
        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (schema.Parameters.Any(x => x.Name is "Filter.Definition" or "Filter.CombinedBy"))
        {
            int index = schema.Parameters.IndexOf(
                schema.Parameters.First(
                    x =>
                        x.Name is "Filter.Definition" or "Filter.CombinedBy"));

            schema.Parameters.Insert(
                index,
                new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = nameof(Filter),
                    Description =
                        "(Or,[{\"name\",==,[\"test1\",\"test2\"],And},{\"name\",==,[\"test1\",\"test2\"],And}])",
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    Required = false
                });

            schema.Parameters.Remove(schema.Parameters.FirstOrDefault(x => x.Name == "Filter.Definition"));
            schema.Parameters.Remove(schema.Parameters.FirstOrDefault(x => x.Name == "Filter.CombinedBy"));
        }

        if (TryGetParameter(
                schema.Parameters,
                p => p.Name == nameof(QueryObjectBase.Offset),
                out OpenApiParameter offsetParam))
        {
            offsetParam.Description = "The amount of items to skip before starting to collect the result set.";
            offsetParam.Schema.Default ??= new OpenApiInteger(0);
        }

        if (TryGetParameter(
                schema.Parameters,
                p => p.Name == nameof(QueryObjectBase.Limit),
                out OpenApiParameter limitParam))
        {
            limitParam.Description = "The amount of items to return.";
            limitParam.Schema.Default ??= new OpenApiInteger(50);
        }

        if (TryGetParameter(
                schema.Parameters,
                p => p.Name == nameof(QueryObjectBase.OrderedBy),
                out OpenApiParameter orderedByParam))
        {
            orderedByParam.Description ??= "Defines the name of the property to be sorted by.";
        }

        if (TryGetParameter(
                schema.Parameters,
                p => p.Name == nameof(QueryObjectBase.SortOrder),
                out OpenApiParameter sortOrderParam))
        {
            sortOrderParam.Description ??= "Defines the sort direction.";
        }
    }
}
