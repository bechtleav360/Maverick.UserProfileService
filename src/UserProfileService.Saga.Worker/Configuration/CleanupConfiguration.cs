using System;

namespace UserProfileService.Saga.Worker.Configuration;

/// <summary>
///     Contains configuration of cleanup background service.
/// </summary>
public class CleanupConfiguration
{
    internal const string DefaultSectionName = "Cleanup";

    /// <summary>
    ///     The duration between two cleanup operations.
    /// </summary>
    public TimeSpan Interval { get; } = TimeSpan.FromHours(3);

    /// <summary>
    ///     Determines whether the specified <see cref="CleanupConfiguration" /> is equal to the current
    ///     <see cref="CleanupConfiguration" />.
    /// </summary>
    /// <param name="other">The object to compare is equal to the current object, otherwise, false.</param>
    /// <returns></returns>
    protected bool Equals(CleanupConfiguration other)
    {
        return Interval.Equals(other.Interval);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is CleanupConfiguration config
            && Equals(config);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Interval.GetHashCode();
    }
}
