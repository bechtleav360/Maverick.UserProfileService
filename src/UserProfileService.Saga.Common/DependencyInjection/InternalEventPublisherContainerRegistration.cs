using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.DependencyInjection;

namespace UserProfileService.Saga.Common.DependencyInjection;

internal class InternalEventPublisherContainerRegistration : IEventPublisherContainerRegistration
{
    private readonly IServiceCollection _services;

    public InternalEventPublisherContainerRegistration(IServiceCollection services)
    {
        _services = services;
    }

    public IEventPublisherContainerRegistration AddEventPublisher<TEventPublisher>(
        Func<IServiceProvider, IEventPublisherTypeResolver>? resolver = null)
        where TEventPublisher : class, IEventPublisher
    {
        _services.AddScoped<IEventPublisher, TEventPublisher>();
        _services.AddScoped<TEventPublisher>();

        if (resolver != null)
        {
            _services.AddTransient(resolver);
        }

        return this;
    }

    public IEventPublisherContainerRegistration UseEventProcessorSetup<TSetup>()
        where TSetup : EventProcessingSetup
    {
        ServiceDescriptor registration = ServiceDescriptor.Singleton<EventProcessingSetup>(p => ActivatorUtilities.CreateInstance<TSetup>(p));

        _services.Replace(registration);

        return this;
    }
}
