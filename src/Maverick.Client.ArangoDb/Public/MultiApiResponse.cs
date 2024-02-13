using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Query;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Response Typ for Queries that implies more than one API calls.
/// </summary>
/// <typeparam name="T">Type of an object that you want get from the arango.</typeparam>
public sealed class MultiApiResponse<T> : MultiApiResponse
{
    /// <summary>
    ///     Gets or sets a list of exceptions that occurred during parsing or deserialization.
    ///     This property provides detailed information about any issues encountered
    ///     while processing and interpreting data.
    /// </summary>
    public IReadOnlyCollection<JsonDeserializationException> ParsingExceptions { get; }

    /// <summary>
    ///     A list contains the result of executed query.
    /// </summary>
    public IReadOnlyList<T> QueryResult { get; }

    internal MultiApiResponse(IList<BaseApiResponse> responses, IEnumerable<T> queryResult): base(responses)
    {
        QueryResult = queryResult.ToList();

        ParsingExceptions = responses.OfType<IResponseWithParsingException>()
                 .Where(r => r.ParsingException != null)
                 .Select(r => r.ParsingException)
                 .ToList();
    }
}

/// <summary>
///     Response Typ for Queries that implies more than one API calls.
/// </summary>
public class MultiApiResponse : IApiResponse
{
    /// <summary>
    ///     boolean flag to indicate whether an error occurred (true in this case)
    /// </summary>
    public bool Error { get; }

    /// <summary>
    /// A collection of warning messages occurred during executing a cursor request.
    /// </summary>
    public List<string> Warnings { get; }

    /// <summary>
    ///     Request execution time in milliseconds.
    /// </summary>
    public long ExecutionTime { get; }

    /// <summary>
    ///     A list of the API Requests executed to fetch all query results.
    /// </summary>
    public IEnumerable<BaseApiResponse> Responses { get; }

    internal MultiApiResponse(IEnumerable<BaseApiResponse> responses)
    {
        Responses = responses ?? throw new ArgumentNullException(nameof(responses));

        long execTime = 0;
        var errorHappened = false;
        var warningSet = new List<CursorResponseWarning>();

        responses.ToList()
            .ForEach(
                res =>
                {
                    if (res is ICursorResponse
                        {
                            CursorDetails.Extra.Warnings: not null
                        } cursorResponse)
                    {
                        warningSet.AddRange(cursorResponse.CursorDetails.Extra.Warnings);
                    }
                    execTime += res?.DebugInfos?.ExecutionTime ?? 0;
                    errorHappened &= res?.Error ?? true;
                });

        ExecutionTime = execTime;
        Error = errorHappened;

        Warnings = warningSet
            .Select(w => $"{w.Message} (internal code: {w.Code})")
            .Distinct()
            .ToList();
    }
}
