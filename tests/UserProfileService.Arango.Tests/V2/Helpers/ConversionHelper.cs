using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.Tests.V2.Helpers
{
    public static class ConversionHelper
    {
        private static Mapper GetProfileMapper()
        {
            var config =
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<GroupEntityModel, Group>();
                        cfg.CreateMap<UserEntityModel, User>();
                        cfg.CreateMap<GroupEntityModel, GroupBasic>();
                        cfg.CreateMap<UserEntityModel, UserBasic>();
                        cfg.CreateMap<User, UserBasic>();
                        cfg.CreateMap<Group, GroupBasic>();

                        cfg.CreateMap<IProfile, GroupBasic>()
                            .Include<GroupEntityModel, GroupBasic>()
                            .Include<Group, GroupBasic>();

                        cfg.CreateMap<IProfile, UserBasic>()
                            .Include<UserEntityModel, UserBasic>()
                            .Include<User, UserBasic>();
                    });

            return new Mapper(config);
        }

        public static IEnumerable<IProfile> ConvertToBasicTypeForTest(this IEnumerable<IProfile> collection)
        {
            return collection.Select(ConvertToBasicTypeForTest).ToList();
        }

        public static IProfile ConvertToBasicTypeForTest(this IProfile profile)
        {
            if (profile.Kind == ProfileKind.User)
            {
                return GetProfileMapper().Map<UserBasic>(profile);
            }

            if (profile.Kind == ProfileKind.Group)
            {
                return GetProfileMapper().Map<GroupBasic>(profile);
            }

            throw new NotSupportedException();
        }

        public static IList<TElem> AsList<TElem>(this TElem singleElement)
        {
            return singleElement != null
                ? new List<TElem>
                {
                    singleElement
                }
                : new List<TElem>();
        }
    }
}
