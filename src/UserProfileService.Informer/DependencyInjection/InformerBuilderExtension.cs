using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Informer.Abstraction;

namespace UserProfileService.Informer.DependencyInjection;

/// <summary>
///     The builder is used to create the handler dictionary that contains notification handler
///     that should be executed when a specific event type appears. The notification handler must
///     implement the interface <see cref="IProcessNotifierExecutor" />.
/// </summary>
public static class InformerBuilderExtension
{
    /// <summary>
    ///     Registers for the event <typeparamref name="TEvent" /> an notification handler <typeparamref name="THandler" />.
    /// </summary>
    /// <param name="informerBuilder">The builder that lets execute the notification handler fluently.</param>
    /// <typeparam name="TEvent">The event for which notification handler should be registered.</typeparam>
    /// <typeparam name="THandler">The notification handler that will be executed for an specific event type.</typeparam>
    /// <returns>
    ///     The <see cref="IMessageInformer" /> that can be used again to register dependencies for the
    ///     <see cref="IMessageInformer" />. 
    /// </returns>
    public static IMessageInformerBuilder AddNotificationHandler<TEvent, THandler>(
        this IMessageInformerBuilder informerBuilder)
        where TEvent : class, IUserProfileServiceEvent
        where THandler : class, IProcessNotifierExecutor
    {
        bool functionListExists = informerBuilder.NotificationDictionary.TryGetValue(
            typeof(TEvent),
            out List<Func<IServiceProvider, IProcessNotifierExecutor>>? functionsList);

        if (functionListExists && functionsList != null)
        {
            functionsList.Add(p => (IProcessNotifierExecutor)ActivatorUtilities.CreateInstance(p, typeof(THandler), p));
        }

        else
        {
            informerBuilder.NotificationDictionary.Add(
                typeof(TEvent),
                new List<Func<IServiceProvider, IProcessNotifierExecutor>>
                {
                    p => (IProcessNotifierExecutor)ActivatorUtilities.CreateInstance(p, typeof(THandler), p)
                });
        }

        return informerBuilder;
    }

    /// <summary>
    ///     Is used to to build all notification informer.
    /// </summary>
    /// <param name="informerBuilder">The builder that is used to register the services and notification builder.</param>
    public static void BuildMessageInformer(this IMessageInformerBuilder informerBuilder)
    {
        informerBuilder.ServiceCollection.TryAddScoped(p => informerBuilder.NotificationDictionary);
    }
}
