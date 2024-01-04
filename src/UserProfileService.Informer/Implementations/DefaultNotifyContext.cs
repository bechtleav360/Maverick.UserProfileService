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
    public List<ExternalIdentifier> ExternalIdentifier { get; set; }
    
}
