using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.MartenEventStore.UnitTests.Helpers;

public static class StreamNameResolverTestArguments
{
    public static IEnumerable<object[]> GetStreamNamesAndReferenceObjectIdents(string prefix)
    {
        yield return new object[]
        {
            $"{prefix}_S-1-23-4567-123451_user", new ObjectIdent("S-1-23-4567-123451", ObjectType.User)
        };

        yield return new object[]
        {
            $"{prefix}_47E9A525-DA0E-41B9-8020-2F43002153AE_GroUp",
            new ObjectIdent("47E9A525-DA0E-41B9-8020-2F43002153AE", ObjectType.Group)
        };

        yield return new object[]
        {
            $"{prefix}_function/123-456#cool_Function",
            new ObjectIdent("function/123-456#cool", ObjectType.Function)
        };
    }
}
