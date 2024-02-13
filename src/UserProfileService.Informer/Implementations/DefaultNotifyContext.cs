using Maverick.UserProfileService.Models.Models;
using UserProfileService.Informer.Abstraction;
using ExternalIdentifier = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;

namespace UserProfileService.Informer.Implementations;

/// <summary>
///     The default implementation for the notify context.
/// </summary>
public class DefaultNotifyContext : INotifyContext
{
    /// <inheritdoc />
    public ObjectIdent? ContextType { get; set; }

    /// <inheritdoc />
    public IList<ExternalIdentifier>? ExternalIdentifier { get; set; }

    /// <inheritdoc />
    public bool NotifyConsumer { get; set; }
}
