using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Common.Tests.Utilities.Utilities
{
    public class EnumHelpers
    {
        private static readonly Dictionary<Type, ObjectType> _objectTypeMapping = new Dictionary<Type, ObjectType>
        {
            { typeof(SecondLevelProjectionFunction), ObjectType.Function },
            { typeof(SecondLevelProjectionRole), ObjectType.Role },
            { typeof(SecondLevelProjectionGroup), ObjectType.Group },
            { typeof(SecondLevelProjectionUser), ObjectType.User },
            { typeof(SecondLevelProjectionOrganization), ObjectType.Organization },
            { typeof(Function), ObjectType.Function },
            { typeof(Role), ObjectType.Role },
            { typeof(Group), ObjectType.Group },
            { typeof(User), ObjectType.User },
            { typeof(Organization), ObjectType.Organization }
        };

        /// <summary>
        ///     Gets the <see cref="ObjectType" /> of a provided <typeparamref name="TEntity" /> type.
        /// </summary>
        public static ObjectType GetObjectType<TEntity>()
            where TEntity : class
        {
            return GetObjectType(typeof(TEntity));
        }

        /// <summary>
        ///     Gets the <see cref="ObjectType" /> of a provided <paramref name="entityType" />.
        /// </summary>
        public static ObjectType GetObjectType(Type entityType)
        {
            if (!_objectTypeMapping.TryGetValue(entityType, out ObjectType result))
            {
                throw new ArgumentException("The provided entity type is not supported.", nameof(entityType));
            }

            return result;
        }
    }
}
