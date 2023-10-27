namespace UserProfileService.Adapter.Marten.EntityModels;

/// <summary>
///     The settings section section that has a name and an unique id.
///     The model is used to store the information in the database.
/// </summary>
public class UserSettingSectionDbModel
{
    /// <summary>
    ///     The creation date of this section.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    ///     The id of the user setting section.
    /// </summary>
    public string Id { set; get; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     The name of the section.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Creates an objects of type <see cref="UserSettingSectionDbModel" />.
    /// </summary>
    /// <param name="name">The name of the section.</param>
    public UserSettingSectionDbModel(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        Name = name;
    }

    /// <summary>
    ///     Creates an objects of type <see cref="UserSettingSectionDbModel" />.
    ///     The default constructor is needed to deserialize the object from the database.
    /// </summary>
    public UserSettingSectionDbModel()
    {
    }
}
