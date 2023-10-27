using System;
using Maverick.Client.ArangoDb.Protocol;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing cursor properties and
///     perhaps values of type T inside it <see cref="CreateCursorResponseEntity{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
public class CursorResponse<T> : SingleApiResponse<CreateCursorResponseEntity<T>>, ICursorResponse
{
    /// <inheritdoc />
    [JsonIgnore]
    public ICursorInnerResponse CursorDetails => Result;

    internal CursorResponse(Response response, CreateCursorResponseEntity<T> entity) : base(response, entity)
    {
    }

    internal CursorResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
