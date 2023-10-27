using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is emitted when the user settings object of a section should be deleted.
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.UserSettingObjectDeleted)]
public class UserSettingObjectDeletedMessage : UserSettingObjectDeletedPayload
{
}
