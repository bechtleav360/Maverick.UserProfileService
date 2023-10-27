using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a list of objects with
///     attributes of the created documents  <see cref="DocumentResponseEntity" />
/// </summary>
/// <inheritdoc />
public class CreateDocumentsResponse : SingleApiResponse<IList<DocumentResponseEntity>>
{
    internal CreateDocumentsResponse(Response response, IList<DocumentResponseEntity> documents) : base(
        response,
        documents)
    {
    }

    internal CreateDocumentsResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
