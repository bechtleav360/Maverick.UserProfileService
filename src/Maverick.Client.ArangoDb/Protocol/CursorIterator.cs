using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.PerformanceLogging.Implementations;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Query;

namespace Maverick.Client.ArangoDb.Protocol;

internal class CursorIterator<T>
{
    private CreateCursorBody CursorBody { get; }
    private AQuery CursorClient { get; }
    private string TransactionId { get; }
    public string CursorId { get; private set; }
    public BaseApiResponse CursorResponse { get; private set; }
    public bool Failed { get; set; }
    public bool HasNext { get; set; } = true;
    public IEnumerable<T> Value { get; private set; }
    public JsonDeserializationException ParsingException { get; private set; }

    public CursorIterator(AQuery cursorClient, CreateCursorBody cursorBody, string transactionId = null)
    {
        CursorClient = cursorClient;
        CursorBody = cursorBody;
        TransactionId = transactionId;
    }

    public async Task<CursorIterator<T>> NextAsync()
    {
        if (CursorId == null)
        {
            CursorResponse<T> response =
                await DefaultPerformanceLogExecutor.LogPerformanceAsync(
                    () => CursorClient.CreateCursorAsync<T>(CursorBody, TransactionId),
                    CursorBody,
                    TransactionId);

            CursorResponse = response;
            Value = response?.Result?.Result;
            CursorId = response?.Result?.Id;
            Failed = response?.Error ?? true;
            HasNext = !Failed && (response?.Result?.HasMore ?? false);
            ParsingException = response?.ParsingException;

            return this;
        }

        if (HasNext)
        {
            PutCursorResponse<T> response =
                await DefaultPerformanceLogExecutor.LogPerformanceAsync(
                    () => CursorClient.PutCursorAsync<T>(CursorId),
                    CursorBody,
                    TransactionId);

            CursorResponse = response;
            Value = response?.Result?.Result;
            ParsingException = response?.ParsingException;
            HasNext = response?.Result?.HasMore ?? false;
        }

        return this;
    }
}
