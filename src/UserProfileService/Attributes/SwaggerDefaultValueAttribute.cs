namespace UserProfileService.Attributes;

/// <summary>
///     Defines the default value to be passed to OpenAPI definition.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerDefaultValueAttribute : Attribute
{
    /// <summary>
    ///     The name of the parameter whose default value should be set.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    ///     The default value to be set.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="SwaggerDefaultValueAttribute" />.
    /// </summary>
    /// <param name="parameterName">The name of the parameter whose default value should be set.</param>
    /// <param name="value">The default value to be set.</param>
    public SwaggerDefaultValueAttribute(string parameterName, object value)
    {
        ParameterName = parameterName;
        Value = value;
    }
}
