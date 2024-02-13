namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents an entity model for functional access rights.
/// </summary>
public class FunctionalAccessRightEntityModel
{
    /// <summary>
    ///     Gets or sets a value indicating whether the access right is inherited.
    /// </summary>
    public bool Inherited { get; set; }
    /// <summary>
    ///     Gets or sets the name of the access right.
    /// </summary>
    public string Name { get; set; }
}
