using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a list of object with some
///     attributes of the document before and after the update <see cref="DocumentResponseEntity" />
/// </summary>
/// <inheritdoc />
public class UpdateDocumentsResponse : SingleApiResponse<IList<DocumentResponseEntity>>
{
    internal UpdateDocumentsResponse(Response response, IList<DocumentResponseEntity> documentResponseEntities) :
        base(response, documentResponseEntities)
    {
    }

    internal UpdateDocumentsResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
