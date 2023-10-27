using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Arango.Tests.V2.Helpers;
using UserProfileService.Arango.Tests.V2.TestModels;
using Xunit;
using Group = Maverick.UserProfileService.Models.Models.Group;

namespace UserProfileService.Arango.Tests.V2
{
    public class ArangoEnumerableQueryFilterTests
    {
        [Fact]
        public void Search_entities_filtered_by_query_object_shouldNotWork_definition_not_complete()
        {
            IModelBuilder newM = ModelBuilder.NewOne;

            newM.Entity<TestEntity>()
                .Collection("objects")
                .QueryCollection("objectsQuery");

            var startingPoint = new ArangoDbEnumerable<TestEntity>(newM.BuildOptions("test_"));

            Assert.Throws<ValidationException>(
                () => startingPoint.UsingOptions(
                        new QueryObject
                        {
                            Search = "test",
                            Filter = new Filter
                            {
                                Definition = new List<Definitions>
                                {
                                    new Definitions
                                    {
                                        FieldName = "noValid"
                                    }
                                }
                            }
                        })
                    .ToQuery(CollectionScope.Query));
        }

        [Fact]
        public void Search_entities_filtered_by_query_object_shouldNotWork_definition_malformed_date()
        {
            IModelBuilder newM = ModelBuilder.NewOne;

            newM.Entity<TestEntity>()
                .Collection("objects")
                .QueryCollection("objectsQuery");

            var startingPoint = new ArangoDbEnumerable<TestEntity>(newM.BuildOptions("test_"));

            var exception = Assert.Throws<ValidationException>(
                () => startingPoint.UsingOptions(
                        new QueryObject
                        {
                            Search = "test",
                            Filter = new Filter
                            {
                                Definition = new List<Definitions>
                                {
                                    new Definitions
                                    {
                                        FieldName = nameof(TestEntity.BirthDay),
                                        // the 0 before 2 for the hours is missing!
                                        Values = new[] { "2020-01-01T2:34:12.671Z" }
                                    }
                                }
                            }
                        })
                    .ToQuery(CollectionScope.Query));

            var regularExpression = new Regex(
                "Could not convert input value to appropriate property type.*BirthDay",
                RegexOptions.Singleline);

            Assert.Matches(regularExpression, exception.Message);
        }

