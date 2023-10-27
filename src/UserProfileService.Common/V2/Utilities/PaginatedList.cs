using System.Collections.Generic;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.Utilities;

/// <inheritdoc cref="IPaginatedList{TElem}" />
public class PaginatedList<TElem> : List<TElem>, IPaginatedList<TElem>
{
    /// <inheritdoc />
    public long TotalAmount { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="PaginatedList{TElem}" /> without setting any argument.
    /// </summary>
    public PaginatedList()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PaginatedList{TElem}" /> with a specified <see cref="IEnumerable{T}" />
    ///     and a specified total amount.
    /// </summary>
    /// <param name="elements">
    ///     The sequence of elements that will be contained in the new instance of the
    ///     <see cref="PaginatedList{TElem}" />.
    /// </param>
    public PaginatedList(IEnumerable<TElem> elements) : base(elements)
    {
        TotalAmount = Count;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PaginatedList{TElem}" /> with a specified <see cref="IEnumerable{T}" />.
    ///     The total amount will be set to the count property of the list.
    /// </summary>
    /// <param name="elements">
    ///     The sequence of elements that will be contained in the new instance of the
    ///     <see cref="PaginatedList{TElem}" />.
    /// </param>
    /// <param name="totalAmount">The total amount of elements.</param>
    public PaginatedList(
        IEnumerable<TElem> elements,
        long totalAmount) : base(elements)
    {
        TotalAmount = totalAmount;
    }
}
