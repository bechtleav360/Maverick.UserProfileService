using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class CollectedArangoDbQueryResult : IArangoDbQueryResult
{
    private readonly string _countingQuery;
    private readonly string _query;

    public List<string> AffectedCollections { get; }

    internal CollectedArangoDbQueryResult(
        List<string> affectedCollections,
        string selectionString,
        string filterString,
        string returnValue,
        string limits,
        string sorting,
        string sortVariableName,
        string distinctionKey)
    {
        AffectedCollections = affectedCollections;

        _countingQuery =
            $"RETURN {{ {nameof(CountingModel.DocumentCount)}: "
            + $"LENGTH({GetQuery(selectionString, filterString, returnValue, string.Empty, distinctionKey, string.Empty, sortVariableName)}) }}";

        _query = GetQuery(
            selectionString,
            filterString,
            returnValue,
            limits,
            distinctionKey,
            sorting,
            sortVariableName);
    }

    private static string GetQuery(
        string selectionString,
        string filterString,
        string returnValue,
        string limits,
        string distinctionKey,
        string sort,
        string sortVariableName)
    {
        return $"{selectionString} {filterString} "
            + $"LET value = FIRST({returnValue}) "
            + $"COLLECT key = value.{distinctionKey} INTO grouped = value "
            + $"LET {sortVariableName} = FIRST(grouped) {sort} {limits}"
            + $" RETURN {sortVariableName} ";
    }

    /// <inheritdoc />
    public string GetQueryString()
    {
        return _query;
    }

    /// <inheritdoc />
    public string GetCountQueryString()
    {
        return _countingQuery;
    }
}
