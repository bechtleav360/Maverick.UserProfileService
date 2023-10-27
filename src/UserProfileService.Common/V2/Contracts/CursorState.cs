using System;
using System.Collections.Generic;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Represents a state object about a cursor.
/// </summary>
public class CursorState
{
    /// <summary>
    ///     The time when the cursor will expire.
    /// </summary>
    public DateTimeOffset ExpirationTime { get; set; }

    /// <summary>
    ///     A boolean value indicating whether there are more result item sets or not (relatively to the current cursor of the
    ///     complete result set).
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    ///     The cursor id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The index of the last item.
    /// </summary>
    public int LastItem { get; set; }

    /// <summary>
    ///     The page size of each result set.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     The type name of each payload element.
    /// </summary>
    public string PayloadType { get; set; }

    /// <summary>
    ///     The total amount of the complete query.
    /// </summary>
    public int TotalAmount { get; set; }
}

/// <summary>
///     Represents a state object about a cursor including a payload.
/// </summary>
/// <typeparam name="TPayload">The type of each item in the payload.</typeparam>
public class CursorState<TPayload> : CursorState
{
    /// <summary>
    ///     Returns an empty <see cref="CursorState{TPalyoad}" /> object.
    /// </summary>
    public static CursorState<TPayload> Empty => new CursorState<TPayload>();

    /// <summary>
    ///     A sequence of objects that are stored in the cursor state.
    /// </summary>
    public IEnumerable<TPayload> Payload { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="CursorState{TPayload}" /> without any parameter.
    /// </summary>
    public CursorState()
    {
        PayloadType = typeof(TPayload).Name;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="CursorState{TPayload}" /> with a given <paramref name="old" /> state and a
    ///     payload.
    /// </summary>
    /// <param name="old"></param>
    /// <param name="payload"></param>
    public CursorState(
        CursorState old,
        IEnumerable<TPayload> payload)
    {
        Id = old.Id;
        ExpirationTime = old.ExpirationTime;
        HasMore = old.HasMore;
        PageSize = old.PageSize;
        LastItem = old.LastItem;
        TotalAmount = old.TotalAmount;
        Payload = payload;
        PayloadType = typeof(TPayload).Name;
    }
}
