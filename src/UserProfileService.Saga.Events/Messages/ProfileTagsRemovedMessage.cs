using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is raised when tags are removed from a profile (user or group).
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.ProfileTagsRemoved)]
public class ProfileTagsRemovedMessage : TagsRemovedPayload
{
    /// <summary>
    ///     Describes the type of profile that will be changed.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global => The set method will be used.
    public ProfileKind ProfileKind { get; set; }
}
