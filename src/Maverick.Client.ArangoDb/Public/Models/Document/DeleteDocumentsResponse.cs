using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a list of object with some
///     attributes of the deleted documents <see cref="DocumentBase" />
/// </summary>
/// <inheritdoc />
public class DeleteDocumentsResponse : SingleApiResponse<List<DocumentBase>>
{
    internal DeleteDocumentsResponse(Response response, List<DocumentBase> deletedDocuments) : base(
        response,
        deletedDocuments)
    {
    }

    internal DeleteDocumentsResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
