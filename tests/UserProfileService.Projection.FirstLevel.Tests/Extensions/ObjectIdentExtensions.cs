using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Tests.Extensions
{
    internal static class ObjectIdentExtensions
    {
        public static ObjectIdent ToObjectIdent(this ObjectIdentPath objIdentPath)
        {
            return new ObjectIdent(objIdentPath.Id, objIdentPath.Type);
        }

        public static ObjectIdentPath ToObjectIdentPath(this ObjectIdent objIdentPath)
        {
            return new ObjectIdentPath(objIdentPath.Id, objIdentPath.Type);
        }
    }
}
