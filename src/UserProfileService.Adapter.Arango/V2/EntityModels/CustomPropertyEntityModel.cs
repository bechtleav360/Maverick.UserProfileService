namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class CustomPropertyEntityModel
{
    public string Key { get; set; }

    /// <summary>
    ///     The id of the related object/profile that owns this custom property (ArangoDb internal id).
    /// </summary>
    public string Related { get; set; }

    public string Value { get; set; }
}
