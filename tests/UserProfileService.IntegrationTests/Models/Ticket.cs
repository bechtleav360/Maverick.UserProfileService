using System;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;

namespace UserProfileService.IntegrationTests.Models
{
    internal class Ticket : TicketBase
    {
        public Ticket(string id, string type) : base(id, type)
        {
        }

        public Ticket(string id, DateTime finished, TicketStatus status) : base(id, finished, status)
        {
        }

        public Ticket(string id, string type, DateTime finished, TicketStatus status) : base(id, type, finished, status)
        {
        }

        public Ticket(string id, string type, DateTime finished, int errorCode, string errorMessage) : base(
            id,
            type,
            finished,
            errorCode,
            errorMessage)
        {
        }
    }
}
