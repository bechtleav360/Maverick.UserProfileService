using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Arango.UnitTests.V2.TestModels
{
    public class TestEntity
    {
        public string Id { get; set; }

        [Searchable]
        public string Type { get; set; }

        public int Weight { get; set; }

        [Searchable]
        public string FirstName { get; set; }

        [Searchable]
        public string LastName { get; set; }

        public DateTime BirthDay { get; set; }

        public List<TestEntityTag> Tags { get; set; }

        public string[] Characteristics { get; set; }
    }

    public class TestEntityTag
    {
        public string Id { get; set; }

        [DefaultFilterValue]
        public string Name { get; set; }
    }
}
