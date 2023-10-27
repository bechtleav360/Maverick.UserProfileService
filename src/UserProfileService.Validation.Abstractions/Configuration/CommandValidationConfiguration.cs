using System.Collections.Generic;

namespace UserProfileService.Validation.Abstractions.Configuration;

/// <summary>
///     Configuration of the validation for the commands that can be executed.
/// </summary>
public class CommandValidationConfiguration
{
    /// <summary>
    ///     Defines whether the commands must be validated by external services. Default is false.
    /// </summary>
    public IDictionary<string, bool> External { get; set; } = new Dictionary<string, bool>();
}
