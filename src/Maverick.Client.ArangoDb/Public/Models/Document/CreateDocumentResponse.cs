using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with attributes
///     of the created document  <see cref="DocumentResponseEntity" />
/// </summary>
/// <inheritdoc />
public class CreateDocumentResponse : SingleApiResponse<DocumentResponseEntity>
{
    internal CreateDocumentResponse(Response response, DocumentResponseEntity document) : base(response, document)
    {
    }

    internal CreateDocumentResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
