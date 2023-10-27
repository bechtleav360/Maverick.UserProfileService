using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Events.Payloads;

/// <summary>
///     Defines a model to create domain models with unique identifier.
/// </summary>
public interface ICreateModelPayload : IPayload
{
    /// <summary>
    ///     A collection of ids that are used to identify the resource in an external source.
    /// </summary>
    public IList<ExternalIdentifier> ExternalIds { get; set; }

    /// <summary>
    ///     The id representing the unique identifier of this model.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }
}
