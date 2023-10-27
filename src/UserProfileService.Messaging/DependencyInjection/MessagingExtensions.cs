using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Messaging.Exceptions;
using UserProfileService.Messaging.Formatters;
using UserProfileService.Messaging.Serialization;

namespace UserProfileService.Messaging.DependencyInjection;

/// <summary>
///     Extensions to add and configure Maverick-Style messaging for an Application.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    ///     Sets the global MassTransit configuration up, and delegates transport-specific configuration.
    /// </summary>
    /// <param name="configurator">mass-transit configurator to setup</param>
    /// <param name="metadata">metadata for the current application, used to configure messaging</param>
    /// <param name="messagingConfig">root-section for all messaging related configuration</param>
    /// <param name="assemblies">assemblies from which all consumers shall be registered</param>
    /// <param name="busConfigurator">
    ///     optional customization for the underlying messaging-platform after default configuration
    /// </param>
    /// <param name="rabbitMqCustomizer">
    ///     optional customization for the rabbitmq-bus after default configuration.
    /// </param>
    /// <param name="inMemoryCustomizer">
    ///     optional customization for the in-memory bus after default configuration.
    /// </param>
    /// <param name="nameFormatter">custom name formatter for endpoints and entities</param>
    /// <exception cref="InvalidMessagingConfigurationException"></exception>
    /// <exception cref="InvalidMessagingPlatformException"></exception>
    private static void ConfigureMassTransit(
        IBusRegistrationConfigurator configurator,
        ServiceMessagingMetadata metadata,
        IConfiguration messagingConfig,
        Assembly[] assemblies,
        Action<IBusRegistrationConfigurator>? busConfigurator,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? rabbitMqCustomizer,
        Action<IBusRegistrationContext, IInMemoryBusFactoryConfigurator>? inMemoryCustomizer,
        CustomEntityNameFormatter nameFormatter)
    {
        string messagingType =
            messagingConfig.GetSection("Messaging:Type")
                .Value
                ?.ToLowerInvariant()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(messagingType))
        {
            throw new InvalidMessagingConfigurationException("no 'Messaging:Type' set");
        }

        configurator.SetEndpointNameFormatter(nameFormatter);

        if (messagingType.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            switch (messagingType)
            {
                case "rabbitmq":
                    configurator.UsingRabbitMq(
                        (rmqContext, rmqConfigurator) => ConfigureRabbitMq(
                            rmqContext,
                            rmqConfigurator,
                            metadata,
                            messagingConfig,
                            rabbitMqCustomizer,
                            nameFormatter));

                    break;

                case "inmemory":
                case "memory":
                    configurator.UsingInMemory(
                        (context, memoryConfigurator) => ConfigureMemory(
                            context,
                            memoryConfigurator,
                            metadata,
                            inMemoryCustomizer,
                            nameFormatter));

                    break;

                default:
                    throw new InvalidMessagingPlatformException(messagingType);
            }
        }
        catch (Exception e)
        {
            throw new InvalidMessagingConfigurationException("error while configuring transport", e);
        }

        if (assemblies.Any())
        {
            configurator.AddConsumers(assemblies);
            configurator.AddSagas(assemblies);
            configurator.AddSagaStateMachines(assemblies);
            configurator.SetInMemorySagaRepositoryProvider();
        }

        busConfigurator?.Invoke(configurator);
    }

    /// <summary>
    ///     Sets customizations used regardless of transport.
    /// </summary>
    /// <param name="configurator">configuration-handle through which the transport can be configured</param>
    /// <param name="metadata">metadata for the current application, used to configure messaging</param>
    /// <param name="nameFormatter">custom name formatter for endpoints and entities</param>
    private static void UseCommonConfiguration(
        IBusFactoryConfigurator configurator,
        ServiceMessagingMetadata metadata,
        CustomEntityNameFormatter nameFormatter)
    {
        configurator.MessageTopology.SetEntityNameFormatter(nameFormatter);

        var messageSerializer = new CloudEventMessageSerializerFactory(metadata.Source, nameFormatter);

        configurator.AddDeserializer(messageSerializer, true);
        configurator.AddSerializer(messageSerializer);
    }

    /// <summary>
    ///     Sets the in-memory transport up with our customizations.
    /// </summary>
    /// <param name="context">context in which this transport is being configured</param>
    /// <param name="configurator">configuration-handle through which the transport can be configured</param>
    /// <param name="metadata">metadata for the current application, used to configure messaging</param>
    /// <param name="inMemoryCustomizer">
    ///     optional customization for the in-memory bus after default configuration.
    /// </param>
    /// <param name="nameFormatter">custom name formatter for endpoints and entities</param>
    private static void ConfigureMemory(
        IBusRegistrationContext context,
        IInMemoryBusFactoryConfigurator configurator,
        ServiceMessagingMetadata metadata,
        Action<IBusRegistrationContext, IInMemoryBusFactoryConfigurator>? inMemoryCustomizer,
        CustomEntityNameFormatter nameFormatter)
    {
        UseCommonConfiguration(configurator, metadata, nameFormatter);

        inMemoryCustomizer?.Invoke(context, configurator);

        configurator.ConfigureEndpoints(context);
    }

    /// <summary>
    ///     Sets the rabbitmq transport up with our customizations.
    /// </summary>
    /// <param name="context">context in which this transport is being configured</param>
    /// <param name="configurator">configuration-handle through which the transport can be configured</param>
    /// <param name="metadata">metadata for the current application, used to configure messaging</param>
    /// <param name="messagingConfig">root-section for all messaging related configuration</param>
    /// <param name="rabbitMqCustomizer">
    ///     optional customization for the rabbitmq-bus after default configuration.
    /// </param>
    /// <param name="nameFormatter">custom name formatter for endpoints and entities</param>
    private static void ConfigureRabbitMq(
        IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator configurator,
        ServiceMessagingMetadata metadata,
        IConfiguration messagingConfig,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? rabbitMqCustomizer,
        CustomEntityNameFormatter nameFormatter)
    {
        UseCommonConfiguration(configurator, metadata, nameFormatter);

        IConfiguration config = messagingConfig.GetSection("Messaging:RabbitMq");
        var vhost = config.GetValue<string?>("VirtualHost", null);
        var port = config.GetValue<string?>("Port", null);
        var user = config.GetValue<string?>("User", null);
        var pass = config.GetValue<string?>("Password", null);

        string host = config.GetValue<string?>("Host", null)
            ?? throw new InvalidMessagingConfigurationException("RabbitMq-Host missing from Messaging:RabbitMq:Host");

        void ConfigureRabbitMqCredentials(IRabbitMqHostConfigurator rmq)
        {
            if (user is not null)
            {
                rmq.Username(user);
            }

            if (pass is not null)
            {
                rmq.Password(pass);
            }
        }

        if (!string.IsNullOrWhiteSpace(vhost)
            && !string.IsNullOrWhiteSpace(port)
            && ushort.Parse(port) > 0)
        {
            configurator.Host(host, ushort.Parse(port), vhost, ConfigureRabbitMqCredentials);
        }
        else if (!string.IsNullOrWhiteSpace(vhost))
        {
            configurator.Host(host, vhost, ConfigureRabbitMqCredentials);
        }
        else
        {
            configurator.Host(host, ConfigureRabbitMqCredentials);
        }

        rabbitMqCustomizer?.Invoke(context, configurator);

        configurator.ConfigureEndpoints(context);
    }

    /// <summary>
    ///     Register all components required for Maverick-Style messaging in the given service-collection.
    /// </summary>
    /// <param name="services">service-collection in which all services will be registered</param>
    /// <param name="metadata">metadata for the current application, used to configure messaging</param>
    /// <param name="messagingConfig">root-section for all messaging related configuration</param>
    /// <param name="assemblies">
    ///     assemblies from which all consumers shall be registered. Defaults to entry-assembly if null.
    /// </param>
    /// <param name="busConfigurator">
    ///     optional customization for the underlying messaging-platform after default configuration
    /// </param>
    /// <param name="rabbitMqCustomizer">
    ///     optional customization for the rabbitmq-bus after default configuration.
    /// </param>
    /// <param name="inMemoryCustomizer">
    ///     optional customization for the in-memory bus after default configuration.
    /// </param>
    /// <returns>modified instance of <paramref name="services" /></returns>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        ServiceMessagingMetadata metadata,
        IConfiguration messagingConfig,
        IEnumerable<Assembly>? assemblies = null,
        Action<IBusRegistrationConfigurator>? busConfigurator = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? rabbitMqCustomizer = null,
        Action<IBusRegistrationContext, IInMemoryBusFactoryConfigurator>? inMemoryCustomizer = null)
    {
        try
        {
            services.AddMassTransit(
                configurator =>
                    ConfigureMassTransit(
                        configurator,
                        metadata,
                        messagingConfig,
                        assemblies?.ToArray() ?? new[] { Assembly.GetEntryAssembly()! },
                        busConfigurator,
                        rabbitMqCustomizer,
                        inMemoryCustomizer,
                        new CustomEntityNameFormatter(
                            new KebabCaseEndpointNameFormatter(false),
                            new MessageNameFormatterEntityNameFormatter(
                                new DefaultMessageNameFormatter("::", "--", ":", "-")),
                            metadata)));
        }
        catch (Exception e)
        {
            throw new MessagingRegistrationException("internal error while registering messaging components", e);
        }

        return services;
    }

    /// <summary>
    ///     Helper method to configure a consumer to create a queue with the properties
    ///     autodelete = true & durable = false
    /// </summary>
    /// <param name="configurator">the <see cref="IRabbitMqBusFactoryConfigurator" /> to configure</param>
    /// <param name="registrationContext">Registration context which contains the consumers and sagas</param>
    /// <param name="configureConsumer">Optional: custom configuration for the consumer</param>
    /// <typeparam name="TConsumer">The type of the consumer to configure</typeparam>
    public static void AsTemporaryConsumer<TConsumer>(
        this IRabbitMqBusFactoryConfigurator configurator,
        IRegistrationContext registrationContext,
        Action<IConsumerConfigurator<TConsumer>>? configureConsumer = null)
        where TConsumer : class, IConsumer
    {
        configurator.ReceiveEndpoint(
            new TemporaryEndpointDefinition(),
            endpoint => endpoint.ConfigureConsumer(registrationContext, configureConsumer));
    }
}
