using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is emitted when a user settings section should be deleted.
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.UserSettingSectionDeleted)]
public class UserSettingSectionDeletedMessage : UserSettingSectionDeletedPayload
{
}
