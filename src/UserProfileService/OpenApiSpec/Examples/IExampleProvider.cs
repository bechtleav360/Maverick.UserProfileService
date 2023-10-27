namespace UserProfileService.OpenApiSpec.Examples;

/// <summary>
///     Generates sample data to be used in <see cref="RequestBodyExampleGeneratorFilter" /> to modify OpenAPI
///     specification.
/// </summary>
public interface IExampleProvider
{
    /// <summary>
    ///     Generates sample data.
    /// </summary>
    /// <returns>Sample data as an object.</returns>
    object GetExample();
}
