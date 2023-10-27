using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UserProfileService.Common.V2.TicketStore.Enums;
using UserProfileService.Common.V2.TicketStore.Models;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.IntegrationTests.Extensions
{
    internal static class HttpResponseMessageExtension
    {
        public static async Task<(bool success, T obj)> TryParseContent<T>(this HttpResponseMessage responseMessage)
        {
            string contentStream = await responseMessage.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contentStream))
            {
                return (false, default);
            }

            var obj = JsonConvert.DeserializeObject<T>(contentStream);

            return obj == null ? (false, default) : (true, obj);
        }

        public static async Task<(bool success, string createdId)> WaitForSuccessAsync(
            this HttpResponseMessage responseMessage,
            HttpClient client,
            ITestOutputHelper output = null,
            int maxAttempts = 5)
        {
            await responseMessage.EnsureSuccessStatusCodeAsync(output);

            var rawStatus = new Uri(responseMessage.Headers.Location + "/raw");

            var attempts = 0;

            while (attempts <= maxAttempts)
            {
                HttpResponseMessage statusResponse = await client.GetAsync(rawStatus);

                await statusResponse.EnsureSuccessStatusCodeAsync(output);

                (bool success, UserProfileOperationTicket ticket) =
                    await statusResponse.TryParseContent<UserProfileOperationTicket>();

                Assert.True(success);

                switch (ticket.Status)
                {
                    case TicketStatus.Complete:
                        return (true, ticket.ObjectIds.FirstOrDefault());
                    case TicketStatus.Failure:
                        return (false, ticket.ObjectIds.FirstOrDefault());
                }

                Thread.Sleep(500 * attempts);
                attempts++;
            }

            return (false, null);
        }

        public static async Task EnsureSuccessStatusCodeAsync(
            this HttpResponseMessage responseMessage,
            ITestOutputHelper output = null)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string content = await responseMessage.Content.ReadAsStringAsync();
                output?.WriteLine(content);

                throw new HttpRequestException($"{(int)responseMessage.StatusCode} - {responseMessage.ReasonPhrase}");
            }
        }
    }
}
