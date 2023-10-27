using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.Tests.V2.Helpers;
using Xunit;

namespace UserProfileService.Arango.Tests.V2
{
    public class ModelBuilderTests
    {
        [Theory]
        [InlineData(WellKnownDatabaseKeys.CollectionPrefixUserProfileService)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        [InlineData("myDefault")]
        public void Set_up_model_collection_names(string collectionPrefix)
        {
            IModelBuilder modelBuilder = ModelBuilder.NewOne;

            // users
            modelBuilder
                .Entity<User>()
                .Collection("Profile")
                .QueryCollection("QueryForProfile");

            modelBuilder
                .Entity<User>()
                .HasKeyIdentifier(u => u.Id);

            // groups
            modelBuilder
                .Entity<Group>()
                .HasKeyIdentifier(g => g.Id);

            modelBuilder
                .Entity<Group>()
                .Collection("Profile")
                .QueryCollection("GroupsQuery");

            ModelBuilderOptions opt = modelBuilder.BuildOptions(collectionPrefix);
            Assert.Equal("Profile".GetCollectionNameInTest(collectionPrefix), opt.GetCollectionName(typeof(Group)));
            Assert.Equal("Profile".GetCollectionNameInTest(collectionPrefix), opt.GetCollectionName<User>());

            Assert.Equal(
                "GroupsQuery".GetCollectionNameInTest(collectionPrefix),
                opt.GetQueryCollectionName(typeof(Group)));

            Assert.Equal(
                "QueryForProfile".GetCollectionNameInTest(collectionPrefix),
                opt.GetQueryCollectionName<User>());
        }

        [Theory]
        [InlineData(
            WellKnownDatabaseKeys.CollectionPrefixUserProfileService,
            WellKnownDatabaseKeys.CollectionPrefixUserProfileService)]
        [InlineData("", "")]
        [InlineData(null, "a")]
        [InlineData("myDefault", "myQueryDefault")]
        public void Set_up_model_collection_names_with_different_prefixes(string collectionPrefix, string queryPrefix)
        {
            IModelBuilder modelBuilder = ModelBuilder.NewOne;

            // users
            modelBuilder
                .Entity<User>()
                .Collection("Profile")
                .QueryCollection("QueryForProfile");

            modelBuilder
                .Entity<User>()
                .HasKeyIdentifier(u => u.Id);

            ModelBuilderOptions opt = modelBuilder.BuildOptions(collectionPrefix, queryPrefix);
            Assert.Equal("Profile".GetCollectionNameInTest(collectionPrefix), opt.GetCollectionName<User>());

            Assert.Equal("QueryForProfile".GetCollectionNameInTest(queryPrefix), opt.GetQueryCollectionName<User>());
        }

        [Fact]
        public void Set_up_model_key_identifiers()
        {
            IModelBuilder modelBuilder = ModelBuilder.NewOne;

            modelBuilder.Entity<RoleView>()
                .HasKeyIdentifier("roleId");

            modelBuilder.Entity<FunctionView>()
                .HasKeyIdentifier(f => f.Name);

            ModelBuilderOptions opt =
                modelBuilder.BuildOptions(WellKnownDatabaseKeys.CollectionPrefixUserProfileService);

            Assert.Equal("roleId", opt.GetKeyIdentifier(typeof(RoleView)));
            Assert.Equal("Name", opt.GetKeyIdentifier<FunctionView>());
        }

        [Fact]
        public void Set_up_model_relations()
        {
            IModelBuilder modelBuilder = ModelBuilder.NewOne;

            modelBuilder.Entity<User>().AddParentRelation<Group>(rb => rb.WithCollectionName("GroupRelations"));
            modelBuilder.Entity<Group>().AddParentRelation<Group>(rb => rb.WithCollectionName("GroupRelations"));
            modelBuilder.Entity<FunctionView>().AddChildRelation<Group>();
            modelBuilder.Entity<FunctionView>().AddChildRelation<User>();

            ModelBuilderOptions opt =
                modelBuilder.BuildOptions(WellKnownDatabaseKeys.CollectionPrefixUserProfileService);

            string[] unused = opt.GetRelatedInboundEdgeCollections<FunctionView>();
        }
    }
}
