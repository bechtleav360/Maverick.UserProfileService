using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.Tests.V2.TestModels;

namespace UserProfileService.Arango.Tests.V2.Helpers
{
    internal class ModelBuilderHelper
    {
        internal static ModelBuilderOptions Get()
        {
            IModelBuilder modelBuilder = ModelBuilder.NewOne;

            modelBuilder.Entity<TestEntity>()
                .HasKeyIdentifier(u => u.Id)
                .HasTypeIdentification(u => u.Type, "Test")
                .Collection("entity")
                .QueryCollection("entityQuery");

            return modelBuilder.BuildOptions("dbo");
        }
    }
}
