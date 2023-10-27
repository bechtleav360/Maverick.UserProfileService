using System;

namespace UserProfileService.Sync.Abstraction.Annotations;

/// <summary>
///     Attribute used to identify sync models with a unique value. Example: Configuration of systems to specific entities.
/// </summary>
public class ModelAttribute : Attribute
{
    /// <summary>
    ///     Entity name of model to be specified.
    /// </summary>
    public string Model { get; }

    /// <summary>
    ///     Create an instance of <see cref="ModelAttribute" />
    /// </summary>
    /// <param name="model">Entity name of model to be specified.</param>
    public ModelAttribute(string model)
    {
        Model = model;
    }
}
