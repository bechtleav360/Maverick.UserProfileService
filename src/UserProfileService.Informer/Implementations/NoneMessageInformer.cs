using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Informer.Abstraction;

namespace UserProfileService.Informer.Implementations;

/// <summary>
///     An implementation where nothing happens.
/// </summary>
public class NoneMessageInformer : IMessageInformer
{
    /// <inheritdoc />
    public Task NotifyEventOccurredAsync(IUserProfileServiceEvent serviceEvent, INotifyContext context)
    {
        return Task.CompletedTask;
    }
}
