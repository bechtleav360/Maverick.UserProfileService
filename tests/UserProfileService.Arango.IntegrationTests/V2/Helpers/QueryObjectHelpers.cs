using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bogus;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    public class QueryObjectHelpers
    {
        /// <summary>
        ///     Is used to select the test case.
        /// </summary>
        public enum TestingContext
        {
            Groups,
            Users,
            Profiles,
            Functions,
            Roles,
            SecOs
        }

        private static readonly string[] _possibleFilterGroupProperties =
        {
            nameof(GroupEntityModel.DisplayName),
            nameof(GroupEntityModel.Name),
            nameof(GroupEntityModel.CreatedAt),
            nameof(GroupEntityModel.IsSystem),
            nameof(GroupEntityModel.Tags)
        };

        private static readonly int[] _possiblePaginationOffsets = { 0, 10, 25 };
        private static readonly int[] _possiblePaginationLimits = { 5, 100 };

        private static IEnumerable<Filter> GetFilters(TestingContext context)
        {
            return GetDefinitions(context)
                .Select(
                    def =>
                        new Filter
                        {
                            CombinedBy = BinaryOperator.And,
                            Definition = new List<Definitions>
                            {
                                def
                            }
                        });
        }

        private static IEnumerable<Definitions> GetDefinitions(TestingContext context)
        {
            List<(string pName, string pValue)> values = GetFilterValues(context);

            return values.Select(
                    v => new Definitions
                    {
                        BinaryOperator = BinaryOperator.Or,
                        Operator = FilterOperator.Equals,
                        FieldName = v.pName,
                        Values = new[] { v.pValue }
                    })
                .Concat(
                    values.Select(
                        v => new Definitions
                        {
                            BinaryOperator = BinaryOperator.And,
                            Operator = FilterOperator.Equals,
                            FieldName = v.pName,
                            Values = new[] { v.pValue }
                        }));
        }

        private static IEnumerable<(int offset, int limit)> GetPaginationSettings()
        {
            return _possiblePaginationOffsets
                .SelectMany(
                    offset => _possiblePaginationLimits
                        .Select(limit => (offset, limit)));
        }

        private static List<string> GetSearchValues(TestingContext context, int? amount = null)
        {
            return context switch
            {
                TestingContext.Groups => GetGroupSearchValues(amount),
                _ => null
            };
        }

        private static List<string> GetGroupSearchValues(int? amount)
        {
            List<string> groupTerms = SampleDataTestHelper.GetTestGroupEntities()
                .Select(g => CutTerm(g.Name, 4))
                .Concat(
                    SampleDataTestHelper.GetTestGroupEntities()
                        .Select(g => CutTerm(g.DisplayName, 4)))
                .ToList();

            if (amount == null)
            {
                return groupTerms;
            }

            var faker = new Faker();

            return faker.PickRandom(groupTerms, amount.Value).ToList();
        }

        private static List<(string pName, string pValue)> GetFilterValues(TestingContext context)
        {
            if (context == TestingContext.Groups)
            {
                return GetGroupFilterValues();
            }

            if (context == TestingContext.Profiles)
            {
                return GetGroupFilterValues();
            }

            return null;
        }

        private static List<(string pName, string pValue)> GetGroupFilterValues()
        {
            IEnumerable<GroupEntityModel> randomGroups = PickRandom(
                SampleDataTestHelper.GetTestGroupEntities(),
                _possibleFilterGroupProperties.Length);

            return randomGroups.Select(
                    (
                            g,
                            i)
                        => (_possibleFilterGroupProperties[i],
                            GetPropertyValueAsString(g, _possibleFilterGroupProperties[i])))
                .ToList();
        }

        private static string GetPropertyValueAsString(
            object obj,
            string propertyName)
        {
            PropertyInfo property = obj
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.Name == propertyName);

            if (property == null)
            {
                throw new Exception($"TEST: Cannot find property '{propertyName}'.");
            }

            string value = ConvertToString(property.GetValue(obj)) ?? string.Empty;

            return value;
        }

        private static string ConvertToString(object obj)
        {
            return obj switch
            {
                string s => s,
                double d => d.ToString("F"),
                int i => i.ToString("D"),
                DateTime dt => dt.ToUniversalTime().ToString("O"),
                bool b => b.ToString(),
                IEnumerable enumerable => ConvertEnumerableToString(enumerable),
                _ => obj?.ToString()
            };
        }

        private static string ConvertEnumerableToString(IEnumerable enumerable)
        {
            if (enumerable is IEnumerable<string> stringList)
            {
                return PickRandom(stringList);
            }

            if (enumerable is IEnumerable<Tag> tagList)
            {
                return PickRandom(tagList)?.Name;
            }

            if (enumerable is IEnumerable<CalculatedTag> cTagList)
            {
                return PickRandom(cTagList)?.Name;
            }

            throw new NotSupportedException($"TEST: This enumerable is not supported. {enumerable.GetType().FullName}");
        }

        private static string CutTerm(string current, int amount)
        {
            if (current == null)
            {
                return null;
            }

            if (current.Length < amount)
            {
                return current;
            }

            return current[..amount];
        }

        private static string[] GetSortingFieldNames(TestingContext context)
        {
            return context switch
            {
                TestingContext.Groups => new[]
                {
                    nameof(GroupEntityModel.Name),
                    nameof(GroupEntityModel.DisplayName),
                    nameof(GroupEntityModel.Tags),
                    nameof(GroupEntityModel.CreatedAt),
                    nameof(GroupEntityModel.Weight)
                },
                TestingContext.Users => new[]
                {
                    nameof(UserEntityModel.Name),
                    nameof(UserEntityModel.DisplayName),
                    nameof(UserEntityModel.Tags),
                    nameof(UserEntityModel.Email),
                    nameof(UserEntityModel.CreatedAt)
                },
                TestingContext.Profiles => new[]
                {
                    nameof(GroupEntityModel.Name), nameof(GroupEntityModel.DisplayName)
                },
                _ => null
            };
        }

        private static TElem PickRandom<TElem>(IEnumerable<TElem> sequence)
        {
            IEnumerable<TElem> random = PickRandom(sequence, 1);

            return random == null ? default : random.FirstOrDefault();
        }

        private static IEnumerable<TElem> PickRandom<TElem>(
            IEnumerable<TElem> sequence,
            int amount)
        {
            List<TElem> list = sequence?.ToList();

            if (list == null || list.Count == 0)
            {
                return list;
            }

            var faker = new Faker();

            return faker.PickRandom(list, amount);
        }

        public static IEnumerable<QueryObject> GetQueryObjects(TestingContext context)
        {
            var faker = new Faker();

            var ordering = new[] { SortOrder.Asc, SortOrder.Desc };

            List<string> searchTerms = GetSearchValues(context);

            return
                GetSortingFieldNames(context)
                    .Append(null)
                    .SelectMany(
                        s =>
                            ordering
                                .SelectMany(
                                    o => GetPaginationSettings()
                                        .SelectMany(
                                            p => GetFilters(context)
                                                .Select(
                                                    f => new QueryObject
                                                    {
                                                        Filter = f,
                                                        Limit = p.limit,
                                                        Offset = p.offset,
                                                        SortOrder = o,
                                                        Search = PickRandom(searchTerms)
                                                            .OrNull(faker, 0.2f),
                                                        OrderedBy = s
                                                    }))));
        }

        public static bool CheckFunctionSearchableProperties<TFunc>(TFunc function, string searchText)
            where TFunc : FunctionBasic
        {
            return !string.IsNullOrWhiteSpace(searchText)
                && function?.Name?.Equals(searchText, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool CheckRoleSearchableProperties<TRole>(TRole role, string searchText)
            where TRole : RoleBasic
        {
            return !string.IsNullOrWhiteSpace(searchText)
                && (role?.Name?.Equals(searchText, StringComparison.OrdinalIgnoreCase) == true
                    || role?.Description?.Equals(searchText, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}
