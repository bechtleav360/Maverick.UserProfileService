using Maverick.UserProfileService.Models.BasicModels;

namespace UserProfileService.Validation.Abstractions.Configuration;

/// <summary>
///     Validation configuration for <see cref="FunctionBasic" />
/// </summary>
public class FunctionValidationConfiguration
{
    /// <summary>
    ///     Specifies whether a function with the same role and organization may be created.
    /// </summary>
    public bool DuplicateAllowed { get; set; } = true;
}
