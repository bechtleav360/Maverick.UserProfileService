using System;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.Configure<ValidationConfiguration>(configuration);

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
}
