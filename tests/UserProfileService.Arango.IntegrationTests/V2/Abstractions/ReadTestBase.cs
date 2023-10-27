using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;
using UserProfileService.Common.V2.Exceptions;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.Abstractions
{
    public abstract class ReadTestBase
    {
        protected readonly DatabaseFixture Fixture;
        protected readonly IMapper Mapper;
        protected readonly ITestOutputHelper Output;

        protected ReadTestBase(DatabaseFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
            Mapper = SampleDataTestHelper.GetDefaultTestMapper();
        }

        protected static IEnumerable<object[]> ConcatArguments(params IEnumerable<object[]>[] others)
        {
            return Enumerable.Empty<object[]>().ConcatSeveralSequences(others);
        }

        protected static QueryObject GetDefaultQueryObject(
            Filter filter,
            string searchString,
            string orderBy = "DisplayName")
        {
            return new QueryObject
            {
                Limit = 5,
                Offset = 0,
                OrderedBy = orderBy,
                SortOrder = SortOrder.Asc,
                Search = searchString,
                Filter = filter
            };
        }

        protected static AssignmentQueryObject GetDefaultAssignmentQueryObject(
            Filter filter,
            string searchString,
            string orderBy = "DisplayName")
        {
            return new AssignmentQueryObject
            {
                Limit = 5,
                Offset = 0,
                OrderedBy = orderBy,
                SortOrder = SortOrder.Asc,
                Search = searchString,
                Filter = filter
            };
        }

        protected static IEnumerable<(IQueryObject options, Type expectedExceptionType, int prio)>
            GetQueryObjectsAndExceptionTypes<TEntity>(
                string validNameValue = null,
                string defaultOrderBy = "DisplayName")

        {
            yield return (new AssignmentQueryObject(), null, 0);
            yield return (null, null, 0);

            yield return (GetDefaultAssignmentQueryObject(
                new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = "Bla"
                        }
                    }
                },
                "test",
                "Name"), typeof(ValidationException), 2);

            yield return (GetDefaultAssignmentQueryObject(new Filter(), "test", defaultOrderBy), null, 0);
            yield return (GetDefaultAssignmentQueryObject(new Filter(), "test", "Id"), null, 0);

            yield return (GetDefaultAssignmentQueryObject(
                new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = "DisplayName",
                            Values = new[] { validNameValue ?? "ad" },
                            Operator = FilterOperator.Equals,
                            BinaryOperator = BinaryOperator.Or
                        }
                    }
                },
                null,
                defaultOrderBy), GetExceptionType<TEntity>("DisplayName"), 1);

            yield return (GetDefaultAssignmentQueryObject(
                    new Filter
                    {
                        Definition = new List<Definitions>
                        {
                            new Definitions
                            {
                                FieldName = "Name",
                                Values = new[] { validNameValue ?? "ad" },
                                Operator = FilterOperator.Equals,
                                BinaryOperator = BinaryOperator.Or
                            }
                        }
                    },
                    null,
                    "Id"),
                GetExceptionType<TEntity>("Name") ?? GetExceptionType<TEntity>("Id"), 2);
        }

        protected static IEnumerable<object[]> GetInvalidIds(
            short positionId = 0,
            short positionExceptionType = 1,
            short positionOverwriteFunction = 2)
        {
            return new List<object[]>
            {
                // overwrite InstanceNotFoundException, if priority 1
                GetArgumentObject(
                    new Tuple<int, object>(positionId, "123-123-123_notValid"),
                    new Tuple<int, object>(positionExceptionType, typeof(InstanceNotFoundException)),
                    new Tuple<int, object>(positionOverwriteFunction, (Func<int, bool>)(priority => priority == 2))),
                GetArgumentObject(
                    new Tuple<int, object>(positionId, ""),
                    new Tuple<int, object>(positionExceptionType, typeof(ArgumentException)),
                    new Tuple<int, object>(positionOverwriteFunction, null)),
                GetArgumentObject(
                    new Tuple<int, object>(positionId, "    "),
                    new Tuple<int, object>(positionExceptionType, typeof(ArgumentException)),
                    new Tuple<int, object>(positionOverwriteFunction, null)),
                GetArgumentObject(
                    new Tuple<int, object>(positionId, null),
                    new Tuple<int, object>(positionExceptionType, typeof(ArgumentNullException)),
                    new Tuple<int, object>(positionOverwriteFunction, null))
            };
        }

        protected static object[] GetArgumentObject(params Tuple<int, object>[] mappingCollection)
        {
            int size = mappingCollection.Max(t => t.Item1) + 1;

            return Enumerable.Range(0, size)
                .Select(i => mappingCollection.FirstOrDefault(t => t.Item1 == i)?.Item2)
                .ToArray();
        }

        protected static Type GetExceptionType<TEntity>(string propertyName)
        {
            return PropertyNameValidFor<TEntity>(propertyName)
                ? null
                : typeof(ValidationException);
        }

        protected static bool PropertyNameValidFor<TEntity>(string propertyName)
        {
            return propertyName != null
                && typeof(TEntity)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Any(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        }

        protected static Type GetExpectedExceptionType(
            Type first,
            Type second,
            int priority,
            object forceSecond)
        {
            return GetExpectedExceptionType(first, second, (forceSecond as Func<int, bool>)?.Invoke(priority));
        }

        protected static Type GetExpectedExceptionType(
            Type first,
            Type second,
            bool? forceSecond = null)
        {
            return first != null && second != null
                ? forceSecond ?? false
                    ? second
                    : first
                : first ?? second;
        }

        public static IEnumerable<object[]> GetSearchProfileTestArguments()
        {
            yield return new object[]
            {
                GetDefaultQueryObject(
                    new Filter
                    {
                        CombinedBy = BinaryOperator.And,
                        Definition = new List<Definitions>
                        {
                            new Definitions
                            {
                                Operator = FilterOperator.GreaterThan,
                                Values = new[] { "2020-09-15T02:45:00.000Z" },
                                FieldName = "CreatedAt",
                                BinaryOperator = BinaryOperator.And
                            }
                        }
                    },
                    "a"),
                (Func<UserBasic, bool>)(u => u.MatchProperties("a")
                    && u.CreatedAt
                    > new DateTime(
                        2020,
                        09,
                        15,
                        2,
                        45,
                        0,
                        0,
                        DateTimeKind.Utc))
            };
        }
    }
}
