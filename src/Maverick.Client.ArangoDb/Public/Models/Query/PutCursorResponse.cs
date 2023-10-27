using System;
using Maverick.Client.ArangoDb.Protocol;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with attributes
///     of the next batch from cursor <see cref="PutCursorResponseEntity{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
public class PutCursorResponse<T> : SingleApiResponse<PutCursorResponseEntity<T>>, ICursorResponse
{
    /// <inheritdoc />
    [JsonIgnore]
    public ICursorInnerResponse CursorDetails => Result;

    internal PutCursorResponse(Response response, PutCursorResponseEntity<T> responseEntity) : base(
        response,
        responseEntity)
    {
    }

    internal PutCursorResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
