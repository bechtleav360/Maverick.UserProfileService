namespace UserProfileService.Projection.Abstractions.Messages;
/// <summary>
///     Message containing information about a successful projection.
/// </summary>
public class ProjectionSuccessResponseMessage
{
    /// <summary>
    ///     Optional id of the entity that was projected
    /// </summary>
    public string EntityId { get; set; }
}
