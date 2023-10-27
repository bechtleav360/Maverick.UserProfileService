using System.Diagnostics;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.Implementations;

/// <inheritdoc />
public class ActivitySourceWrapper : IActivitySourceWrapper
{
    /// <inheritdoc />
    public ActivitySource ActivitySource { get; set; }

    /// <summary>
    ///     Creates an instance of <ss cref="ActivitySourceWrapper" />.
    /// </summary>
    /// <param name="activitySource">The activity source that is used to create activity source.</param>
    public ActivitySourceWrapper(ActivitySource activitySource)
    {
        ActivitySource = activitySource;
    }

    /// <summary>
    ///     Creates an instance of <ss cref="ActivitySourceWrapper" />.
    /// </summary>
    /// <param name="name">The name of the source activity.</param>
    /// <param name="version">The version of the source activity.</param>
    public ActivitySourceWrapper(string name, string version)
    {
        ActivitySource = new ActivitySource(name, version);
    }
}
