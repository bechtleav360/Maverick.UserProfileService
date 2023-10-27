namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal interface IArangoDbQueryResult
{
    string GetQueryString();
    string GetCountQueryString();
}
