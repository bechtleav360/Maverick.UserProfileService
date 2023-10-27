using System.Collections.Generic;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    internal class SeedObject
    {
        public string Id { get; }
        public object Entity { get; }
        public IList<SingleAssignment> Assignments { get; set; }

        // ids and inherited for all assigned tags
        public IList<TagAssignmentId> AssignedTags { get; set; }

        public SeedObject(
            string id,
            object entity)
        {
            Id = id;
            Entity = entity;
            Assignments = new List<SingleAssignment>();
            AssignedTags = new List<TagAssignmentId>();
        }
    }
}
