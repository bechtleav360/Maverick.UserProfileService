using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a document as generic
///     object
/// </summary>
/// <typeparam name="T"></typeparam>
/// <inheritdoc />
public class GetDocumentResponse<T> : SingleApiResponse<T>
{
    internal GetDocumentResponse(Response response, T documentData) : base(response, documentData)
    {
    }

    internal GetDocumentResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a document entity
///     <see cref="DocumentWithData" />
/// </summary>
/// <inheritdoc />
public class GetDocumentResponse : SingleApiResponse<DocumentWithData>
{
    internal GetDocumentResponse(Response response, DocumentWithData documentData = null) : base(
        response,
        documentData)
    {
    }

    internal GetDocumentResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
