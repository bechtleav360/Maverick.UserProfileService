using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     attributes of the deleted document  <see cref="DeleteDocumentResponseEntity" />
/// </summary>
/// <inheritdoc />
public class DeleteDocumentResponse : SingleApiResponse<DeleteDocumentResponseEntity>
{
    internal DeleteDocumentResponse(Response response, DeleteDocumentResponseEntity document) : base(
        response,
        document)
    {
    }

    internal DeleteDocumentResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
