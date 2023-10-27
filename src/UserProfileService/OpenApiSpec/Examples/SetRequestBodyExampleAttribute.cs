namespace UserProfileService.OpenApiSpec.Examples;

/// <summary>
///     Defines the <see cref="IExampleProvider" /> generator type of this method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class SetRequestBodyExampleAttribute : Attribute
{
    /// <summary>
    ///     The provider/generator  type that will create sample data.
    /// </summary>
    public Type GeneratorType { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="SetRequestBodyExampleAttribute" />.
    /// </summary>
    /// <param name="generatorType">The provider/generator  type that will create sample data.</param>
    public SetRequestBodyExampleAttribute(Type generatorType)
    {
        GeneratorType = generatorType;
    }
}
