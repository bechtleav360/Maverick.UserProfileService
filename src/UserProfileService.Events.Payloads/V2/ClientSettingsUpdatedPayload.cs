using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Is a wrapper for setting client settings of an resource.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class ClientSettingsUpdatedPayload : PayloadBase<ClientSettingsUpdatedPayload>
{
    /// <summary>
    ///     Specifies the key of the client-settings to set.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Key { get; set; }

    /// <summary>
    ///     Used to identify the resource.
    /// </summary>
    [Required]
    public ProfileIdent Resource { get; set; }

    /// <summary>
    ///     Specifies the settings value to set.
    /// </summary>
    [Required]
    public JsonPatchDocument Settings { get; set; }
}
