using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class ArangoDbQueryResult : IArangoDbQueryResult
{
    private readonly string _filterString;
    private readonly string _orderByString;
    private readonly string _paginationString;
    private readonly string _returnValueString;
    private readonly string _selectionString;

    public List<string> AffectedCollections { get; }

    public ArangoDbQueryResult(
        string filterString,
        string selectionString,
        string orderByString,
        string paginationString,
        string returnValueString,
        List<string> affectedCollections)
    {
        AffectedCollections = affectedCollections;
        _filterString = filterString;
        _selectionString = selectionString;
        _orderByString = orderByString;
        _paginationString = paginationString;
        _returnValueString = returnValueString;
    }

    /// <inheritdoc />
    public string GetQueryString()
    {
        return $"{_selectionString} {_filterString} {_orderByString} {_paginationString} {_returnValueString}";
    }

    /// <inheritdoc />
    public string GetCountQueryString()
    {
        return
            $"RETURN {{ {nameof(CountingModel.DocumentCount)}: LENGTH({_selectionString} {_filterString} {_returnValueString}) }}";
    }
}
