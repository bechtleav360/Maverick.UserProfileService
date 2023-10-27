using System;
using System.Collections.Generic;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.Implementations
{
    public class TicketB : TicketBase
    {
        public List<string> Values { get; set; } = new List<string>();

        public TicketB() : this(Guid.NewGuid().ToString("D"))
        {
        }

        /// <inheritdoc />
        public TicketB(string id) : base(id, nameof(TicketB))
        {
        }

        /// <inheritdoc />
        public TicketB(string id, DateTime finished, TicketStatus status) : base(id, nameof(TicketB), finished, status)
        {
        }

        /// <inheritdoc />
        public TicketB(string id, DateTime finished, int errorCode, string errorMessage) : base(
            id,
            nameof(TicketB),
            finished,
            errorCode,
            errorMessage)
        {
        }
    }
}
