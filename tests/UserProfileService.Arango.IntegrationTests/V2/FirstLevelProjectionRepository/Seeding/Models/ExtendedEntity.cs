using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models
{
    public class ExtendedEntity<TType> : IExtendedEntity
    {
        public TType Value { get; set; }

        public IList<TagAssignment> TagAssignments { get; set; } = new List<TagAssignment>();

        public object GetValue()
        {
            return Value;
        }
    }

    public interface IExtendedEntity
    {
        IList<TagAssignment> TagAssignments { get; }
        object GetValue();
    }

    public static class ExtendedEntityExtensions
    {
        public static bool TryGetValue<TEntity>(
            this IExtendedEntity entity,
            out TEntity val)
        {
            val = default;

            if (!(entity?.GetValue() is TEntity e))
            {
                return false;
            }

            val = e;

            return true;
        }
    }
}
