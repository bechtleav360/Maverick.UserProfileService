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
   public ObjectIdent ContextType { get; set; }

   /// <summary>
   ///     The external id that is needed for the notify context.
   /// </summary>
   public string ExternalIdentifier { set; get; }
}
