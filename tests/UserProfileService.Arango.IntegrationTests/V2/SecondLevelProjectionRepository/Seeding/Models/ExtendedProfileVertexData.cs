using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    public class ExtendedProfileVertexData : SecondLevelProjectionProfileVertexData, IProfileTreeData
    {
        [JsonProperty(AConstants.KeySystemProperty)]
        public string Key { get; set; }

        protected bool Equals(ExtendedProfileVertexData other)
        {
            return Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ExtendedProfileVertexData)obj);
        }

        public override int GetHashCode()
        {
            return Key != null ? Key.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
