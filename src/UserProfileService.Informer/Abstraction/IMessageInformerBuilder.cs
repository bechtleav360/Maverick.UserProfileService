using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Informer.Abstraction;

/// <summary>
///     The notifier builder enables to register the components for the
///     <see cref="IMessageInformer" /> fluently.
/// </summary>
public interface IMessageInformerBuilder
{
    /// <summary>
    ///     The dictionary that is created when adding new handler for notification
    /// </summary>
    public Dictionary<Type, List<Func<IServiceProvider, IProcessNotifierExecutor>>> NotificationDictionary { set; get; }

    /// <summary>
    ///     The service collection is used to register service that are needed by runtime.
    /// </summary>
    public IServiceCollection ServiceCollection { get; set; }
}
