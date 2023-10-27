using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.V2.Contracts;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class ConversionUtilities
{
    private static IMapper _defaultMapper;

    private static IMapper DefaultArangoMapper =>
        _defaultMapper ??= new MapperConfiguration(
                cfg =>
                {
                    CreateGroupMap(cfg);
                    CreateUserMap(cfg);
                    CreateOrganizationMap(cfg);
                    CreateFunctionMap(cfg);
                    CreateRoleMap(cfg);
                    cfg.CreateMap<AssignmentEntityModel, Assignment>();
                })
            .CreateMapper();

    private static void CreateUserMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<UserBasic, Member>();
        cfg.CreateMap<UserEntityModel, Member>();
        cfg.CreateMap<UserEntityModel, UserBasic>();

        cfg.CreateMap<UserEntityModel, User>()
            .IncludeBase<UserEntityModel, UserBasic>()
            .ForMember(u => u.CustomPropertyUrl, expression => expression.Ignore())
            .ForMember(u => u.ImageUrl, expression => expression.Ignore())
            .ForMember(u => u.TagUrl, expression => expression.Ignore());

        cfg.CreateMap<UserEntityModel, UserView>()
            .IncludeBase<UserEntityModel, UserBasic>()
            .ForMember(
                u => u.Functions,
                expression => expression.MapFrom(
                    (userEntity, userView) =>
                        userView.Functions = userEntity?.SecurityAssignments?.OfType<LinkedFunctionObject>()
                                .ToList<ILinkedObject>()
                            ?? new List<ILinkedObject>()))
            .ForMember(u => u.MemberOf, expression => expression.NullSubstitute(new List<Member>()));

        cfg.CreateMap<UserEntityModel, IProfile>().As<UserBasic>();

        cfg.CreateMap<UserEntityModel, ConditionalUser>()
            .IncludeBase<UserEntityModel, UserView>()
            .ForMember(
                o => o.Conditions,
                expression => expression.NullSubstitute(new List<RangeCondition>()))
            .ForMember(
                o => o.IsActive,
                expression =>
                    expression.MapFrom((entity, user) => user.IsActive = entity.Conditions?.Any() ?? false));
    }

    private static void CreateGroupMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<GroupBasic, Member>();
        cfg.CreateMap<GroupEntityModel, Member>();
        cfg.CreateMap<GroupEntityModel, GroupBasic>();

        cfg.CreateMap<GroupEntityModel, Group>()
            .IncludeBase<GroupEntityModel, GroupBasic>()
            .ForMember(grp => grp.ImageUrl, expression => expression.Ignore())
            .ForMember(grp => grp.TagUrl, expression => expression.Ignore())
            .ForMember(
                grp => grp.Members,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            grp.Members = context.Mapper.Map<IList<Member>>(
                                entity.Members.Where(m => m.Kind != ProfileKind.Organization))))
            .ForMember(
                grp => grp.MemberOf,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            grp.MemberOf = context.Mapper.Map<IList<Member>>(
                                entity.MemberOf.Where(m => m.Kind != ProfileKind.Organization))));

        cfg.CreateMap<GroupEntityModel, GroupView>()
            .IncludeBase<GroupEntityModel, GroupBasic>()
            .ForMember(
                grp => grp.HasChildren,
                expression => expression
                    .MapFrom(
                        (entity, grp) =>
                            grp.HasChildren = entity.Members?.Count(m => m.Kind != ProfileKind.Organization) > 0))
            .ForMember(
                grp => grp.ChildrenCount,
                expression => expression
                    .MapFrom(
                        (entity, grp) =>
                            grp.ChildrenCount =
                                entity.Members?.Count(m => m.Kind != ProfileKind.Organization) ?? 0))
            .ForMember(
                grp => grp.Tags,
                expression => expression
                    .MapFrom((entity, grp) => grp.Tags = entity.Tags?.Select(t => t.Name).ToList()));

        cfg.CreateMap<GroupEntityModel, IProfile>()
            .As<GroupBasic>();

        cfg.CreateMap<GroupEntityModel, IContainerProfile>()
            .As<GroupBasic>();

        cfg.CreateMap<GroupEntityModel, ConditionalGroup>()
            .IncludeBase<GroupEntityModel, GroupView>()
            .ForMember(
                o => o.Conditions,
                expression => expression.NullSubstitute(new List<RangeCondition>()))
            .ForMember(
                o => o.IsActive,
                expression =>
                    expression.MapFrom((entity, group) => group.IsActive = entity.Conditions?.Any() ?? false));
    }

    private static void CreateOrganizationMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<OrganizationBasic, Member>();
        cfg.CreateMap<OrganizationEntityModel, Member>();
        cfg.CreateMap<OrganizationEntityModel, OrganizationBasic>();

        cfg.CreateMap<OrganizationEntityModel, Organization>()
            .IncludeBase<OrganizationEntityModel, OrganizationBasic>()
            .ForMember(org => org.CustomPropertyUrl, expression => expression.Ignore())
            .ForMember(org => org.ImageUrl, expression => expression.Ignore())
            .ForMember(org => org.TagUrl, expression => expression.Ignore())
            .ForMember(
                grp => grp.Members,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            grp.Members = context.Mapper.Map<IList<Member>>(
                                entity.Members.Where(m => m.Kind == ProfileKind.Organization))))
            .ForMember(
                grp => grp.MemberOf,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            grp.MemberOf = context.Mapper.Map<IList<Member>>(
                                entity.MemberOf.Where(m => m.Kind == ProfileKind.Organization))));

        cfg.CreateMap<OrganizationEntityModel, OrganizationView>()
            .IncludeBase<OrganizationEntityModel, OrganizationBasic>()
            .ForMember(
                org => org.HasChildren,
                expression => expression
                    .MapFrom(
                        (entity, org) => org.HasChildren = entity
                                .Members?.Count(
                                    m =>
                                        m.Kind == ProfileKind.Organization)
                            > 0))
            .ForMember(
                org => org.ChildrenCount,
                expression => expression
                    .MapFrom(
                        (entity, org) => org.ChildrenCount = entity
                                .Members?.Count(
                                    m =>
                                        m.Kind == ProfileKind.Organization)
                            ?? 0))
            .ForMember(
                org => org.Tags,
                expression => expression
                    .MapFrom((entity, org) => org.Tags = entity.Tags?.Select(t => t.Name).ToList()));

        cfg.CreateMap<OrganizationEntityModel, IProfile>()
            .As<OrganizationBasic>();

        cfg.CreateMap<OrganizationEntityModel, IContainerProfile>()
            .As<OrganizationBasic>();

        cfg.CreateMap<OrganizationEntityModel, ConditionalOrganization>()
            .IncludeBase<OrganizationEntityModel, OrganizationView>()
            .ForMember(
                o => o.Conditions,
                expression => expression.NullSubstitute(new List<RangeCondition>()))
            .ForMember(
                o => o.IsActive,
                expression =>
                    expression.MapFrom((entity, org) => org.IsActive = entity.Conditions?.Any() ?? false));
    }

    private static void CreateRoleMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<RoleObjectEntityModel, RoleBasic>();

        cfg.CreateMap<RoleObjectEntityModel, LinkedRoleObject>()
            .ForMember(r => r.Conditions, expression => expression.NullSubstitute(new List<RangeCondition>()))
            .ForMember(
                r => r.IsActive,
                expression =>
                    expression.MapFrom((entity, role) => role.IsActive = entity.Conditions?.Any() ?? false));

        cfg.CreateMap<RoleObjectEntityModel, RoleView>()
            .ForMember(
                r => r.LinkedProfiles,
                expression => expression.NullSubstitute(new List<Member>()));
    }

    private static void CreateFunctionMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<FunctionObjectEntityModel, FunctionBasic>();

        cfg.CreateMap<FunctionObjectEntityModel, LinkedFunctionObject>()
            .ForMember(f => f.Conditions, expression => expression.NullSubstitute(new List<RangeCondition>()))
            .ForMember(f => f.RoleName, expression => expression.MapFrom(p => p.Role.Name))
            .ForMember(f => f.OrganizationName, expression => expression.MapFrom(p => p.Organization.Name))
            .ForMember(
                f => f.IsActive,
                expression =>
                    expression.MapFrom((entity, func) => func.IsActive = entity.Conditions?.Any() ?? false));

        cfg.CreateMap<FunctionObjectEntityModel, FunctionView>()
            .ForMember(
                f => f.LinkedProfiles,
                expression => expression.NullSubstitute(new List<Member>()));

        cfg.CreateMap<FunctionBasic, LinkedFunctionObject>()
            .ForMember(f => f.RoleName, expression => expression.MapFrom(p => p.Role.Name))
            .ForMember(f => f.OrganizationName, expression => expression.MapFrom(p => p.Organization.Name));
    }

    internal static IEnumerable<IContainerProfile> ToSpecifiedContainerProfileModels<TGroupResult,
        TOrganizationResult>(
        this IEnumerable<IProfileEntityModel> profiles,
        bool includeInactiveAssignments = true)
        where TGroupResult : IContainerProfile
        where TOrganizationResult : IContainerProfile
    {
        return profiles?.Select(profile => includeInactiveAssignments ? profile : RemoveInvalidAssignments(profile))
                .Select(ToSpecifiedContainerProfileModels<TGroupResult, TOrganizationResult>)
                .Where(converted => converted != null)
            ?? new List<IContainerProfile>();
    }

    internal static IEnumerable<IProfile> ToSpecifiedProfileModels<TUserResult, TGroupResult, TOrganizationResult>(
        this IEnumerable<IProfileEntityModel> profile,
        bool includeInactiveAssignments = true)
        where TUserResult : IProfile
        where TGroupResult : IContainerProfile
        where TOrganizationResult : IContainerProfile
    {
        return profile?.Select(e => includeInactiveAssignments ? e : RemoveInvalidAssignments(e))
                .Select(ToSpecifiedProfileModel<TUserResult, TGroupResult, TOrganizationResult>)
                .Where(converted => converted != null)
            ?? new List<IProfile>();
    }

    internal static IEnumerable<Member> ToMemberModels(
        this IEnumerable<IProfileEntityModel> profile)
    {
        return profile?.Select(ToMemberModel)
                .Where(converted => converted != null)
            ?? new List<Member>();
    }

    internal static IEnumerable<IAssignmentObject> ToSpecifiedModel<TRole, TFunction>(
        this IEnumerable<IAssignmentObjectEntity> objects)
        where TRole : IAssignmentObject
        where TFunction : IAssignmentObject
    {
        return objects?.Select(ToSpecifiedModel<TRole, TFunction>).Where(converted => converted != null)
            ?? new List<IAssignmentObject>();
    }

    internal static IContainerProfile ToSpecifiedContainerProfileModels<TGroupResult, TOrganizationResult>(
        IProfileEntityModel profile)
        where TGroupResult : IContainerProfile
        where TOrganizationResult : IContainerProfile
    {
        return profile switch
        {
            GroupEntityModel groupEntityModel => DefaultArangoMapper.Map<TGroupResult>(groupEntityModel),
            OrganizationEntityModel organizationEntityModel => DefaultArangoMapper.Map<TOrganizationResult>(
                organizationEntityModel),
            null => default,
            _ => throw new NotSupportedException(
                $"The type '{profile.GetType()}' is not supported by this method (as container profile type).")
        };
    }

    internal static IProfile ToSpecifiedProfileModel<TUserResult, TGroupResult, TOrganizationResult>(
        this IProfileEntityModel profile)
        where TUserResult : IProfile
        where TGroupResult : IContainerProfile
        where TOrganizationResult : IContainerProfile
    {
        return profile switch
        {
            GroupEntityModel groupEntityModel => DefaultArangoMapper.Map<TGroupResult>(groupEntityModel),
            UserEntityModel userEntity => DefaultArangoMapper.Map<TUserResult>(userEntity),
            OrganizationEntityModel organizationEntityModel => DefaultArangoMapper.Map<TOrganizationResult>(
                organizationEntityModel),
            null => default,
            _ => throw new NotSupportedException($"The type '{profile.GetType()}' is not supported by this method.")
        };
    }

    internal static TResult ToSpecifiedProfileModel<TResult>(
        this IProfileEntityModel profile,
        bool includeInactiveAssignments = true)
        where TResult : IProfile
    {
        if (!includeInactiveAssignments)
        {
            RemoveInvalidAssignments(profile);
        }

        return profile switch
        {
            GroupEntityModel groupEntityModel => DefaultArangoMapper.Map<TResult>(groupEntityModel),
            UserEntityModel userEntity => DefaultArangoMapper.Map<TResult>(userEntity),
            OrganizationEntityModel organizationEntityModel => DefaultArangoMapper.Map<TResult>(
                organizationEntityModel),
            null => default,
            _ => throw new NotSupportedException($"The type '{profile.GetType()}' is not supported by this method.")
        };
    }

    internal static Member ToMemberModel(this IProfileEntityModel profile)
    {
        return profile switch
        {
            GroupEntityModel groupEntityModel => DefaultArangoMapper.Map<Member>(groupEntityModel),
            UserEntityModel userEntity => DefaultArangoMapper.Map<Member>(userEntity),
            OrganizationEntityModel organizationEntityModel => DefaultArangoMapper.Map<Member>(organizationEntityModel),
            null => default,
            _ => throw new NotSupportedException($"The type '{profile.GetType()}' is not supported by this method.")
        };
    }

    internal static User ToUserProfile(this UserEntityModel userEntity)
    {
        return userEntity == null 
            ? default
            : DefaultArangoMapper.Map<User>(userEntity);
    }

    internal static OrganizationBasic EnsureBasicOrganizationProfile<TOrganization>(this TOrganization organization)
        where TOrganization : OrganizationBasic
    {
        if (organization == null)
        {
            return default;
        }

        if (organization is OrganizationEntityModel organizationEntity)
        {
            return DefaultArangoMapper.Map<OrganizationBasic>(organizationEntity);
        }

        return organization;
    }

    internal static LinkedRoleObject ToConditionalRole(this RoleObjectEntityModel roleEntity)
    {
        return roleEntity == null
            ? default
            : DefaultArangoMapper.Map<LinkedRoleObject>(roleEntity);
    }

    internal static LinkedFunctionObject ToConditionalFunction(this FunctionObjectEntityModel functionEntity)
    {
        return functionEntity == null
            ? default
            : DefaultArangoMapper.Map<LinkedFunctionObject>(functionEntity);
    }

    internal static LinkedFunctionObject ToConditionalFunction(this FunctionBasic functionBasic)
    {
        return functionBasic == null
            ? default
            : DefaultArangoMapper.Map<LinkedFunctionObject>(functionBasic);
    }

    internal static LinkedFunctionObject SetIsActiveToTrue(this LinkedFunctionObject linkedFunctionObject)
    {
        linkedFunctionObject.IsActive = true;

        return linkedFunctionObject;
    }

    internal static TRole ToSpecifiedRoleModel<TRole>(this IAssignmentObjectEntity @object)
        where TRole : RoleView
    {
        return @object switch
        {
            RoleObjectEntityModel userEntity => DefaultArangoMapper.Map<TRole>(userEntity),
            null => default,
            _ => throw new NotSupportedException($"The type '{@object.GetType()}' is not supported by this method.")
        };
    }

    internal static TFunction ToSpecifiedFunctionModel<TFunction>(this IAssignmentObjectEntity @object)
        where TFunction : FunctionView
    {
        return @object switch
        {
            FunctionObjectEntityModel functionEntityModel =>
                DefaultArangoMapper.Map<TFunction>(functionEntityModel),
            null => default,
            _ => throw new NotSupportedException($"The type '{@object.GetType()}' is not supported by this method.")
        };
    }

    internal static IAssignmentObject ToSpecifiedModel<TRole, TFunction>(this IAssignmentObjectEntity @object)
        where TRole : IAssignmentObject
        where TFunction : IAssignmentObject
    {
        return @object switch
        {
            FunctionObjectEntityModel groupEntityModel => DefaultArangoMapper.Map<TFunction>(groupEntityModel),
            RoleObjectEntityModel userEntity => DefaultArangoMapper.Map<TRole>(userEntity),
            null => default,
            _ => throw new NotSupportedException($"The type '{@object.GetType()}' is not supported by this method.")
        };
    }

    internal static bool TryConvertToType(
        this IEnumerable<string> collection,
        Type targetType,
        out IList<object> target,
        out IList<string> conversionIssues)
    {
        target = null;
        conversionIssues = new List<string>();
        var objectList = new List<object>();

        if (collection == null)
        {
            conversionIssues.Add("Collection cannot be null!");

            return false;
        }

        foreach (string element in collection)
        {
            try
            {
                objectList.Add(element.ConvertToType(targetType));
            }
            catch (FormatException fe)
            {
                conversionIssues.Add(fe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        if (objectList.Count == 0)
        {
            return false;
        }

        target = objectList;

        return true;
    }

    internal static object ConvertToType(
        this string value,
        Type targetType)
    {
        while (true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Value must be provided. I should not be null or empty or whitespace!",
                    nameof(value));
            }

            if (targetType == typeof(double))
            {
                return double.Parse(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(float))
            {
                return float.Parse(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(int))
            {
                return int.Parse(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(long))
            {
                return long.Parse(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(bool))
            {
                return bool.Parse(value);
            }

            if (targetType == typeof(DateTime))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }

            if (typeof(IEnumerable).IsAssignableFrom(targetType) && targetType.GenericTypeArguments.Length == 1)
            {
                targetType = targetType.GenericTypeArguments[0];

                continue;
            }

            return value;
        }
    }

    internal static string ConvertToAqlOperatorString(this FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.GreaterThan => ">",
            FilterOperator.GreaterThanEquals => ">=",
            FilterOperator.LowerThan => "<",
            FilterOperator.LowerThanEquals => "<=",
            FilterOperator.Equals => "==",
            FilterOperator.NotEquals => "!=",
            _ => "==" //default should be "equals to"
        };
    }

    /// <summary>
    ///     The operator should be converted, because the left and the right side of an inequation has been switched.<br />
    ///     For example: a &gt; b should become b &lt; a. The operator sign will be transferred/inverted.
    /// </summary>
    internal static FilterOperator InvertOperator(this FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.GreaterThan => FilterOperator.LowerThan,
            FilterOperator.GreaterThanEquals => FilterOperator.LowerThanEquals,
            FilterOperator.LowerThan => FilterOperator.GreaterThan,
            FilterOperator.LowerThanEquals => FilterOperator.GreaterThanEquals,
            _ => filterOperator
        };
    }

    internal static string ToEscapedStringSuitableForLikeInAql(this string current)
    {
        // Why only one escaped backslash?
        // Two backslashes are necessary, but JSON.Convert will escape each of them too.
        // So it would result in too many of them.
        return !string.IsNullOrWhiteSpace(current)
            ? current
                .Replace("%", "\\%")
                .Replace("_", "\\_")
            : current;
    }

    internal static string GetStringOfObjectForAql(object obj)
    {
        return obj switch
        {
            string s => $"\"{s.Trim('"')}\"",
            double d => d.ToString("F", CultureInfo.InvariantCulture),
            int i => i.ToString("D", CultureInfo.InvariantCulture),
            long l => l.ToString("D", CultureInfo.InvariantCulture),
            bool b => b.ToString(),
            DateTime dt => $"\"{dt.ToUniversalTime():o}\"",
            Enum anyEnum => $"\"{anyEnum:G}\"",
            IEnumerable enumerable => ConvertEnumerableToStringForAql(enumerable),
            _ => "\"\""
        };
    }

    internal static string ConvertEnumerableToStringForAql(IEnumerable enumerable)
    {
        if (enumerable is IEnumerable<object> generic)
        {
            return string.Concat(
                "[",
                string.Join(
                    ",",
                    generic.Select(GetStringOfObjectForAql)),
                "]");
        }

        return "[]";
    }

    internal static TEntity RemoveInvalidAssignments<TEntity>(TEntity profile)
        where TEntity : class, IProfileEntityModel
    {
        profile.MemberOf = profile.MemberOf.Where(m => m.IsActive).ToList();

        profile.SecurityAssignments =
            profile.SecurityAssignments.Where(m => m.IsActive).ToList();

        if (profile is IContainerProfileEntityModel container)
        {
            container.Members = container.Members.Where(m => m.IsActive).ToList();
        }

        return profile;
    }
}
