using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     The factory that creates a relation handler.
/// </summary>
public interface IRelationFactory
{
    /// <summary>
    ///     Create a handler for a certain system and entity.
    /// </summary>
    /// <param name="sourceSystem">The source system of the relation handler.</param>
    /// <param name="relationEntity">The relation entity of the relation handler.</param>
    /// <returns>Returns a relation handler of type <see cref="IRelationHandler" />.</returns>
    IRelationHandler CreateRelationHandler(string sourceSystem, string relationEntity);
}
