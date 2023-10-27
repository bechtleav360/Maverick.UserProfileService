using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Contains methods to extend <see cref="Response" />
/// </summary>
internal static class ResponseModelExtensions
{
    /// <summary>
    ///     Adds a transaction id to the debugging infos of <paramref name="response" />.
    /// </summary>
    internal static void AddTransactionId(
        this Response response,
        string transactionId)
    {
        if (response?.DebugInfo?.TransactionId == null
            || string.IsNullOrWhiteSpace(transactionId))
        {
            return;
        }

        response.DebugInfo.TransactionId = transactionId;
    }
}
