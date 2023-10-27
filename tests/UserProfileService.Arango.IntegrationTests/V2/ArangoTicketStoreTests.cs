using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.Implementations;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2
{
    [Collection(nameof(DatabaseCollection))]
    public class ArangoTicketStoreTests
    {
        private readonly DatabaseFixture _fixture;

        public ArangoTicketStoreTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("f703ea39-2c04-47c0-a1e3-823885318170", typeof(TicketA), null)]
        [InlineData("03479d18-b0d4-4511-90e5-6f1463828efd", typeof(TicketB), null)]
        [InlineData("null-ticket", null, typeof(ArgumentNullException))]
        [InlineData("invalid id", typeof(TicketA), typeof(ArgumentException))]
        [InlineData("invalid#id", typeof(TicketB), typeof(ArgumentException))]
        public async Task CreateTicketTest(string id, Type ticketType, Type expectedException)
        {
            ITicketStore ticketStore = await _fixture.GetTicketStoreAsync();

            TicketBase ticket = null;

            if (ticketType == typeof(TicketA))
            {
                ticket = new TicketA(id)
                {
                    ExtraValue = "test"
                };
            }
            else if (ticketType == typeof(TicketB))
            {
                ticket = new TicketB(id);
            }

            try
            {
                await ticketStore.AddOrUpdateEntryAsync(ticket);

                Assert.Null(expectedException);
                TicketBase readTicket = await ticketStore.GetTicketAsync(ticket?.Id);
                Assert.Equal(ticket?.Type, readTicket.Type);
            }
            catch (Exception e)
            {
                Assert.NotNull(expectedException);
                Assert.IsType(expectedException, e);
            }
        }

        [Fact]
        public async Task DeleteByIdTest()
        {
            ITicketStore ticketStore = await _fixture.GetTicketStoreAsync();
            var ticketId = Guid.NewGuid().ToString("D");

            await ticketStore.AddOrUpdateEntryAsync(new TicketA(ticketId));
            TicketBase ticket = await ticketStore.GetTicketAsync(ticketId);
            Assert.NotNull(ticket);

            int deleted = await ticketStore.DeleteTicketsAsync(t => t.Id == ticketId);
            Assert.Equal(1, deleted);

            ticket = await ticketStore.GetTicketAsync(ticketId);
            Assert.Null(ticket);
        }

        [Fact]
        public async Task FindTicketById()
        {
            TicketBase ticket = new TicketA(Guid.NewGuid().ToString("D"));

            ITicketStore ticketStore = await _fixture.GetTicketStoreAsync();

            await ticketStore.AddOrUpdateEntryAsync(ticket);

            string id = ticket.Id;
            IList<TicketBase> readTickets = await ticketStore.GetTicketsAsync(tb => tb.Id == id);
            Assert.Single(readTickets);
            Assert.Equal(ticket.Id, readTickets[0].Id);
        }

        [Theory]
        [InlineData("f14f4aad-bcce-4f1e-9d22-58807a5d2620", true, true)]
        [InlineData("not_found", false, false)]
        [InlineData(" invalid id ", false, false)]
        [InlineData("ßinvalidId", false, false)]
        public async Task GetByIdTest(string id, bool create, bool isResultExpected)
        {
            ITicketStore ticketStore = await _fixture.GetTicketStoreAsync();

            if (create)
            {
                await ticketStore.AddOrUpdateEntryAsync(new TicketA(id));
            }

            TicketBase ticket = await ticketStore.GetTicketAsync(id);

            Assert.Equal(isResultExpected, ticket != null);

            if (isResultExpected)
            {
                Assert.Equal(id, ticket?.Id);
            }
        }

        [Theory]
        [InlineData(typeof(TicketA), typeof(TicketA))]
        [InlineData(typeof(TicketA), typeof(TicketB))]
        [InlineData(typeof(TicketB), typeof(TicketA))]
        [InlineData(typeof(TicketB), typeof(TicketB))]
        public async Task OverrideTicketTest(Type beforeType, Type afterType)
        {
            ITicketStore ticketStore = await _fixture.GetTicketStoreAsync();
            var ticketId = Guid.NewGuid().ToString("D");

            TicketBase before = null;

            if (beforeType == typeof(TicketA))
            {
                before = new TicketA(ticketId)
                {
                    ExtraValue = "before"
                };
            }
            else if (beforeType == typeof(TicketB))
            {
                before = new TicketB(ticketId)
                {
                    Values = new List<string>
                    {
                        "before"
                    }
                };
            }

            await ticketStore.AddOrUpdateEntryAsync(before);

            TicketBase after = null;

            if (afterType == typeof(TicketA))
            {
                after = new TicketA(ticketId)
                {
                    ExtraValue = "after",
                    Status = TicketStatus.Complete
                };
            }
            else if (afterType == typeof(TicketB))
            {
                after = new TicketB(ticketId)
                {
                    Values = new List<string>
                    {
                        "after"
                    },
                    Status = TicketStatus.Complete
                };
            }

            await ticketStore.AddOrUpdateEntryAsync(after);

            TicketBase current = await ticketStore.GetTicketAsync(ticketId);

            Assert.NotNull(current);
            Assert.IsType(afterType, current);
            Assert.Equal(TicketStatus.Complete, current.Status);
        }
    }
}
