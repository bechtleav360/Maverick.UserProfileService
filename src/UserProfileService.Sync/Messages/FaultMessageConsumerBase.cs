using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Sync.Messages
{
    /// <summary>
    /// Represents the base class for fault message consumers.
    /// </summary>
    public class FaultMessageConsumerBase
    {
        /// <summary>
        /// The logging instance <see cref="ILogger"/>
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FaultMessageConsumerBase"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for error logging.</param>
        /// <remarks>
        /// This constructor initializes the logger property with the provided logger instance.
        /// </remarks>
        public FaultMessageConsumerBase(ILogger logger)
        {
            Logger = logger;
        }


        /// <summary>
        /// Consumes a fault event of the specified type and logs error details.
        /// </summary>
        /// <typeparam name="TMessage">The type of the fault event being consumed.</typeparam>
        /// <param name="@event">The fault event instance.</param>
        /// <remarks>
        /// This method is responsible for consuming a fault event of the specified type and logging relevant error details,
        /// including the event type, timestamp, exceptions, and process name.
        /// </remarks>
        public async Task Consume<TMessage>(Fault<TMessage> @event)
        {
            await Task.Run(() => Logger.LogErrorMessage(
                null,
                "Error happened by consuming event of type: {type}, Timestamp: {time}, Exception(s): {exception}, ProcessName:{name}",
                LogHelpers.Arguments(
                    typeof(TMessage),
                    @event.Timestamp.ToString("MM/dd/yyyy HH:mm:ss"),
                    FormatExceptionAsString(@event.Exceptions),
                    @event.Host.ProcessName ?? string.Empty)));

        }


        private string FormatExceptionAsString(ExceptionInfo exception)
        {
            return exception == null
                ? string.Empty
                : $"StackTrace: {exception?.StackTrace} Source: {exception.Source} Message: {exception.Message} ExceptionType: {exception.ExceptionType}, InnerException: {FormatExceptionAsString(exception.InnerException)} ";
        }

        private string FormatExceptionAsString(ExceptionInfo[] exceptions)
        {
            if (exceptions == null || !exceptions.Any())
            {
                return string.Empty;
            }
            
            return string.Join(',', exceptions.Select(FormatExceptionAsString));
        }
    }
}
