using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is emitted when properties in a profile were updated.
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.ProfileChange)]
public class ProfilePropertiesChangedMessage : PropertiesUpdatedPayload
{
    /// <summary>
    ///     Type of entity to remove the tags from.
    /// </summary>
    public ProfileKind ProfileKind { get; set; }
}
