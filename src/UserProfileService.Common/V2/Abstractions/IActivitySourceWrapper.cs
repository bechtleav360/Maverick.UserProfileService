using System.Diagnostics;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Wrapper to wrap the activity source to use
///     dependency injection.
/// </summary>
public interface IActivitySourceWrapper
{
    /// <summary>
    ///     The activity source to start and end activities.
    /// </summary>
    ActivitySource ActivitySource { get; set; }
}
