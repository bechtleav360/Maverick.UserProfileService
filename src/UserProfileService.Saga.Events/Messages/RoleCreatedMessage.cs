﻿using UserProfileService.Commands.Attributes;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Events.Messages;

/// <summary>
///     This message is emitted when a new role is created.
///     BE AWARE messages in the queue should be processed/removed before an upgrade is performed.
/// </summary>
[Command(CommandConstants.RoleCreate)]
public class RoleCreatedMessage : RoleCreatedPayload
{
}
