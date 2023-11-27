namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

public interface IArangoDbQueryResult
{
    string GetQueryString();
    string GetCountQueryString();
}
