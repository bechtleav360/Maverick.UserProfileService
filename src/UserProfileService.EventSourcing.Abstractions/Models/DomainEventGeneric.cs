namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Fallback when deserializing events.
/// </summary>
public class DomainEventGeneric : DomainEventBaseV2<string>
{
}