        [Fact]
        public void SelectGroupsFilteredByMembersCount()
        {
            var startingPoint = new ArangoDbEnumerable<Group>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = $"{nameof(Group.Members)}.{nameof(Group.Members.Count)}",
                            Operator = FilterOperator.GreaterThan,
                            Values = new[] { "10" }
                        }
                    }
                }
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                "^FOR\\s+[a-z][0-9]\\s+IN\\s+Service_profilesQuery\\s+FILTER\\s*\\([a-z][0-9]\\.Kind\\s*==\\s*\"Group\"\\s+AND\\s*\\[10\\]\\s*ANY\\s*<COUNT\\(g0\\.Members\\)\\)",
                text);
        }

        [Fact]
        public void Search_users_by_query_object()
        {
            IModelBuilder modelBuilder = ModelBuilder.NewOne;

            modelBuilder.Entity<User>()
                .HasTypeIdentification("Type", "User")
                .Collection("users")
                .QueryCollection("users");

            var startingPoint = new ArangoDbEnumerable<User>(modelBuilder.BuildOptions("test_"));

            string text = startingPoint
                .UsingOptions(
                    new QueryObject
                    {
                        Search = "test",
                        Filter = new Filter
                        {
                            Definition = new List<Definitions>
                            {
                                new Definitions
                                {
                                    FieldName = nameof(User.CreatedAt),
                                    Values = new[] { "2010-01-01" }
                                }
                            }
                        }
                    })
                .ToQuery(CollectionScope.Query);

            const string pattern =
                @"^FOR u0 IN test_users FILTER\(\(u0.Type == ""User"" AND \[""2010\-01\-01T00:00:00""\] ANY ==u0.CreatedAt\)\s+" +
                @"AND \(u0.Type == ""User"" AND \(\(\(\(\(LIKE\(u0.(?:[A-Za-z]+Name|Email),""%test%"",true\)\s+" + 
                @"OR LIKE\(u0.(?:[A-Za-z]+Name|Email),""%test%"",true\)\)\s+" + 
                @"OR LIKE\(u0.(?:[A-Za-z]+Name|Email),""%test%"",true\)\)\s+" + 
                @"OR LIKE\(u0.(?:[A-Za-z]+Name|Email),""%test%"",true\)\)\s+" + 
                @"OR LIKE\(u0.(?:User)?Name,""%test%"",true\)\) OR LIKE\(u0.(?:User)?Name,""%test%"",true\)\)\)\)\s+" + 
                "LIMIT 0,100 RETURN u0$";

            Assert.Matches(pattern, text);
        }

        [Fact]
        public void Search_users_by_simple_query_object_without_filter()
        {
            IModelBuilder modelBuilder = ModelBuilder.NewOne;

            modelBuilder.Entity<User>()
                .HasTypeIdentification("type")
                .Collection("users")
                .QueryCollection("users");

            var startingPoint = new ArangoDbEnumerable<User>(modelBuilder.BuildOptions("test_"));

            string text = startingPoint
                .UsingOptions(
                    new QueryObject
                    {
                        Search = "test"
                    })
                .ToQuery(CollectionScope.Query);

            const string pattern =
                @"^FOR\s+u0\s+IN\s+test_users\s+FILTER\s*\(\s*u0\.type\s+==\s+""User""\s*" + 
                @"AND\s*\(\(\(\(\(LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\s+" + 
                @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+" + 
                @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+" + 
                @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+" + 
                @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+" + 
                @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\)\s*" + 
                @"LIMIT\s+0,100\s+RETURN\s+u0$";

            Assert.Matches(pattern, text);
        }

        [Fact]
        public void Search_entities_filtered_by_query_object()
        {
            IModelBuilder newM = ModelBuilder.NewOne;

            newM.Entity<TestEntity>()
                .HasTypeIdentification(te => te.Type)
                .Collection("objects")
                .QueryCollection("objectsQuery");

            var startingPoint = new ArangoDbEnumerable<TestEntity>(newM.BuildOptions("test_"));

            string text = startingPoint
                .UsingOptions(
                    new QueryObject
                    {
                        Search = "st",
                        Limit = 3,
                        Offset = 1,
                        OrderedBy = nameof(TestEntity.LastName),
                        SortOrder = SortOrder.Desc,
                        Filter = new Filter
                        {
                            Definition = new List<Definitions>
                            {
                                new Definitions
                                {
                                    Operator = FilterOperator.Equals,
                                    BinaryOperator = BinaryOperator.Or,
                                    FieldName = nameof(TestEntity.Tags),
                                    Values = new[] { "my#stuff", "develop" }
                                },
                                new Definitions
                                {
                                    Operator = FilterOperator.Equals,
                                    BinaryOperator = BinaryOperator.Or,
                                    FieldName = nameof(TestEntity.Characteristics),
                                    Values = new[] { "av360", "bund" }
                                },
                                new Definitions
                                {
                                    Operator = FilterOperator.GreaterThan,
                                    FieldName = nameof(TestEntity.Weight),
                                    Values = new[] { "23" }
                                },
                                new Definitions
                                {
                                    Operator = FilterOperator.LowerThanEquals,
                                    FieldName = nameof(TestEntity.BirthDay),
                                    Values = new[] { "2001-01-01", "2000-12-31" },
                                    BinaryOperator = BinaryOperator.And
                                }
                            },
                            CombinedBy = BinaryOperator.And
                        }
                    })
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                "^FOR\\s+t0\\s+IN\\s+test_objectsQuery\\s+FILTER\\(\\(t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+t0\\.Tags\\[\\*]\\.Name\\s+ANY\\s+IN\\s+\\[\"my\\#stuff\",\"develop\"]\\s*AND\\s+t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+t0\\.Characteristics\\s+ANY\\s+IN\\s*\\[\"av360\",\"bund\"]\\s*AND\\s*t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+\\[23]\\s*ANY\\s*<\\s*t0\\.Weight\\s*AND\\s+t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+\\[\"2001-01-01T00:00:00\",\"2000-12-31T00:00:00\"]\\s*ALL\\s*>=\\s*t0\\.BirthDay\\)\\s*OR\\s+t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s*\\(\\(LIKE\\(t0\\.Type,\"%st%\",true\\)\\s+OR\\s+LIKE\\(t0\\.FirstName,\"%st%\",true\\)\\)\\s+OR\\s+LIKE\\(t0\\.LastName,\"%st%\",true\\)\\)\\)\\s+SORT\\s+t0\\.LastName\\s+DESC\\s+LIMIT\\s+1,3\\s+RETURN\\s+t0$",
                text);
        }

        [Fact]
        public void Search_entities_filtered_by_query_object_combined_by_or()
        {
            IModelBuilder newM = ModelBuilder.NewOne;

            newM.Entity<TestEntity>()
                .HasTypeIdentification(te => te.Type)
                .Collection("objects")
                .QueryCollection("objectsQuery");

            var startingPoint = new ArangoDbEnumerable<TestEntity>(newM.BuildOptions("test_"));

            string text = startingPoint
                .UsingOptions(
                    new QueryObject
                    {
                        Search = "st",
                        Limit = 3,
                        Offset = 1,
                        OrderedBy = nameof(TestEntity.LastName),
                        SortOrder = SortOrder.Desc,
                        Filter = new Filter
                        {
                            Definition = new List<Definitions>
                            {
                                new Definitions
                                {
                                    Operator = FilterOperator.Equals,
                                    BinaryOperator = BinaryOperator.Or,
                                    FieldName = nameof(TestEntity.Tags),
                                    Values = new[] { "my#stuff", "develop" }
                                },
                                new Definitions
                                {
                                    Operator = FilterOperator.Equals,
                                    BinaryOperator = BinaryOperator.Or,
                                    FieldName = nameof(TestEntity.Characteristics),
                                    Values = new[] { "av360", "bund" }
                                },
                                new Definitions
                                {
                                    Operator = FilterOperator.GreaterThan,
                                    FieldName = nameof(TestEntity.Weight),
                                    Values = new[] { "23" }
                                },
                                new Definitions
                                {
                                    Operator = FilterOperator.LowerThanEquals,
                                    FieldName = nameof(TestEntity.BirthDay),
                                    Values = new[] { "2001-01-01", "2000-12-31" },
                                    BinaryOperator = BinaryOperator.And
                                }
                            },
                            CombinedBy = BinaryOperator.Or
                        }
                    })
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                "^FOR\\s+t0\\s+IN\\s+test_objectsQuery\\s+FILTER\\(\\(t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+t0\\.Tags\\[\\*]\\.Name\\s+ANY\\s+IN\\s+\\[\"my\\#stuff\",\"develop\"]\\s*OR\\s+t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+t0\\.Characteristics\\s+ANY\\s+IN\\s*\\[\"av360\",\"bund\"]\\s*OR\\s*t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+\\[23]\\s*ANY\\s*<\\s*t0\\.Weight\\s*OR\\s+t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s+\\[\"2001-01-01T00:00:00\",\"2000-12-31T00:00:00\"]\\s*ALL\\s*>=\\s*t0\\.BirthDay\\)\\s*OR\\s+t0\\.Type\\s*==\\s*\"TestEntity\"\\s+AND\\s*\\(\\(LIKE\\(t0\\.Type,\"%st%\",true\\)\\s+OR\\s+LIKE\\(t0\\.FirstName,\"%st%\",true\\)\\)\\s+OR\\s+LIKE\\(t0\\.LastName,\"%st%\",true\\)\\)\\)\\s+SORT\\s+t0\\.LastName\\s+DESC\\s+LIMIT\\s+1,3\\s+RETURN\\s+t0$",
                text);
        }

        [Fact]
        public void Search_entities_filtered_by_query_object_shouldNotWork()
        {
            IModelBuilder newM = ModelBuilder.NewOne;

            newM.Entity<TestEntity>()
                .Collection("objects")
                .QueryCollection("objectsQuery");

            var startingPoint = new ArangoDbEnumerable<TestEntity>(newM.BuildOptions("test_"));

            Assert.Throws<ValidationException>(
                () => startingPoint.UsingOptions(
                        new QueryObject
                        {
                            Filter = new Filter
                            {
                                Definition = new List<Definitions>
                                {
                                    new Definitions
                                    {
                                        FieldName = "noValid",
                                        Values = new[] { "ejal" }
                                    }
                                }
                            }
                        })
                    .ToQuery(CollectionScope.Query));
        }

        [Fact]
        public void Get_assigned_profiles_of_a_function()
        {
            var startingPoint =
                new ArangoDbEnumerable<IProfileEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Limit = 2,
                Offset = 2,
                OrderedBy = nameof(IProfileEntityModel.DisplayName),
                Search = "test",
                SortOrder = SortOrder.Asc
            };

            string text = startingPoint
                .Where(p => p.SecurityAssignments.Any(a => a.Id == "123-456"))
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            string pattern =
                @$"^FOR\s+i0\s+IN\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\s+" +
                   @"FILTER\(i0\.SecurityAssignments\[\*\]\.Id\s+ANY\s+\=\=\s+""123\-456""\s+"+
                   @"AND\s+\(\(i0\.Kind\s+\=\=\s+""User""\s+"+
                   @"AND\s+\(\(\(\(\(LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\s+"+
                   @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+"+
                   @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+"+
                   @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+"+
                   @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\s+"+
                   @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%test%"",true\)\)\)\s+"+
                   @"OR\s+\(i0\.Kind\s+\=\=\s+""Group""\s+AND\s+\(LIKE\(i0\.(?:Display)?Name,""%test%"",true\)\s+"+
                   @"OR\s+LIKE\(i0\.(?:Display)?Name,""%test%"",true\)\)\)\s*OR\s*\(i0\.Kind\s*==\s*""Organization""\s*"+
                   @"AND\s*\(LIKE\(i0\.(?:Display)?Name,""%test%"",true\)\s*"+
                   @"OR\s+LIKE\(i0\.(?:Display)?Name,""%test%"",true\)\)\)\)\)\s*"+
                   @"SORT\s+i0\.DisplayName\s+Asc\s+LIMIT\s+2,2\s+RETURN\s+i0";

            Assert.Matches(pattern, text);
        }

        [Fact]
        public void Get_group_list_filtered_by_member_ids()
        {
            var startingPoint =
                new ArangoDbEnumerable<GroupEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName =
                                $"{nameof(GroupEntityModel.Members)}.{nameof(Member.Name)}",
                            Values = new[] { "User-1", "User-2" },
                            BinaryOperator = BinaryOperator.Or,
                            Operator = FilterOperator.Equals
                        }
                    }
                }
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"^FOR\\s+g0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s*\\(\\s*g0\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s+g0\\.Members\\[\\*\\]\\.Name\\s+ANY"
                + "\\s+IN\\s+\\[\"User\\-1\",\"User\\-2\"\\]\\s*\\)\\s*LIMIT\\s+0,100\\s+RETURN\\s+g0$",
                text);
        }

        [Fact]
        public void Get_all_users_without_assigned_function_that_is_virtualProperty()
        {
            var startingPoint =
                new ArangoDbEnumerable<IProfileEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = $"{nameof(UserView.Functions)}.Count",
                            Values = new[] { "0" },
                            BinaryOperator = BinaryOperator.Or,
                            Operator = FilterOperator.Equals
                        }
                    }
                }
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"FOR\\s+u0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s*\\(\\(u0\\.Kind\\s*==\\s*\"User\"\\s*"
                + "AND\\s+\\[0\\]\\s*ANY\\s*==\\s*COUNT\\(u0\\.SecurityAssignments\\["
                + "\\*\\s+FILTER CURRENT\\.Type\\s*==\\s*\"Function\"\\]\\)\\)\\)\\s*LIMIT\\s+0,100\\s+RETURN\\s+u0",
                text);
        }

        [Fact]
        public void Get_all_users_with_specified_function_that_is_virtualProperty()
        {
            var startingPoint =
                new ArangoDbEnumerable<IProfileEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(UserView.Functions),
                            Values = new[] { "Z23" },
                            BinaryOperator = BinaryOperator.Or,
                            Operator = FilterOperator.Equals
                        }
                    }
                }
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"FOR\\s+u0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s*\\(\\(u0\\.Kind\\s*==\\s*\"User\"\\s*"
                + "AND\\s+u0\\.SecurityAssignments\\[\\*\\s+FILTER\\s+CURRENT\\.Type\\s*==\\s*\"Function\"\\]\\.Name\\s+ANY\\s+IN\\s+\\[\"Z23\"\\]\\)\\)\\s*"
                + "LIMIT\\s+0,100\\s+RETURN\\s+u0",
                text);
        }

        [Fact]
        public void Get_all_users_with_specified_function_pattern_as_virtualProperty()
        {
            var startingPoint =
                new ArangoDbEnumerable<IProfileEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(UserView.Functions),
                            Values = new[] { "Z23" },
                            BinaryOperator = BinaryOperator.Or,
                            Operator = FilterOperator.Contains
                        }
                    }
                }
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"FOR\\s+u0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s*\\(\\(u0\\.Kind\\s*==\\s*\"User\"\\s*"
                + "AND\\s*\\(FOR\\s+insideProperty\\s+IN\\s+NOT_NULL\\(u0\\.SecurityAssignments,\\[\\]\\)\\s*"
                + "\\[\\*\\s+FILTER\\s+CURRENT\\.Type\\s*==\\s*\"Function\"\\]\\s*"
                + "RETURN\\s*\\[\"Z23\"\\]\\s*"
                + "\\[\\*\\s+RETURN\\s+LIKE\\(insideProperty\\.Name,CONCAT\\(\"%\",CURRENT,\"%\"\\),true\\)\\]ANY==true\\)\\s*ANY\\s*==\\s*true\\)\\)\\s*"
                + "LIMIT\\s+0,100\\s+RETURN\\s+u0",
                text);
        }

        [Fact]
        public void Get_groups_with_specified_function_pattern_as_virtualProperty()
        {
            var startingPoint =
                new ArangoDbEnumerable<IProfileEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(GroupView.ChildrenCount),
                            Values = new[] { "5" },
                            BinaryOperator = BinaryOperator.Or,
                            Operator = FilterOperator.GreaterThan
                        }
                    }
                }
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                "FOR\\s+g0\\s+IN\\s+Service_profilesQuery\\s+"
                + "FILTER\\s*\\(\\(g0\\.Kind\\s*==\\s*\"Group\"\\s*"
                + "AND\\s+\\[5\\]\\s+ANY\\s*<\\s*COUNT\\(g0\\.Members\\[\\*\\s+FILTER\\s+\\(\\(CURRENT\\.Kind\\s*==\\s*\"Group\"\\)\\s*OR\\s+\\(CURRENT\\.Kind\\s*==\\s*\"User\"\\)\\)\\]\\.Name\\)\\)\\)\\s*"
                + "LIMIT\\s+0,100\\s+"
                + "RETURN\\s+g0",
                text);
        }

        [Fact]
        public void Get_function_filtered_by_organization()
        {
            var startingPoint =
                new ArangoDbEnumerable<FunctionObjectEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
            {
                Limit = 2,
                Offset = 2,
                SortOrder = SortOrder.Asc,

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
                            Values = new[] { "1234" }
                        }
                    }
                }
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            string pattern =
                $"FOR\\s+f0\\s+IN\\s+{"rolesFunctionsQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s*\\(f0\\.Type\\s*==\\s*\"Function\"\\s*"
                + "AND\\s\\[\"1234\"\\]\\s+ALL\\s*==\\s*f0\\.Organization\\.Name\\)\\s*"
                + "LIMIT\\s+2,2\\s+RETURN\\s+f0";

            Assert.Matches(pattern, text);
        }

        [Fact]
        public void Get_function_filtered_by_organization_and_sorted_by_organization_weight()
        {
            var startingPoint =
                new ArangoDbEnumerable<FunctionObjectEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
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
                            Values = new[] { "Z23" }
                        }
                    }
                },
                OrderedBy = "Organization.Weight"
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            string pattern =
                $"FOR\\s+f0\\s+IN\\s+{"rolesFunctionsQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s*\\(f0\\.Type\\s*==\\s*\"Function\"\\s*"
                + "AND\\s\\[\"Z23\"\\]\\s+ALL\\s*==\\s*f0\\.Organization\\.Name\\)\\s*"
                + "SORT\\s*f0\\.Organization\\.Weight\\s*DESC\\s*"
                + "LIMIT\\s+0,100\\s+RETURN\\s+f0";

            Assert.Matches(pattern, text);
        }

        [Theory]
        [InlineData("Organization.Name")]
        [InlineData("ORGANIZATION.NAME")]
        [InlineData("Organization.NAME")]
        [InlineData("ORGANization.NAME")]
        [InlineData("ORGANization.naME")]
        public void Get_function_filtered_by_organization_and_should_ignore_case_of_sort_properties(string orderedBy)
        {
            var startingPoint =
                new ArangoDbEnumerable<FunctionObjectEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
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
                            Values = new[] { "Z23" }
                        }
                    }
                },
                OrderedBy = orderedBy
            };

            string text = startingPoint
                .UsingOptions(options)
                .ToQuery(CollectionScope.Query);

            string pattern =
                $"FOR\\s+f0\\s+IN\\s+{"rolesFunctionsQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s*\\(f0\\.Type\\s*==\\s*\"Function\"\\s*"
                + "AND\\s\\[\"Z23\"\\]\\s+ALL\\s*==\\s*f0\\.Organization\\.Name\\)\\s*"
                + "SORT\\s*f0\\.Organization\\.Name\\s*DESC\\s*"
                + "LIMIT\\s+0,100\\s+RETURN\\s+f0";

            Assert.Matches(pattern, text);
        }

        [Theory]
        [InlineData("Organization.Dreck")]
        [InlineData("Organization.BUGBUSTERS")]
        [InlineData("BONN.BEUEL")]
        public void Get_function_filtered_by_organization_should_throw_when_passing_invalid_sort_properties(
            string orderedBy)
        {
            var startingPoint =
                new ArangoDbEnumerable<FunctionObjectEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var options = new QueryObject
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
                            Values = new[] { "Z23" }
                        }
                    }
                },
                OrderedBy = orderedBy
            };

            Assert.Throws<ValidationException>(
                () =>
                    startingPoint
                        .UsingOptions(options)
                        .ToQuery(CollectionScope.Query));
        }
    }
}
