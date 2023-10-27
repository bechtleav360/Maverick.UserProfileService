using MassTransit;

namespace UserProfileService.Common.Health.Consumers;

/// <inheritdoc cref="ConsumerDefinition{TConsumer}" />
public class HealthCheckMessageConsumerDefinition : ConsumerDefinition<HealthCheckMessageConsumer>
{
    /// <inheritdoc />
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<HealthCheckMessageConsumer> consumerConfigurator)
    {
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rbc)
        {
            // AutoDelete is disabled if QueueExpiration is set.
            //x.QueueExpiration = TimeSpan.FromMinutes(60);
            rbc.AutoDelete = true; // Queue is deleted after the consumer disconnects.
            rbc.Durable = false; // Queue is in-memory only and does not survive a restart.
            rbc.PublishFaults = false; // No fault queue is created for this queue.
            rbc.SetQueueArgument("x-max-length", 10); // The queue holds a maximum of 10 elements.
            // Prevents publishers from filling up the queue while the consumer is still alive, but not consuming messages.
            rbc.SetQueueArgument("x-single-active-consumer", true); // Only a single consumer can consume messages.
        }
    }
}
