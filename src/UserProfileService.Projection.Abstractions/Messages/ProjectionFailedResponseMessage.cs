namespace UserProfileService.Projection.Abstractions.Messages;

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
