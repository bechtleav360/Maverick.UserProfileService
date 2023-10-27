using System.Collections.Generic;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

namespace UserProfileService.Arango.Tests.V2.Mocks
{
    public class TestArangoDbQueryResult : IArangoDbQueryResult
    {
        /// <inheritdoc />
        public List<string> AffectedCollections { get; } = new List<string>();

        /// <inheritdoc />
        public string GetQueryString()
        {
            return "myQuery";
        }

        /// <inheritdoc />
        public string GetCountQueryString()
        {
            return "myCountingQuery";
        }
    }
}
