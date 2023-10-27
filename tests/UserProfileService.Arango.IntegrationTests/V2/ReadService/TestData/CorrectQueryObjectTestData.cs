using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;
using UserProfileService.Common.V2.Utilities;

namespace UserProfileService.Arango.IntegrationTests.V2.ReadService.TestData
{
    /// <summary>
    ///     Class containing some syntactically correct queryObjects.
    /// </summary>
    public class CorrectQueryObjectTestData : IEnumerable<object[]>
    {
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new AssignmentQueryObject
                {
                    SortOrder = SortOrder.Desc,

                    Filter = new Filter
                    {
                        CombinedBy = BinaryOperator.And,
                        Definition = new List<Definitions>
                        {
                            new Definitions
                            {
                                FieldName = "organization.name",
                                BinaryOperator = BinaryOperator.And,
                                Operator = FilterOperator.Equals,
                                Values = new[] { "solid:state" }
                            }
                        }
                    },
                    OrderedBy = "Organization.Weight"
                },
                
                new PaginatedList<FunctionView>(
                    SampleDataTestHelper.GetTestFunctionEntities()
                        .Where(f => f.Organization.Name.Contains("solid:state"))
                        .OrderByDescending(f => f.Organization?.Weight)
                        .Select(
                            f => SampleDataTestHelper.GetDefaultTestMapper()
                                .Map<FunctionView>(f))
                        .ToList())
                
                
            };

            yield return new object[]
            {
                new AssignmentQueryObject
                {
                    SortOrder = SortOrder.Asc,

                    Filter = new Filter
                    {
                        CombinedBy = BinaryOperator.And,
                        Definition = new List<Definitions>
                        {
                            new Definitions
                            {
                                FieldName = "organization.Id",
                                BinaryOperator = BinaryOperator.And,
                                Operator = FilterOperator.Equals,
                                Values = new[] { "ea" }
                            }
                        }
                    },
                    OrderedBy = "Organization.Name"
                },
                new PaginatedList<FunctionView>(
                    SampleDataTestHelper.GetTestFunctionEntities()
                        .Where(f => f.Organization.DisplayName.Contains("ea"))
                        .OrderByDescending(f => f.Organization?.Name)
                        .Reverse()
                        .Select(
                            f => SampleDataTestHelper.GetDefaultTestMapper()
                                .Map<FunctionView>(f))
                        .ToList())
            };
        }
    }
}
