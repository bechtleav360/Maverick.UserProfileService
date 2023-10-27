namespace UserProfileService.Saga.Worker.Configuration;

/// <summary>
///     Connection-Configuration to read Config-Events from EventBus
/// </summary>
public class EventBusConnectionConfiguration
{
    /// <summary>
    ///     Hub used to receive Config-Events
    /// </summary>
    public string Hub { get; set; }

    /// <summary>
    ///     Set URI used to connect to EventBus
    /// </summary>
    public string Server { get; set; }
}
