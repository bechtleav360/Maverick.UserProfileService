using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     attributes of the document before and after the update <see cref="UpdateDocumentEntity{T}" />
/// </summary>
/// <typeparam name="T">document typ</typeparam>
/// <inheritdoc />
public class UpdateDocumentResponse<T> : SingleApiResponse<UpdateDocumentEntity<T>>
{
    internal UpdateDocumentResponse(Response response, UpdateDocumentEntity<T> document) : base(response, document)
    {
    }

    internal UpdateDocumentResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
