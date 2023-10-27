using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Informer.Abstraction;

namespace UserProfileService.Informer.Implementations;

/// <summary>
///     The message executes all notifies that are registered for an event type.
/// </summary>
public class MessageInformer : IMessageInformer
{
    private readonly ILogger<MessageInformer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, List<Func<IServiceProvider, IProcessNotifierExecutor>>?> _notificationHandlers;

    /// <summary>
    ///     Creates an object of type <see cref="MessageInformer" />.
    /// </summary>
    /// <param name="logger">The logger for logging purposes.</param>
    /// <param name="notificationHandlers">
    ///     The dictionary contains all notification handler that should be executed for a
    ///     specific event type.
    /// </param>
    /// <param name="serviceProvider">The service provider is used to create the notification handler.</param>
    public MessageInformer(
        ILogger<MessageInformer> logger,
        Dictionary<Type, List<Func<IServiceProvider, IProcessNotifierExecutor>>?> notificationHandlers,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _notificationHandlers = notificationHandlers;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task NotifyEventOccurredAsync(IUserProfileServiceEvent serviceEvent, INotifyContext context)
    {
        _logger.EnterMethod();

        bool processNotifierExecutorExists = _notificationHandlers.TryGetValue(
            serviceEvent.GetType(),
            out List<Func<IServiceProvider, IProcessNotifierExecutor>>? notificationNotifierExecutors);

        if (!processNotifierExecutorExists)
        {
            _logger.LogDebugMessage(
                "For the event type {eventType} were no handler to execution found.",
                LogHelpers.Arguments(serviceEvent.GetType()));

            return;
        }

        _logger.LogInfoMessage(
            "For the even type {eventType} has been found {countListHandler} handlers to execute.",
            LogHelpers.Arguments(serviceEvent.GetType(), notificationNotifierExecutors?.Count));

        if (notificationNotifierExecutors != null)
        {
            foreach (Func<IServiceProvider, IProcessNotifierExecutor> notificationExecutor in
                     notificationNotifierExecutors)
            {
                try
                {
                    if (notificationNotifierExecutors == null)
                    {
                        continue;
                    }

                    IProcessNotifierExecutor handler = notificationExecutor.Invoke(_serviceProvider);

                    _logger.LogInfoMessage(
                        "Trying to execute the notification handler of type {handlerType} for the '{serviceType}'",
                        LogHelpers.Arguments(handler.GetType(), serviceEvent.GetType()));

                    await handler.ExecuteNotificationMessageAsync(serviceEvent, context);

                    _logger.LogInfoMessage(
                        "For the event type '{eventType}' the notification handler of type {handlerType} was executed successfully.",
                        LogHelpers.Arguments(serviceEvent.GetType(), handler.GetType()));
                }
                catch (Exception ex)
                {
                    _logger.LogErrorMessage(
                        ex,
                        "An error occurred while tying to notify that the event type '{eventType}' appeared.",
                        LogHelpers.Arguments(serviceEvent.GetType()));

                    throw;
                }
            }
        }
        _logger.ExitMethod();
    }
}
