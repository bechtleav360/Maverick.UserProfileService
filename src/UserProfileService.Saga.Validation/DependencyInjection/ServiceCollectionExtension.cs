using System;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Events.Payloads;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Validation.Abstractions.Configuration;
using FunctionCreatedPayloadValidatorV2 =
    UserProfileService.Saga.Validation.Fluent.V2.FunctionCreatedPayloadValidator;
using FunctionCreatedPayloadValidatorV3 =
    UserProfileService.Saga.Validation.Fluent.V3.FunctionCreatedPayloadValidator;

namespace UserProfileService.Saga.Validation.DependencyInjection;

/// <summary>
///     Contains methods to extend <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    ///     Adds the validation for the saga messages.
    /// </summary>
    /// <typeparam name="TValidationReadService">Type of service to use for <see cref="IValidationReadService" />.</typeparam>
    /// <typeparam name="TVolatileRepoValidationService"></typeparam>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <param name="configuration">Section of validation configuration (see <see cref="ValidationConfiguration" />).</param>
    /// <returns>
    ///     <see cref="IServiceCollection" />
    /// </returns>
    public static IServiceCollection AddSagaValidation<TValidationReadService, TVolatileRepoValidationService>(
        this IServiceCollection services,
        IConfigurationSection configuration)
        where TValidationReadService : class, IValidationReadService
        where TVolatileRepoValidationService : class, IVolatileRepoValidationService
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services.Configure<ValidationConfiguration>(configuration);

        services.RegisterCustomSagaMessageValidation(_ => {});
        
        services.TryAddTransient<ICustomValidationServiceFactory, DefaultCustomValidationServiceFactory>();

        services
            .AddTransient<IRepoValidationService, RepoValidationService>()
            .AddTransient<IVolatileRepoValidationService, TVolatileRepoValidationService>()
            .AddTransient<IValidationService, ValidationService>()
            .AddTransient<IValidationReadService, TValidationReadService>()
            .AddPayloadValidation();

        return services;
    }

    /// <summary>
    ///     Adds the validation for the saga messages.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <param name="configuration">Section of validation configuration (see <see cref="ValidationConfiguration" />).</param>
    /// <param name="implementationFactory">Factory to use for implementation of <see cref="IValidationReadService" />.</param>
    /// <returns><see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddSagaValidation(
        this IServiceCollection services,
        IConfigurationSection configuration,
        Func<IServiceProvider, IValidationReadService> implementationFactory)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services.RegisterCustomSagaMessageValidation(_ => {});

        services.Configure<ValidationConfiguration>(configuration);

        services.TryAddTransient<ICustomValidationServiceFactory, DefaultCustomValidationServiceFactory>();

        services
            .AddTransient<IRepoValidationService, RepoValidationService>()
            .AddTransient<IValidationService, ValidationService>()
            .AddTransient(implementationFactory)
            .AddPayloadValidation();

        return services;
    }

    /// <summary>
    ///     Adds the validation for the payloads as <see cref="IPayloadValidationService" />.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <returns><see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddPayloadValidation(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services
            .AddValidatorsFromAssemblyContaining<
                FunctionCreatedPayloadValidatorV2>() // All validators for v2 models.
            .AddValidatorsFromAssemblyContaining<
                FunctionCreatedPayloadValidatorV3>(); // All validators for v3 models.

        services
            .AddTransient<IPayloadValidationService, PayloadValidationService>();

        return services;
    }

    /// <summary>
    ///     Registers custom validation services with the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to which custom validation services will be registered.</param>
    /// <param name="modifier">
    ///     An action that allows customization of the <see cref="CustomValidationServiceFactoryOptions" /> before
    ///     registration.
    ///     This action can be used to configure mappings between message types and custom validation service types.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection" /> with the custom validation services registered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <c>null</c>.</exception>
    public static IServiceCollection RegisterCustomSagaMessageValidation(
        this IServiceCollection services,
        Action<CustomValidationServiceFactoryOptions> modifier)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (modifier == null)
        {
            throw new ArgumentNullException(nameof(modifier));
        }

        var options = new CustomValidationServiceFactoryOptions();
        modifier.Invoke(options);

        services.TryAddTransient(_ => options);

        return services;
    }

    /// <summary>
    ///     Adds a mapping between a message type and a custom validation service type to the
    ///     <see cref="CustomValidationServiceFactoryOptions" />.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be validated. It must be a class.</typeparam>
    /// <typeparam name="TValidator">
    ///     The type of the custom validation service implementing
    ///     <see cref="ICustomValidationService" />.
    /// </typeparam>
    /// <param name="source">
    ///     The <see cref="CustomValidationServiceFactoryOptions" /> instance to which the mapping will be
    ///     added.
    /// </param>
    /// <returns>The updated <see cref="CustomValidationServiceFactoryOptions" /> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="source" /> has a <c>null</c>
    ///     <see cref="CustomValidationServiceFactoryOptions.MessageTypeToValidationServiceMap" />.
    /// </exception>
    public static CustomValidationServiceFactoryOptions AddCustomValidator<TMessage, TValidator>(
        this CustomValidationServiceFactoryOptions source)
        where TMessage : IPayload
        where TValidator : ICustomValidationService
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (source.MessageTypeToValidationServiceMap == null)
        {
            throw new ArgumentException(
                $"Options.{nameof(CustomValidationServiceFactoryOptions.MessageTypeToValidationServiceMap)} must not be null",
                nameof(source));
        }

        source.MessageTypeToValidationServiceMap.TryAdd(
            typeof(TMessage),
            typeof(TValidator));

        return source;
    }
}
