namespace UserProfileService.Validation.Abstractions.Configuration;

/// <summary>
///     Config how the name should be handled.
/// </summary>
public class NameConfiguration
{
    /// <summary>
    ///     Specifies whether the duplicate check is performed for (display) name
    /// </summary>
    public bool Duplicate { get; set; }

    /// <summary>
    ///     Specifies whether the check should be case sensitive.
    ///     Will be considered only if <see cref="Duplicate" /> is False.
    /// </summary>
    public bool IgnoreCase { get; set; } = true;

    /// <summary>
    ///     Describes a regular expression that is used to validate the (display) name
    /// </summary>
    public string Regex { get; set; }
}
