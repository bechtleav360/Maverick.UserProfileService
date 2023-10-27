using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.DependencyInjection;
using UserProfileService.Common.V2.Exceptions;

namespace UserProfileService.Saga.Common.Implementations;

internal class DefaultEventPublisherFactory : IEventPublisherFactory
{
    private readonly ILogger<DefaultEventPublisherFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of <see cref="DefaultEventPublisherFactory" />.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The service provider containing <see cref="IEnumerable{T}" /> of
    ///     <see cref="IEventPublisher" /> registrations.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public DefaultEventPublisherFactory(
        IServiceProvider serviceProvider,
        ILogger<DefaultEventPublisherFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private static IEventPublisher GetDefaultEventPublisher(IServiceScope serviceScope)
    {
        var eventPublisherInstances =
            serviceScope.ServiceProvider.GetRequiredService<IEnumerable<IEventPublisher>>();

        IEventPublisher? defaultEventPublisher = eventPublisherInstances.FirstOrDefault(pub => pub.IsDefault);

        if (defaultEventPublisher == null)
        {
            throw new RegistrationMissingException(
                "No default event publisher has been registered - none could be found in IServiceProvider.");
        }

        return defaultEventPublisher;
    }

    /// <inheritdoc />
    public IEventPublisher GetPublisher(IUserProfileServiceEvent? eventData)
    {
        _logger.EnterMethod();

        if (eventData == null)
        {
            throw new ArgumentNullException(nameof(eventData));
        }

        using IServiceScope serviceScope = _serviceProvider.CreateScope();

        IEventPublisher defaultPublisher = GetDefaultEventPublisher(serviceScope);

        IList<IEventPublisherTypeResolver> services = serviceScope.ServiceProvider
            .GetServices<IEventPublisherTypeResolver>()
            .ToArray();

        List<IEventPublisher> appropriateCustomPublisher =
            services
                .Select(r => r.GetPublisher(eventData))
                .Where(p => p != null)
                .ToList();

        if (appropriateCustomPublisher.Count == 0)
        {
            _logger.LogInfoMessage(
                "No custom event publisher has been found (event type = {eventType}) - using default one",
                eventData.Type.AsArgumentList());

            return _logger.ExitMethod(defaultPublisher);
        }

        if (appropriateCustomPublisher.Count > 1)
        {
            _logger.LogWarnMessage(
                "More than one custom event publisher has been registered (event type = {eventType}) - using first one",
                eventData.Type.AsArgumentList());
        }

        return appropriateCustomPublisher.First();
    }
}
