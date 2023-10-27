using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.QueryBuilder;

/// <summary>
///     rudimentary selectquery builder
/// </summary>
public class SelectQuery : Query
{
    /// <summary>
    /// </summary>
    public List<string> Attributes { get; set; }

    /// <summary>
    ///     condition for filtering
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    ///     Initialize a select query
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="attributes"></param>
    /// <param name="filter"></param>
    /// <param name="sortingBy"></param>
    public SelectQuery(string collectionName, List<string> attributes = null, string filter = null) : base(
        collectionName)
    {
        Attributes = attributes;
        Condition = filter;
    }

    /// <summary>
    ///     Add filter statement to the query
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private string AddFilterStatement(string filter)
    {
        return filter == null ? "" : "FILTER " + filter + " ";
    }

    /// <summary>
    ///     method to build the AQL query
    /// </summary>
    /// <returns></returns>
    public override string BuildRequest()
    {
        string request = "FOR doc IN " + CollectionName;
        var returnStatement = " RETURN doc";

        if (Attributes != null && Attributes.Count > 0)
        {
            returnStatement = " RETURN {";

            foreach (string attribut in Attributes)
            {
                returnStatement += attribut + ": doc." + attribut + ", ";
            }

            // delete the last comma
            returnStatement = returnStatement.Remove(returnStatement.Length - 2);
            returnStatement += "}";
        }

        request += AddFilterStatement(Condition) + returnStatement;

        return request;
    }
}
