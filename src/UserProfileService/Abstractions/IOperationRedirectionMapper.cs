using UserProfileService.Common.V2.TicketStore.Models;
using UserProfileService.Utilities;

namespace UserProfileService.Abstractions;

public interface IOperationRedirectionMapper
{
    OperationMap MapTicket(UserProfileOperationTicket ticket);
}
