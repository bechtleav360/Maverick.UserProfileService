using System.Collections.Generic;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Wraps all properties required for Properties changed events.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class PropertiesUpdatedPayload : PayloadBase<PropertiesUpdatedPayload>
{
    /// <summary>
    ///     Used to identify the resource.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Contains all changed properties with their new name.
    /// </summary>
    public IDictionary<string, object> Properties { get; set; }
}
