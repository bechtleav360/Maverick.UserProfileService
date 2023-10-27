namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Identifies a class as cleanup configuration for a specified provider.
/// </summary>
public interface ICleanupProviderConfiguration
{
    /// <summary>
    ///     Defines the name of the provider this configuration is valid for.
    /// </summary>
    string ValidFor { get; }
}
