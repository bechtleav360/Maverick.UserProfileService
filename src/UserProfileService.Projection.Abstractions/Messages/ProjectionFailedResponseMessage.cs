namespace UserProfileService.Projection.Abstractions.Messages;

/// <summary>
///     Message containing information about a projection failure.
/// </summary>
public class ProjectionFailedResponseMessage
{
    /// <summary>
    ///     Optional id of the entity that was projected
    /// </summary>
    public string EntityId { get; set; }

    /// <summary>
    ///     Exception why because the projection failed.
    /// </summary>
    public string Exception { get; set; }
}
