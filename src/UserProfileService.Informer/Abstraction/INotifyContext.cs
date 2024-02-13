using Maverick.UserProfileService.Models.Models;
using ExternalIdentifier = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;

namespace UserProfileService.Informer.Abstraction;

/// <summary>
///     The notify context contains additional parameter that are needed for the notify message.
/// </summary>
public interface INotifyContext
{
   /// <summary>
   ///     The type that the notification is related to.
   /// </summary>
    ObjectIdent? ContextType { get; set; }

   /// <summary>
   ///     The external id that is needed for the notify context.
   /// </summary>
    List<ExternalIdentifier>? ExternalIdentifier { set; get; }

   /// <summary>
   /// Decides if the handler should be executed.
   /// </summary>
    public bool NotifyConsumer { get; set; }
}
