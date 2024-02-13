using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Api.Common.Extensions;

public static class PaginatedListExtensions
{
    private static string ToLinkUri(this HttpRequest r, int offset, int limit)
    {
        var baseUri = $"{r.Scheme}://{r.Host}";
        var path = $"{r.Path}";

        List<string> queryItems = r.Query.Select(
                x =>
                    $"{x.Key}={(x.Key.ToLowerInvariant() == "offset" ? offset.ToString() : x.Value.ToString())}")
            .ToList();

        if (r.Query.All(x => x.Key.ToLowerInvariant() != "offset"))
        {
            queryItems.Add($"{nameof(QueryObject.Offset)}={offset}");
        }

        if (r.Query.All(x => x.Key.ToLowerInvariant() != "limit"))
        {
            queryItems.Add($"{nameof(QueryObject.Limit)}={limit}");
        }

        string uri = (baseUri + path + "?" + string.Join("&", queryItems)).Replace(@"""", "%22");

        return uri;
    }

    public static ListResponseResult<T>? ToListResponseResult<T>(
        this IEnumerable<T>? list,
        HttpContext context,
        IPaginationSettings? paginationSettings = null)
    {
        paginationSettings ??= new QueryObject();

        if (list == null)
        {
            return null;
        }

        IList<T> renderedList = list as List<T> ?? list.ToList();

        foreach (T o in renderedList)
        {
            o.ResolveUrlProperties(context);
        }

        var paginatedList = list as IPaginatedList<T>;

        return new ListResponseResult<T>
        {
            Result = paginatedList ?? list,
            Response = context.CreateListResponse(
                paginatedList?.TotalAmount ?? renderedList.Count,
                paginationSettings)
        };
    }

    /// <summary>
    ///     Uses <see cref="HttpContext" /> and an optional <see cref="QueryObjectBase" /> parameter to create a
    ///     <see cref="ListResponse" />.
    /// </summary>
    /// <param name="context">The HTTP context containing request information for the link url string generation.</param>
    /// <param name="totalAmount">The total amount of result set.</param>
    /// <param name="query">The query object containing pagination setting.</param>
    /// <returns>A list response object containing next and previous link and the total amount of the request.</returns>
    public static ListResponse CreateListResponse(
        this HttpContext context,
        long totalAmount,
        IPaginationSettings? query = null)
    {
        return new ListResponse
        {
            Count = totalAmount,
            NextLink = query != null && totalAmount > query.Offset + query.Limit
                ? context.Request.ToLinkUri(query.Offset + query.Limit, query.Limit)
                : string.Empty,
            PreviousLink = query != null && 0 <= query.Offset - query.Limit
                ? context.Request.ToLinkUri(query.Offset - query.Limit, query.Limit)
                : query != null && query.Offset != 0
                    ? context.Request.ToLinkUri(0, query.Limit)
                    : string.Empty
        };
    }
}
