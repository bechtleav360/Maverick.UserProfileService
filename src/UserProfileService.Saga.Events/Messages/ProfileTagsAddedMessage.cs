using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is raised when tags are set for a profile (user or group).
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.ProfileTagsAdded)]
public class ProfileTagsAddedMessage : TagsSetPayload
{
    /// <summary>
    ///     Describes the type of profile that will be changed.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global => The profile kind will be needed.
    public ProfileKind ProfileKind { get; set; }
}
