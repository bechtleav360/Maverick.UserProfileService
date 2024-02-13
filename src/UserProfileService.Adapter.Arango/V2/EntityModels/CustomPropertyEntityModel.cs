namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a custom property associated with a profile.
/// </summary>
public class CustomPropertyEntityModel
{
    /// <summary>
    ///     Gets or sets the key for the custom property.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     Gets or sets the id of the related object/profile that owns this custom property (ArangoDb internal ID).
    /// </summary>
    public string Related { get; set; }

    /// <summary>
    ///     Gets or sets the value of the custom property.
    /// </summary>
    public string Value { get; set; }
}
