using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Defines a object by id and type.
///     Additionally, how many steps away the object is in the given context.
/// </summary>
public class ObjectIdentPath : ObjectIdent
{
    /// <summary>
    ///     Path in current context to object ident.
    /// </summary>
    public IList<string> Path { get; set; } = new List<string>();

    /// <summary>
    ///     The number of steps how far away the object is in the given context.
    /// </summary>
    public int Steps => Path.IndexOf(Id);

    /// <summary>
    ///     Create an instance of <see cref="ObjectIdent" />.
    /// </summary>
    public ObjectIdentPath()
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="ObjectIdent" />.
    /// </summary>
    /// <param name="id">Identifier of the object.</param>
    /// <param name="type">Type of the object.</param>
    public ObjectIdentPath(string id, ObjectType type) : base(id, type)
    {
    }
}
