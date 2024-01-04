using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Informer.Implementations;

namespace UserProfileService.Informer.DependencyInjection;

/// <summary>
///     The service collection registers all dependencies that are needed to use
///     the <see cref="IMessageInformer" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     This is a none message informer registration where no notifier are registered.
    ///     The interfaces are still used in a base class. That is why this registration exists.
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" /> where the <see cref="IMessageInformer" />is
    ///     registered.
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> that is used to register services.</returns>
    public static IServiceCollection AddNoneMessageInformer(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddScoped<IMessageInformer, NoneMessageInformer>();
        serviceCollection.TryAddScoped<INotifyContext, DefaultNotifyContext>();
        var dictionaryHandler = new Dictionary<Type, List<Func<IServiceProvider, IProcessNotifierExecutor>>>();

        // an empty dictionary will be registered, because there are no notifier handler registered.
        serviceCollection.TryAddScoped(_ => dictionaryHandler);

        return serviceCollection;
    }

    /// <summary>
    ///     Registers an specific message informer. It is possible to register for an event type
    ///     several notification handler that are triggered when the event type occurred.
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" /> where the <see cref="IMessageInformer" />is
    ///     registered.
    /// </param>
    /// <param name="handlerRegistration">
    ///     The handler registration is used to register for an event type
    ///     notification handler. The notification handler must implement the interface <see cref="IProcessNotifierExecutor" />.
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> that is used to register services.</returns>
    public static IServiceCollection AddMessageInformer(
        this IServiceCollection serviceCollection,
        Action<IMessageInformerBuilder> handlerRegistration)
    {
        serviceCollection.TryAddScoped<IMessageInformer, MessageInformer>();
        serviceCollection.TryAddScoped<INotifyContext, DefaultNotifyContext>();

        var notifierBuilder = new MessageInformerBuilder(serviceCollection);
        handlerRegistration.Invoke(notifierBuilder);

        return serviceCollection;
    }
}
