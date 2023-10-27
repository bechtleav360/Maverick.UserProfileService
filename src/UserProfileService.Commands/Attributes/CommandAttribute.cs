using System;

namespace UserProfileService.Commands.Attributes;

/// <summary>
///     Attribute to define a command for a saga message.
/// </summary>
public class CommandAttribute : Attribute
{
    /// <summary>
    ///     Name of command.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Create an instance of <see cref="CommandAttribute" />.
    /// </summary>
    /// <param name="value">Name of command for saga message.</param>
    public CommandAttribute(string value)
    {
        Value = value;
    }
}
