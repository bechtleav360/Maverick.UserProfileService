using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Health.Report;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.Health.Consumers;

/// <summary>
///     Consumer of HealthCheckMessages. Pushes received messages into an instance of
///     <see cref="IDistributedHealthStatusStore" />.
/// </summary>
public class HealthCheckMessageConsumer : IConsumer<HealthCheckMessage>
{
    private readonly IDistributedHealthStatusStore _distributedHealthStatusStore;
    private readonly ILogger<HealthCheckMessageConsumer> _logger;

    /// <summary>
    ///     Create instance of <see cref="HealthCheckMessageConsumer" />.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="distributedHealthStatusStore">The <see cref="IDistributedHealthStatusStore" /> the message is pushed into.</param>
    public HealthCheckMessageConsumer(
        ILogger<HealthCheckMessageConsumer> logger,
        IDistributedHealthStatusStore distributedHealthStatusStore)
    {
        _logger = logger;
        _distributedHealthStatusStore = distributedHealthStatusStore;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<HealthCheckMessage> context)
    {
        _logger.EnterMethod();

        try
        {
            _logger.LogTraceMessage(
                "Consuming {message}",
                new object[]
                {
                    // HealthCheckMessage is a record and thus turned into a json-like string.
                    context.Message.ToString()
                });

            await _distributedHealthStatusStore.AddHealthStatusAsync(context.Message, context.CancellationToken);
        }
        finally
        {
            _logger.ExitMethod();
        }
    }
}
