namespace UserProfileService.Api.Common.Attributes;

/// <summary>
///     Adds one ore more header parameter to the OpenAPI documentation.
/// </summary>
[AttributeUsage(
    AttributeTargets.Method,
    Inherited = false)]
public class AddHeaderParametersAttribute : Attribute
{
    public string[] HeaderNames { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="AddHeaderParametersAttribute" /> with defined <see cref="HeaderNames" />.
    /// </summary>
    /// <param name="headerNames">
    ///     Refers to a header name in <see cref="WellKnownApiDetails" />.
    ///     <see cref="WellKnownApiDetails.OpenApiParameters" />.
    /// </param>
    public AddHeaderParametersAttribute(params string[] headerNames)
    {
        if (headerNames.Length == 0)
        {
            throw new ArgumentException("headerNames cannot be an empty collection.", nameof(headerNames));
        }

        HeaderNames = headerNames;
    }
}
