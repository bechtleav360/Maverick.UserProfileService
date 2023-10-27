namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     Wrapper contains an <see cref="PaginatedList{TElem}" /> and the total amount of the elements
/// </summary>
public class PaginatedListResponse<T>
{
    /// <summary>
    ///     The original paginated list
    /// </summary>
    public PaginatedList<T> PaginatedList { get; set; }

    /// <summary>
    ///     Total element count of the paginated list
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    ///     Creates a new instance of <see cref="PaginatedListResponse{T}" />
    /// </summary>
    /// <param name="paginatedList">The original paginated list <see cref="PaginatedList{TElem}" /> containing list items</param>
    public PaginatedListResponse(PaginatedList<T> paginatedList)
    {
        PaginatedList = paginatedList;
        TotalCount = paginatedList?.TotalAmount ?? 0;
    }
}
