using System.ComponentModel;
using System.Reflection;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.Utilities;
using InvalidFieldTypeException = UserProfileService.Api.Common.Exceptions.InvalidFieldTypeException;

namespace UserProfileService.Extensions;

/// <summary>
///     ReadServiceExtensions used in the Api
/// </summary>
public static class ReadServiceExtensions
{
    private static IPaginatedList<KeyValuePair<string, string>> SelectKeyValueInner<T>(
        this IPaginatedList<T> list,
        Type sourceType,
        string propertyName)
    {
        List<KeyValuePair<string, string>> keyValueList = list.SelectKeyValue(sourceType, propertyName)
            .GroupBy(x => x.Key)
            .Select(x => x.First())
            .ToList();

        return keyValueList.ToPaginatedList(keyValueList.Count);
    }

    private static bool TrySelectKeyValueOfFunction(
        this IPaginatedList<FunctionView> list,
        string propertyName,
        out IPaginatedList<KeyValuePair<string, string>> keyValueList)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name cannot be null or whitespace.", nameof(propertyName));
        }

        keyValueList = default;

        // Is part of user data, can be null or empty
        if (list == null || list.Count == 0)
        {
            return false;
        }

        if (propertyName.Equals(nameof(FunctionBasic.Role), StringComparison.OrdinalIgnoreCase))
        {
            keyValueList = list.Select(
                    f => new KeyValuePair<string, string>(
                        f?.Id,
                        f?.Role?.Name))
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                .OrderBy(kv => kv.Value)
                .ToPaginatedList();

            return true;
        }

        if (propertyName.Equals(nameof(FunctionBasic.Organization), StringComparison.OrdinalIgnoreCase))
        {
            keyValueList = list.Select(
                    f => new KeyValuePair<string, string>(
                        f?.Id,
                        f?.Organization?.Name))
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                .OrderBy(kv => kv.Value)
                .ToPaginatedList();

            return true;
        }

        return false;
    }

    private static IPaginatedList<KeyValuePair<string, string>> SelectKeyValue<T>(
        this IPaginatedList<T> list,
        Type sourceType,
        string propertyName)
    {
        // Source type and property name are used internal and should be set.
        if (sourceType == null)
        {
            throw new ArgumentNullException(nameof(sourceType));
        }

        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name cannot be null or whitespace.", nameof(propertyName));
        }

        // Is part of user data, can be null or empty
        if (list == null || list.Count == 0)
        {
            return new PaginatedList<KeyValuePair<string, string>>();
        }

        // special cases
        if (list is IPaginatedList<FunctionView> funcList
            && TrySelectKeyValueOfFunction(
                funcList,
                propertyName,
                out IPaginatedList<KeyValuePair<string, string>> funcPropertyList))
        {
            return funcPropertyList;
        }

        // generic part
        PropertyInfo idProp = sourceType.GetProperties()
            .FirstOrDefault(x => string.Equals(x.Name, "Id", StringComparison.OrdinalIgnoreCase));

        PropertyInfo prop = sourceType.GetProperties()
            .FirstOrDefault(x => string.Equals(x.Name, propertyName, StringComparison.InvariantCultureIgnoreCase));

        if (prop == null || idProp == null)
        {
            throw new NotValidException(
                $"Invalid query: Key property \"{(prop == null ? propertyName : "Id")}\" not valid for type \"{sourceType.Name}\".");
        }

        return list.Select(
                x =>
                    new KeyValuePair<string, string>(idProp.GetValue(x)?.ToString(), prop.GetValue(x)?.ToString()))
            .ToPaginatedList(list.TotalAmount);
    }

    private static IPaginatedList<string> SelectProperty<T>(
        this IPaginatedList<T> list,
        Type sourceType,
        string propertyName)
    {
        // Source type and property name are used internal and should be set.
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name cannot be null or whitespace.", nameof(propertyName));
        }

        // Is part of user data, can be null or empty
        if (list == null || list.Count == 0)
        {
            return new PaginatedList<string>();
        }

        if (list is IPaginatedList<FunctionView> funcList
            && TrySelectPropertyOfFunction(funcList, propertyName, out IPaginatedList<string> stringList))
        {
            return stringList;
        }

        PropertyInfo prop = sourceType.GetProperties()
            .FirstOrDefault(x
                => string.Equals(x.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        if (prop == null)
        {
            throw new NotValidException(
                $"Invalid query: Type {sourceType.Name} does not have a property called {propertyName}.");
        }

        return list.Select(x => prop.GetValue(x)?.ToString()).ToPaginatedList(list.TotalAmount);
    }

    private static bool TrySelectPropertyOfFunction(
        this IPaginatedList<FunctionView> list,
        string propertyName,
        out IPaginatedList<string> stringList)
    {
        // Source type and property name are used internal and should be set.
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name cannot be null or whitespace.", nameof(propertyName));
        }

        stringList = default;

        // Is part of user data, can be null or empty
        if (list == null || list.Count == 0)
        {
            return false;
        }

        if (propertyName.Equals(nameof(FunctionBasic.Role), StringComparison.OrdinalIgnoreCase))
        {
            stringList = list.Select(f => f?.Role?.Name)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToPaginatedList();

            return true;
        }

        if (propertyName.Equals(nameof(FunctionBasic.Organization), StringComparison.OrdinalIgnoreCase))
        {
            stringList = list.Select(f => f?.Organization?.Name)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToPaginatedList();

            return true;
        }

        return false;
    }

    private static AssignmentQueryObject GetQueryFilter(
        PaginationData pd,
        string[] filters,
        string propertyName,
        SortOrder so = SortOrder.Asc,
        string orderBy = null)
    {
        return new AssignmentQueryObject
        {
            Offset = pd.Offset,
            Limit = pd.Limit,
            SortOrder = so,
            OrderedBy = orderBy ?? propertyName,
            Filter = filters == null || filters.Length == 0
                ? null
                : new Filter
                {
                    CombinedBy = BinaryOperator.And,
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            BinaryOperator = BinaryOperator.Or,
                            FieldName = propertyName,
                            Operator = FilterOperator.Contains,
                            Values = filters
                        }
                    }
                }
        };
    }

    /// <summary>
    ///     Returns multiple paginated lists with for the viewFilterModels (null if no valid results)
    /// </summary>
    /// <param name="r">
    ///     <see cref="IReadService" />
    /// </param>
    /// <param name="viewFilterModels">Filter which determines the models of the result</param>
    /// <returns></returns>
    public static async Task<List<object>> GetViewFilterLists(
        this IReadService r,
        IEnumerable<ViewFilterModel> viewFilterModels)
    {
        var result = new List<object>();

        foreach (ViewFilterModel viewFilterModel in viewFilterModels)
        {
            switch (viewFilterModel.Type)
            {
                case ViewFilterTypes.KeyValue:
                    result.Add(
                        await r.GetKeyValueList(
                            viewFilterModel.DataStoreContext,
                            viewFilterModel.Pagination,
                            viewFilterModel.Filter,
                            viewFilterModel.FieldName));

                    break;
                case ViewFilterTypes.String:
                    result.Add(
                        await r.GetUniqueStringList(
                            viewFilterModel.DataStoreContext,
                            viewFilterModel.Pagination,
                            viewFilterModel.Filter,
                            viewFilterModel.FieldName));

                    break;
                case ViewFilterTypes.Date:
                    result.Add(await r.GetDateRange(viewFilterModel.DataStoreContext, viewFilterModel.FieldName));

                    break;
            }
        }

        return result.Count > 0 ? result : null;
    }

    /// <summary>
    ///     Returns a KeyValue PaginatedList (The Id is always used as Key)
    /// </summary>
    /// <param name="r">
    ///     <see cref="IReadService" />
    /// </param>
    /// <param name="context">
    ///     <see cref="ViewFilterDataStoreContext" />
    /// </param>
    /// <param name="pd">
    ///     <see cref="PaginationData" />
    /// </param>
    /// <param name="filter">Values which should be used as filter</param>
    /// <param name="property">PropertyName that should be used as Value</param>
    /// <returns>A <see cref="PaginatedList{TElem}" /> where the Key is the Id and the Value the given property</returns>
    public static async Task<IPaginatedList<KeyValuePair<string, string>>> GetKeyValueList(
        this IReadService r,
        ViewFilterDataStoreContext context,
        PaginationData pd,
        string[] filter,
        string property)
    {
        IPaginatedList<KeyValuePair<string, string>> result;

        switch (context)
        {
            case ViewFilterDataStoreContext.Functions:
                result = (await r.GetFunctionsAsync<FunctionView>(GetQueryFilter(pd, filter, property)))
                    .SelectKeyValue(typeof(FunctionView), property);

                break;
            case ViewFilterDataStoreContext.Groups:
                result = (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        RequestedProfileKind.Group,
                        GetQueryFilter(pd, filter, property)))
                    .SelectKeyValue(typeof(GroupBasic), property);

                break;
            case ViewFilterDataStoreContext.Roles:
                result = (await r.GetRolesAsync<RoleBasic>(GetQueryFilter(pd, filter, property)))
                    .SelectKeyValue(typeof(RoleBasic), property);

                break;
            case ViewFilterDataStoreContext.User:
                result = (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        RequestedProfileKind.User,
                        GetQueryFilter(pd, filter, property)))
                    .SelectKeyValue(typeof(UserBasic), property);

                break;
            case ViewFilterDataStoreContext.Organization:
                result = (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        RequestedProfileKind.Organization,
                        GetQueryFilter(pd, filter, property)))
                    .SelectKeyValue(typeof(OrganizationBasic), property);

                break;
            case ViewFilterDataStoreContext.FunctionRoles:
                IPaginatedList<FunctionView> functionRoles =
                    await r.GetFunctionsAsync<FunctionView>(
                        GetQueryFilter(
                            pd,
                            filter,
                            $"{nameof(FunctionBasic.Role)}.{property}"));

                result = functionRoles.Select(x => x.Role)
                    .ToPaginatedList(functionRoles.TotalAmount)
                    .SelectKeyValueInner(typeof(RoleBasic), property);

                break;
            case ViewFilterDataStoreContext.FunctionOrgUnits:
                IPaginatedList<FunctionView> functionOrgUnits =
                    await r.GetFunctionsAsync<FunctionView>(
                        GetQueryFilter(
                            pd,
                            filter,
                            $"{nameof(FunctionBasic.Organization)}.{property}"));

                result = functionOrgUnits.Select(x => x.Organization)
                    .ToPaginatedList(functionOrgUnits.TotalAmount)
                    .SelectKeyValueInner(typeof(OrganizationBasic), property);

                break;
            default:
                result = new PaginatedList<KeyValuePair<string, string>>();
                break;
        }

        return result.ToPaginatedList(result.TotalAmount);
    }

    /// <summary>
    ///     Returns a unique string PaginatedList
    /// </summary>
    /// <param name="r">
    ///     <see cref="IReadService" />
    /// </param>
    /// <param name="context">
    ///     <see cref="ViewFilterDataStoreContext" />
    /// </param>
    /// <param name="pd">
    ///     <see cref="PaginationData" />
    /// </param>
    /// <param name="filter">Values which should be used as filter</param>
    /// <param name="property">PropertyName that should be used as Value</param>
    /// <returns>A <see cref="PaginatedList{TElem}" /> with unique strings</returns>
    public static async Task<IPaginatedList<string>> GetUniqueStringList(
        this IReadService r,
        ViewFilterDataStoreContext context,
        PaginationData pd,
        string[] filter,
        string property)
    {
        IPaginatedList<string> result;

        switch (context)
        {
            case ViewFilterDataStoreContext.Functions:
                result = (await r.GetFunctionsAsync<FunctionView>(GetQueryFilter(pd, filter, property)))
                    .SelectProperty(typeof(FunctionView), property);

                break;
            case ViewFilterDataStoreContext.Groups:
                result = (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        RequestedProfileKind.Group,
                        GetQueryFilter(pd, filter, property)))
                    .SelectProperty(typeof(GroupBasic), property);

                break;
            case ViewFilterDataStoreContext.Roles:
                result = (await r.GetRolesAsync<RoleBasic>(GetQueryFilter(pd, filter, property)))
                    .SelectProperty(typeof(RoleBasic), property);

                break;
            case ViewFilterDataStoreContext.User:
                result = (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        RequestedProfileKind.User,
                        GetQueryFilter(pd, filter, property)))
                    .SelectProperty(typeof(UserBasic), property);

                break;
            case ViewFilterDataStoreContext.Organization:
                result = (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        RequestedProfileKind.Organization,
                        GetQueryFilter(pd, filter, property)))
                    .SelectProperty(typeof(OrganizationBasic), property);

                break;
            case ViewFilterDataStoreContext.FunctionRoles:
                IPaginatedList<FunctionView> roles = await r.GetFunctionsAsync<FunctionView>(
                    GetQueryFilter(
                        pd,
                        filter,
                        $"{nameof(FunctionBasic.Role)}.{property}"));

                result = roles.Select(x => x.Role)
                    .ToPaginatedList(roles.TotalAmount)
                    .SelectProperty(typeof(RoleBasic), property)
                    .Distinct()
                    .ToList()
                    .ToPaginatedList();

                break;
            case ViewFilterDataStoreContext.FunctionOrgUnits:
                IPaginatedList<FunctionView> orgUnits = await r.GetFunctionsAsync<FunctionView>(
                    GetQueryFilter(
                        pd,
                        filter,
                        $"{nameof(FunctionBasic.Organization)}.{property}"));

                result = orgUnits.Select(x => x.Organization)
                    .ToPaginatedList(orgUnits.TotalAmount)
                    .SelectProperty(typeof(OrganizationBasic), property)
                    .Distinct()
                    .ToList()
                    .ToPaginatedList();

                break;
            default:
                throw new InvalidEnumArgumentException("Could not parse ViewFilterDataStoreContext.");
        }

        return result?.Distinct().ToPaginatedList(result.TotalAmount);
    }

    /// <summary>
    ///     Returns a List of DateTimes where the first value is the minimal Value and the last the maximum Value
    /// </summary>
    /// <param name="r">
    ///     <see cref="IReadService" />
    /// </param>
    /// <param name="context">
    ///     <see cref="ViewFilterDataStoreContext" />
    /// </param>
    /// <param name="property">PropertyName that should be used as Value (needs to be DateTime)</param>
    /// <returns></returns>
    public static async Task<IEnumerable<DateTime>> GetDateRange(
        this IReadService r,
        ViewFilterDataStoreContext context,
        string property)
    {
        Type sourceType = context switch
        {
            ViewFilterDataStoreContext.Functions => typeof(FunctionBasic),
            ViewFilterDataStoreContext.Groups => typeof(GroupBasic),
            ViewFilterDataStoreContext.User => typeof(UserBasic),
            ViewFilterDataStoreContext.Roles => typeof(RoleBasic),
            ViewFilterDataStoreContext.Organization => typeof(GroupBasic),
            ViewFilterDataStoreContext.FunctionRoles => typeof(RoleBasic),
            ViewFilterDataStoreContext.FunctionOrgUnits => typeof(OrganizationBasic),
            _ => throw new InvalidEnumArgumentException("Could not parse ViewFilterDataStoreContext.")
        };

        PropertyInfo prop = sourceType.GetProperties()
            .FirstOrDefault(x => string.Equals(x.Name, property,
                StringComparison.OrdinalIgnoreCase));

        if (prop == null)
        {
            throw new NotValidException($"Invalid Property \"{property}\" for Type {sourceType.Name}.");
        }

        if (prop.PropertyType != typeof(DateTime))
        {
            throw new InvalidFieldTypeException(
                $"Property \"{property}\" has to be of Type DateTime (PropertyType: {prop.PropertyType})");
        }

        var result = new List<DateTime>();
        DateTime min;
        DateTime max;

        switch (context)
        {
            case ViewFilterDataStoreContext.Functions:
                min = prop.GetValue(
                        (await r.GetFunctionsAsync<FunctionView>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Asc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MinValue;

                max = prop.GetValue(
                        (await r.GetFunctionsAsync<FunctionView>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Desc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MaxValue;

                break;
            case ViewFilterDataStoreContext.Groups:
                min = prop.GetValue(
                        (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                            RequestedProfileKind.Group,
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Asc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MinValue;

                max = prop.GetValue(
                        (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                            RequestedProfileKind.Group,
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Desc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MaxValue;

                break;
            case ViewFilterDataStoreContext.Roles:
                min = prop.GetValue(
                        (await r.GetRolesAsync<RoleBasic>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Asc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MinValue;

                max = prop.GetValue(
                        (await r.GetRolesAsync<RoleBasic>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Desc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MaxValue;

                break;
            case ViewFilterDataStoreContext.User:
                min = prop.GetValue(
                        (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                            RequestedProfileKind.User,
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Asc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MinValue;

                max = prop.GetValue(
                        (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                            RequestedProfileKind.User,
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Desc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MaxValue;

                break;
            case ViewFilterDataStoreContext.Organization:
                min = prop.GetValue(
                        (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                            RequestedProfileKind.Organization,
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Asc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MinValue;

                max = prop.GetValue(
                        (await r.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                            RequestedProfileKind.Organization,
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Desc,
                                property))).FirstOrDefault()) as DateTime?
                    ?? DateTime.MaxValue;

                break;
            case ViewFilterDataStoreContext.FunctionRoles:
                min = prop.GetValue(
                        (await r.GetFunctionsAsync<FunctionView>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Asc,
                                $"{nameof(FunctionView.Role)}.{property}")))
                        .Select(x => x.Role)
                        .FirstOrDefault()) as DateTime?
                    ?? DateTime.MinValue;

                max = prop.GetValue(
                        (await r.GetFunctionsAsync<FunctionView>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Desc,
                                $"{nameof(FunctionView.Role)}.{property}")))
                        .Select(x => x.Role)
                        .FirstOrDefault()) as DateTime?
                    ?? DateTime.MaxValue;

                break;
            case ViewFilterDataStoreContext.FunctionOrgUnits:
                min = prop.GetValue(
                        (await r.GetFunctionsAsync<FunctionView>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Asc,
                                $"{nameof(FunctionView.Organization)}.{property}")))
                        .Select(x => x.Organization)
                        .FirstOrDefault()) as DateTime?
                    ?? DateTime.MinValue;

                max = prop.GetValue(
                        (await r.GetFunctionsAsync<FunctionView>(
                            GetQueryFilter(
                                new PaginationData
                                {
                                    Limit = 1,
                                    Offset = 0
                                },
                                null,
                                null,
                                SortOrder.Desc,
                                $"{nameof(FunctionView.Organization)}.{property}")))
                        .Select(x => x.Organization)
                        .FirstOrDefault()) as DateTime?
                    ?? DateTime.MaxValue;

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), context, "Value is out of range and cannot be parsed.");
        }

        result.Add(min);
        result.Add(max);

        return result;
    }
}
