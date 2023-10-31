﻿using MassTransit;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Definition for <see cref="CommandProcessState" /> and <see cref="CommandProcessStateMachine" />.
/// </summary>
// ReSharper disable once UnusedType.Global => The class is used by reflection.
public class CommandProcessStateDefinition :
    SagaDefinition<CommandProcessState>
{
    /// <inheritdoc />
    protected override void ConfigureSaga(
        IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<CommandProcessState> sagaConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(5, 1000));
        endpointConfigurator.UseInMemoryOutbox();
    }
}
