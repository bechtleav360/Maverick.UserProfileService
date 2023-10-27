using System;
using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions.Models;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    public class ExtendedProfileEdgeData : SecondLevelProjectionProfileEdgeData, IProfileTreeData
    {
        private string _fromCollectionName;
        private string _toCollectionName;

        [JsonIgnore]
        public string FromId { get; set; }

        [JsonIgnore]
        public string ToId { get; set; }

        // fully-qualified id (collection name plus id)
        [JsonProperty(AConstants.SystemPropertyFrom)]
        public string FromFqnId =>
            !string.IsNullOrEmpty(_fromCollectionName)
                ? $"{_fromCollectionName}/{FromId}"
                : FromId;

        // fully-qualified id (collection name plus id)
        [JsonProperty(AConstants.SystemPropertyTo)]
        public string ToFqnId =>
            !string.IsNullOrEmpty(_toCollectionName)
                ? $"{_fromCollectionName}/{ToId}"
                : ToId;

        protected bool Equals(ExtendedProfileEdgeData other)
        {
            return FromId == other.FromId
                && ToId == other.ToId
                && RelatedProfileId == other.RelatedProfileId;
        }

        public void AddCollectionNames(string fromAndToCollection)
        {
            _fromCollectionName = _toCollectionName = fromAndToCollection;
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

            return Equals((ExtendedProfileEdgeData)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FromId, ToId, RelatedProfileId);
        }

        public override string ToString()
        {
            return $"{RelatedProfileId} - {(Conditions.AnyActive() ? "active" : "inactive")} [{FromId} -> {ToId}]";
        }
    }
}
