using System.ServiceModel;
using UserProfileService.Abstractions;
using UserProfileService.Common.V2.TicketStore.Models;

namespace UserProfileService.Utilities;

public class OperationRedirectionMapper : IOperationRedirectionMapper
{
    protected virtual List<OperationMap> Maps => DefaultOperationRedirectionMapping.GetDefaultMapping();

    
    public OperationMap MapTicket(UserProfileOperationTicket ticket)
    {
        OperationMap map = Maps.FirstOrDefault(x => x.Operation == ticket.Operation)
            ?? throw new ActionNotSupportedException(
                $"Unsupported operation: No operation map found valid for action {ticket.Operation}.");

        return map;
    }
}