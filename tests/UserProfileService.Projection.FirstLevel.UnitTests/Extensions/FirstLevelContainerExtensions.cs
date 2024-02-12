using System.Collections.Generic;
using System.Linq;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Extensions
{
    internal static class FirstLevelContainerExtensions
    {
        internal static List<IFirstLevelProjectionContainer> ToFirstLevelContainerList(
            this List<FirstLevelProjectionGroup> list)
        {
            return list.Select(gr => (IFirstLevelProjectionContainer)gr).ToList();
        }

        internal static List<IFirstLevelProjectionContainer> ToFirstLevelContainerList(
            this List<FirstLevelProjectionFunction> list)
        {
            return list.Select(fr => (IFirstLevelProjectionContainer)fr).ToList();
        }

        internal static List<IFirstLevelProjectionContainer> ToFirstLevelContainerList(
            this List<FirstLevelProjectionRole> list)
        {
            return list.Select(rl => (IFirstLevelProjectionContainer)rl).ToList();
        }

        internal static List<IFirstLevelProjectionProfile> ToFirstLevelProfileList(
            this List<FirstLevelProjectionGroup> list)
        {
            return list.Select(gr => (IFirstLevelProjectionProfile)gr).ToList();
        }

        internal static List<IFirstLevelProjectionProfile> ToFirstLevelProfileList(
            this List<FirstLevelProjectionUser> list)
        {
            return list.Select(usr => (IFirstLevelProjectionProfile)usr).ToList();
        }

        internal static List<IFirstLevelProjectionProfile> ToFirstLevelProfileList(
            this List<FirstLevelProjectionOrganization> list)
        {
            return list.Select(org => (IFirstLevelProjectionProfile)org).ToList();
        }
    }
}
