using System.Collections;
using UserProfileService.Marten.EventStore.Options;
// ReSharper disable StringLiteralTypo

namespace UserProfileService.MartenEventStore.UnitTests.TestData;

public class MartenEventStoreOptionsTestData : IEnumerable<object[]>
{
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = "UserProfileServiceStream",
                ConnectionString = "Host=127.0.0.1;Username=root;Password=1;Database=test_db",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = "ups"
            },
            true
        };

        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = "UserProfileServiceStream",
                ConnectionString = "Host=127.0.0.1;Username=root;Password=1;Database=test_db",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = ""
            },
            false
        };

        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = null,
                ConnectionString = "Host=127.0.0.1;Username=root;Password=1;Database=test_db",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = "ups"
            },
            false
        };

        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = "UserProfileServiceStream",
                ConnectionString = "Host=localhost;Username=root;Password=1;Database=test_db",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = "ups"
            },
            true
        };

        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = null,
                ConnectionString = "Host=127.0.0.1;Username=root;Passwordo=1;Database=test_db",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = "ups"
            },
            false
        };

        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = null,
                ConnectionString = "Hosty=127.0.0.1;Username=root;Password=1;Database=test_db",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = "ups"
            },
            false
        };

        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = null,
                ConnectionString = "Host=127.0.0.;Username=root;Password=1;Database=test_db",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = "ups"
            },
            false
        };

        yield return new object[]
        {
            new MartenEventStoreOptions
            {
                SubscriptionName = null,
                ConnectionString = "Hosty=127.0.0.1;Username=root;Password=1;Database=",
                DatabaseSchema = "UserProfileServiceSchema",
                StreamNamePrefix = "ups"
            },
            false
        };
    }
}
