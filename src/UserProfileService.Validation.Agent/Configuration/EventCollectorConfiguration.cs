namespace UserProfileService.EventCollector.Configuration;

/// <summary>
///     Configuration for the event collector agent.
///     This defines how the individual response messages are to be aggregated.
/// </summary>
public class EventCollectorConfiguration
{
    /// <summary>
    ///     Specifies how many responses are expected from each service
    ///     before an aggregated response is sent by the agent.
    /// </summary>
    public int ExpectedResponses { get; set; } = 1;
}
