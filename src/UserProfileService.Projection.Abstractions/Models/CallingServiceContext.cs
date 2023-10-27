using System.Runtime.CompilerServices;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Defines the context of a service or event handler that calls a store (i.e. database) and contains further
///     information about the caller.
/// </summary>
public record CallingServiceContext
{
    /// <summary>
    ///     Returns a boolean value indicating whether the context data has been set or not. Is <c>true</c>, if no context
    ///     information has been set.
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty => string.IsNullOrEmpty(ServiceName) && string.IsNullOrEmpty(UsedMethod);

    /// <summary>
    ///     The name of the service or event handler that is using the current store.
    /// </summary>
    public string ServiceName { get; init; }

    /// <summary>
    ///     The name of the method that is using the underlying store inside this context.
    /// </summary>
    public string UsedMethod { get; init; }

    /// <summary>
    ///     Initializes a new instance of <see cref="CallingServiceContext" /> without any parameter.
    /// </summary>
    public CallingServiceContext()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="CallingServiceContext" /> with a specified service name.
    /// </summary>
    /// <param name="serviceName">The name of the service instance of event handler that is using the store.</param>
    public CallingServiceContext(string serviceName)
    {
        ServiceName = serviceName;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="CallingServiceContext" /> with specified service and method name.
    /// </summary>
    /// <param name="serviceName">The name of the service instance of event handler that is using the store.</param>
    /// <param name="usedMethod">The name of the method that is using the store.</param>
    public CallingServiceContext(string serviceName, string usedMethod)
    {
        ServiceName = serviceName;
        UsedMethod = usedMethod;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="CallingServiceContext" /> with the name of <typeparamref name="TService" /> as
    ///     <see cref="ServiceName" /> and <paramref name="caller" /> as <see cref="UsedMethod" /> name.
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="caller">The calling method or property.</param>
    /// <returns>A new instance of <see cref="CallingServiceContext" />.</returns>
    public static CallingServiceContext CreateNewOf<TService>(
        [CallerMemberName] string caller = null)
    {
        return new CallingServiceContext(
            typeof(TService).Name,
            caller);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(ServiceName))
        {
            return $"{UsedMethod}()";
        }

        if (string.IsNullOrEmpty(UsedMethod))
        {
            return ServiceName;
        }

        return $"{ServiceName}.{UsedMethod}()";
    }
}
