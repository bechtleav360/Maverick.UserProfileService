using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains some informations about the execution plan (the query optimizer will return what it considers to be the
///     optimal plan).
/// </summary>
public class CursorResponsePlan
{
    /// <summary>
    ///     list of collections
    /// </summary>
    public IEnumerable<CursorResponsePlanCollection> Collections { get; set; }

    /// <summary>
    ///     total estimated cost for the plan
    /// </summary>
    public long EstimatedCost { get; set; }

    /// <summary>
    ///     Estimated number of items
    /// </summary>
    public long EstimatedNrItems { get; set; }

    /// <summary>
    ///     only true by initialization queries.
    /// </summary>
    public bool Initialize { get; set; }

    /// <summary>
    ///     Is true, by nodification queries.
    /// </summary>
    public bool IsModificationQuery { get; set; }

    /// <summary>
    ///     An array of execution nodes of the plan.
    /// </summary>
    public IEnumerable<object> Nodes { get; set; }

    /// <summary>
    ///     An array of rules the optimizer applied
    /// </summary>
    public IEnumerable<string> Rules { get; set; }

    /// <summary>
    ///     list of variables
    /// </summary>
    public IEnumerable<CursorResponsePlanVariable> Variables { get; set; }
}
