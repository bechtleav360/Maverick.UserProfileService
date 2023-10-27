using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is raised when client settings are updated for a profile (user, group or organization).
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.ProfileClientSettingsUpdated)]
public class ProfileClientSettingsUpdatedMessage : ClientSettingsUpdatedPayload
{
}
