using System;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.Implementations
{
    public class TicketA : TicketBase
    {
        public string ExtraValue { get; set; }

        public TicketA() : this(Guid.NewGuid().ToString("D"))
        {
        }

        /// <inheritdoc />
        public TicketA(string id) : base(id, nameof(TicketA))
        {
        }

        /// <inheritdoc />
        public TicketA(string id, DateTime finished, TicketStatus status) : base(id, nameof(TicketA), finished, status)
        {
        }

        /// <inheritdoc />
        public TicketA(string id, DateTime finished, int errorCode, string errorMessage) : base(
            id,
            nameof(TicketA),
            finished,
            errorCode,
            errorMessage)
        {
        }
    }
}
