using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Maverick.Client.ArangoDb.ExternalLibraries.DependencyInjection;

// inspired by:
// https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/main/Microsoft.Toolkit.Mvvm/DependencyInjection/Ioc.cs
internal sealed class Ioc
{
    /// <summary>
    ///     The <see cref="IServiceProvider" /> instance to use, if initialized.
    /// </summary>
    private volatile IServiceProvider _serviceProvider;

    /// <summary>
    ///     Gets the default <see cref="Ioc" /> instance.
    /// </summary>
    public static Ioc Default { get; } = new Ioc();

    /// <summary>
    ///     Tries to resolve an instance of a specified service type.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve.</typeparam>
    /// <returns>An instance of the specified service, or <see langword="null" />.</returns>
    /// <exception cref="InvalidOperationException">Throw if the current <see cref="Ioc" /> instance has not been initialized.</exception>
    public T GetService<T>()
        where T : class
    {
        IServiceProvider provider = _serviceProvider;

        if (provider is null)
        {
            return default;
        }

        return (T)provider!.GetService(typeof(T));
    }

    /// <summary>
    ///     Initializes the shared <see cref="IServiceProvider" /> instance.
    /// </summary>
    public void ConfigureServices(Action<IServiceCollection> configuration)
    {
        if (_serviceProvider != null)
        {
            return;
        }

        var services = new ServiceCollection();

        configuration.Invoke(services);

        Interlocked.CompareExchange(
            ref _serviceProvider,
            services.BuildServiceProvider(),
            null);
    }
}
