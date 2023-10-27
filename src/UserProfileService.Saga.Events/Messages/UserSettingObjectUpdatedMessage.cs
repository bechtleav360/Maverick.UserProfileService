using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is emitted when the value of a setting object as child of a user settings section has been modified.
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.UserSettingObjectUpdated)]
public class UserSettingObjectUpdatedMessage : UserSettingObjectUpdatedPayload
{
}
