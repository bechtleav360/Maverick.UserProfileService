using System.Collections.Generic;
using UserProfileService.Common.V2.DependencyInjection;
using UserProfileService.Saga.Events.Contracts;

namespace UserProfileService.Saga.Worker.Setup;

internal class InternalEventProcessingSetup : EventProcessingSetup
{
    public override HashSet<string> DirectProcessedCommandTypes { get; } =
        new HashSet<string>
        {
            CommandConstants.UserSettingSectionCreated,
            CommandConstants.UserSettingObjectUpdated,
            CommandConstants.UserSettingObjectDeleted,
            CommandConstants.UserSettingSectionDeleted
        };
}
