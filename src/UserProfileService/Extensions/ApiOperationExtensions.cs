using Microsoft.OpenApi.Models;
using UserProfileService.Utilities;

namespace UserProfileService.Extensions;

internal static class ApiOperationExtensions
{
    internal static Dictionary<string, object> ToPropertiesChangeDictionary(this object source)
    {
        return source.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(source));
    }

    internal static string TrimEnd(this string s, string stringToTrim)
    {
        if (stringToTrim != null && s.EndsWith(stringToTrim))
        {
            return s[..^stringToTrim.Length];
        }

        return s;
    }

    internal static void AddParameters(
        this OpenApiOperation operation,
        params OpenApiParameter[] parameters)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        if (parameters.Length == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(parameters));
        }

        IList<OpenApiParameter> newList = operation
                .Parameters?
                .Concat(parameters)
                .Distinct(new OpenApiParameterNameComparer())
                .ToList()
            ?? new List<OpenApiParameter>();

        operation.Parameters = newList;
    }
}
