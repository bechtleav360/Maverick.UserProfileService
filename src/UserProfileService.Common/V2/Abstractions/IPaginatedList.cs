using System.Collections.Generic;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Represents an <see cref="IList{T}" /> that contains pagination details as additional information.
/// </summary>
/// <typeparam name="TElem">The type of each element in the collection.</typeparam>
public interface IPaginatedList<TElem> : IList<TElem>
{
    /// <summary>
    ///     Gets or sets the total amount of elements. That can differ from <see cref="List{T}.Count" />, if the pagination
    ///     settings limit the result.
    /// </summary>
    long TotalAmount { get; set; }
}
