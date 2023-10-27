using System.Collections.Generic;

namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller;

/// <summary>
///     The start type of the application.
/// </summary>
public static class StartType
{
    public const string Auto = "auto";
    public const string Boot = "boot";
    public const string DelayedAuto = "delayed-auto";
    public const string Demand = "demand";
    public const string Disabled = "disabled";
    public const string System = "system";

    /// <summary>
    ///     Validates if the string is in the list of valid keys.
    /// </summary>
    /// <param name="input">The input parameter to check if it is in the list of valid keys.</param>
    /// <returns>If the the input parameter is part of the valid keys list.</returns>
    public static bool Contains(string input)
    {
        var validKeys = new List<string>
        {
            Boot,
            System,
            Auto,
            Demand,
            Disabled,
            DelayedAuto
        };

        return validKeys.Contains(input);
    }
}
