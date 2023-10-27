using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.Tests.V2.TestModels
{
    public class BadTransaction : IDatabaseTransaction
    {
        /// <summary>
        ///     The Id of the transaction.
        /// </summary>
        public string TransactionId { get; set; }

        public CallingServiceContext CallingService { get; set; }
    }
}
