﻿using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    [CollectionDefinition(nameof(DatabaseCollection), DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
