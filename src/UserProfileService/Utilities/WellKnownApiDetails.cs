using Microsoft.OpenApi.Models;
using UserProfileService.Api.Common.Configuration;
using UserProfileService.Configuration;

namespace UserProfileService.Utilities;

/// <summary>
///     Contains well-known api header details like header parameters of controllers.
/// </summary>
public class WellKnownApiDetails
{
    /// <summary>
    ///     Contains all well-known api parameters.
    /// </summary>
    public static Dictionary<string, OpenApiParameter> OpenApiParameters { get; }
        = new Dictionary<string, OpenApiParameter>(StringComparer.OrdinalIgnoreCase)
        {
            {
                WellKnownIdentitySettings.ImpersonateHeader, new OpenApiParameter
                {
                    Name = WellKnownIdentitySettings.ImpersonateHeader,
                    In = ParameterLocation.Header,
                    Description = "The id of the user that is used to impersonate the initiator of a command.",
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    Required = false
                }
            }
        };

    /// <summary>
    ///     Get filtered parameters.
    /// </summary>
    /// <param name="nameFilter">The name filter array. Every parameter will be returned those name matches one element of it.</param>
    /// <returns>An array of <see cref="OpenApiParameters" />.</returns>
    public static OpenApiParameter[] GetParameters(params string[] nameFilter)
    {
        return nameFilter.Where(n => OpenApiParameters.ContainsKey(n))
            .Select(n => OpenApiParameters[n])
            .ToArray();
    }
}
