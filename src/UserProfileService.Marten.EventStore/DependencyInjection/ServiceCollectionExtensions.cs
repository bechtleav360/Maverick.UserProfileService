using System.Reflection;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Services;
using Marten.Services.Json;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Attributes;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Marten.EventStore.Implementations;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.Marten.EventStore.Validation;
using Weasel.Core;

namespace UserProfileService.Marten.EventStore.DependencyInjection;

/// <summary>
///     Contains some extensions method to register entities in the <see cref="IServiceCollection" />
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Returns all event types that derive from <see cref="IUserProfileServiceEvent" /> in the <see cref="AppDomain" />.
    /// </summary>
    /// <returns>Collection of all derived events.</returns>
    private static ICollection<(Type type, string suffix)> GetAllEventTypes(Type interfaceType)
    {
        return AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && interfaceType.IsAssignableFrom(p) && !p.IsInterface)
            .Select(type => (type, GetEventTypeName(type)))
            .ToList();
    }

    private static string GetEventTypeName(Type type)
    {
        long version;
        var unresolvedEventSuffix = string.Empty;

        if (type.TryGetCustomAttribute(out AggregateEventDetails? aggregateEventDetails))
        {
            version = aggregateEventDetails?.VersionInformation ?? 1;

            unresolvedEventSuffix = aggregateEventDetails is
            {
                IsResolved: true
            }
                ? unresolvedEventSuffix
                : "_unresolved";
        }
        else
        {
            version = type.GetCustomAttribute<EventVersionAttribute>()?.VersionInformation ?? 1;
        }

        return version > 2
            ? $"{EventMappingExtensions.GetEventTypeName(type)}_v{version}{unresolvedEventSuffix}"
            : $"{EventMappingExtensions.GetEventTypeName(type)}{unresolvedEventSuffix}";
    }

    private static bool TryGetCustomAttribute<TAttribute>(
        this Type type,
        out TAttribute? attribute)
        where TAttribute : Attribute
    {
        attribute = type.GetCustomAttribute<TAttribute>();

        return attribute != null;
    }

    /// <summary>
    ///     Registers implementations of <see cref="IEventStorageClient" /> that use
    ///     the EventStore functions of the Marten library.
    /// </summary>
    /// <param name="services">
    ///     The <see cref="IServiceCollection" /> to which the services are added.
    /// </param>
    /// <param name="configuration">The configuration to register marten db.</param>
    /// <param name="sectionName">
    ///     The section name in the configuration file
    /// </param>
    /// <param name="eventInterfaceType">
    ///     The type of the events that should be handled by the projections.
    /// </param>
    /// <param name="registerExtension">
    ///     An action that is executed to configure Marten.
    /// </param>
    /// <param name="converters">
    ///     An <see cref="IEnumerable{JsonConverter}" /> that should be used by marten for
    ///     event deserialization.
    /// </param>
    /// <returns>
    ///     The service collection after it has been modified.
    /// </returns>
    public static IServiceCollection AddMartenEventStore(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = Constants.EventStorage.MartenSectionName,
        Type? eventInterfaceType = null,
        Action<StoreOptions, IServiceProvider>? registerExtension = null,
        IList<JsonConverter>? converters = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services.Configure<MartenEventStoreOptions>(options => configuration.Bind(sectionName, options));

        // Validate marten options 
        services.AddSingleton<IValidateOptions<MartenEventStoreOptions>, MartenEventStoreConfigurationValidation>();

        services.AddMarten(
                provider =>
                {
                    var option = new StoreOptions();
                    using IServiceScope scoped = provider.CreateScope();

                    MartenEventStoreOptions? martenObject =
                        scoped.ServiceProvider.GetRequiredService<IOptionsSnapshot<MartenEventStoreOptions>>()
                            .Value;

                    if (!string.IsNullOrWhiteSpace(martenObject?.DatabaseSchema))
                    {
                        option.DatabaseSchemaName = martenObject.DatabaseSchema;
                    }

                    if (!string.IsNullOrWhiteSpace(martenObject?.ConnectionString))
                    {
                        option.Connection(martenObject.ConnectionString);
                    }

                    option.Events.StreamIdentity = StreamIdentity.AsString;

                    if (eventInterfaceType != null)
                    {
                        List<(Type type, string suffix)> allEventTypes = GetAllEventTypes(eventInterfaceType).ToList();

                        foreach ((Type? eventType, string? typeName) in allEventTypes)
                        {
                            option.Events.MapEventType(eventType, typeName);
                        }
                    }

                    var serializer = new JsonNetSerializer
                    {
                        EnumStorage = EnumStorage.AsString,
                        NonPublicMembersStorage = NonPublicMembersStorage.All
                    };

                    serializer.Customize(
                        o =>
                        {
                            o.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                            o.ConstructorHandling = ConstructorHandling.Default;
                            o.TypeNameHandling = TypeNameHandling.None;
                            o.ContractResolver = new JsonNetContractResolver();

                            if (converters == null || !converters.Any())
                            {
                                return;
                            }

                            foreach (JsonConverter jsonConverter in converters)
                            {
                                o.Converters.Add(jsonConverter);
                            }
                        });

                    option.Serializer(serializer);
                    registerExtension?.Invoke(option, provider);

                    return option;
                })
            .AddAsyncDaemon(DaemonMode.HotCold);

        services.AddSingleton<IEventStorageClient, MartenEventStore>();

        return services;
    }

    /// <summary>
    ///     Tries to add the stream name resolver to a service that is used
    ///     to resolve a stream name out of an <see cref="ObjectIdent" />
    /// </summary>
    /// <param name="services">The service collection to be modified.</param>
    /// <param name="serviceLifetime">The service life that is used to register the service.</param>
    /// <returns>The modified <see cref="IServiceCollection" />.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection TryAddStreamNameResolved(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAdd(
            new ServiceDescriptor(typeof(IStreamNameResolver), typeof(StreamNameResolver), serviceLifetime));

        return services;
    }
}
