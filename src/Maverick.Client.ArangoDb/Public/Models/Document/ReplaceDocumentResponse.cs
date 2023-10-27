using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     attributes with the replaced document <see cref="DocumentResponseEntity" />
/// </summary>
/// <inheritdoc />
public class ReplaceDocumentResponse : SingleApiResponse<DocumentResponseEntity>
{
    internal ReplaceDocumentResponse(Response response, DocumentResponseEntity document) : base(response, document)
    {
    }

    internal ReplaceDocumentResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
