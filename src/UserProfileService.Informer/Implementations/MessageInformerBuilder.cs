using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Informer.Abstraction;

namespace UserProfileService.Informer.Implementations;

/// <summary>
///     An implementation of the <see cref="IMessageInformerBuilder" />.
/// </summary>
public class MessageInformerBuilder : IMessageInformerBuilder
{
    /// <inheritdoc />
    public Dictionary<Type, List<Func<IServiceProvider, IProcessNotifierExecutor>>>?
        NotificationDictionary { set; get; } =
        new Dictionary<Type, List<Func<IServiceProvider, IProcessNotifierExecutor>>>();

    /// <inheritdoc />
    public IServiceCollection ServiceCollection { get; set; }

    /// <summary>
    ///     Creates an object of type <see cref="MessageInformerBuilder" />.
    /// </summary>
    /// <param name="serviceCollection"> The service collection is used to register service that are needed by runtime.</param>
    public MessageInformerBuilder(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }
}
