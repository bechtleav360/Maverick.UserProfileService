namespace UserProfileService.Common.V2.CommandLineTools.Cleanup;

/// <summary>
///     Defines the scope of a cleanup.
/// </summary>
public enum CleanupTargetScope
{
    /// <summary>
    ///     Scope is to clean everything
    /// </summary>
    All,

    /// <summary>
    ///     Scope is to clean only legacy/main data
    /// </summary>
    Main,

    /// <summary>
    ///     Scope is to clean only first-level and second-level-projection data
    /// </summary>
    Extended
}
