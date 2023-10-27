namespace UserProfileService.Projection.Abstractions.Messages;

public class ProjectionSuccessResponseMessage
{
    /// <summary>
    ///     Optional id of the entity that was projected
    /// </summary>
    public string EntityId { get; set; }
}
