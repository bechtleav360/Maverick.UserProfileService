namespace UserProfileService.Adapter.Marten.EntityModels;

/// <summary>
///     Defines a Marten document store entity of a user.
/// </summary>
public class UserDbModel
{
    /// <summary>
    ///     The id of the user profile.
    /// </summary>
    public string Id { get; set; } = string.Empty;
}
