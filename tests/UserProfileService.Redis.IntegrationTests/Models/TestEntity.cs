using System;
using System.Collections.Generic;

namespace UserProfileService.Redis.IntegrationTests.Models
{
    public class TestEntity
    {
        public string Name { get; set; }
        public DateTime Updated { get; set; }
        public double Weight { get; set; }

        public static IEqualityComparer<TestEntity> DefaultComparer { get; } = new NameUpdatedWeightEqualityComparer();

        private sealed class NameUpdatedWeightEqualityComparer : IEqualityComparer<TestEntity>
        {
            public bool Equals(TestEntity x, TestEntity y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Name == y.Name
                    && x.Updated.Equals(y.Updated)
                    && x.Weight.Equals(y.Weight);
            }

            public int GetHashCode(TestEntity obj)
            {
                return HashCode.Combine(obj.Name, obj.Updated, obj.Weight);
            }
        }
    }
}
