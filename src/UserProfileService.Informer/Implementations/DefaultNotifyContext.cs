using Maverick.UserProfileService.Models.Models;
using UserProfileService.Informer.Abstraction;

namespace UserProfileService.Informer.Implementations;

/// <summary>
///     The default implementation for the notify context.
/// </summary>
public class DefaultNotifyContext : INotifyContext
{
    /// <inheritdoc />
    public ObjectIdent? ContextType { get; set; }

    /// <inheritdoc />
    public string? ExternalIdentifier { get; set; }
}
