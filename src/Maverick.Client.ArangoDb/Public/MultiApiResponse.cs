using System;
using System.Collections.Generic;
using System.Linq;

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
    ///     A list contains the result of executed query.
    /// </summary>
    public IReadOnlyList<T> QueryResult { get; }

    internal MultiApiResponse(IEnumerable<BaseApiResponse> responses, IEnumerable<T> queryResult) : base(responses)
    {
        QueryResult = queryResult.ToList();
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
    ///     Requests execution time
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

        responses.ToList()
            .ForEach(
                res =>
                {
                    execTime += res?.DebugInfos?.ExecutionTime ?? 0;
                    errorHappened &= res?.Error ?? true;
                });

        ExecutionTime = execTime;
        Error = errorHappened;
    }
}
