using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Is a wrapper for delete client settings of an resource.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class ClientSettingsDeletedPayload : PayloadBase<ClientSettingsDeletedPayload>
{
    /// <summary>
    ///     Specifies the key of the client-settings to delete.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Key { get; set; }

    /// <summary>
    ///     Used to identify the resource.
    /// </summary>
    [Required]
    public ProfileIdent Resource { get; set; }
}
