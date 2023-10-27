using System.Collections.Generic;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models
{
    public class ExtendedProfile : ExtendedEntity<IFirstLevelProjectionProfile>
    {
        public IList<Assignment> Assignments { get; set; } = new List<Assignment>();
        public IDictionary<string, string> ClientSettings { get; set; } = new Dictionary<string, string>();
    }
}
