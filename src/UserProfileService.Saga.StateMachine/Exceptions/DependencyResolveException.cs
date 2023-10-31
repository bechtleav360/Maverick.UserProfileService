using System.Reflection;

namespace UserProfileService.StateMachine.Exceptions;

/// <summary>
///     Failed to resolve the interface of service type
/// </summary>
public sealed class DependencyResolveException : Exception
{
    /// <summary>
    ///     Typed we tried to resolve
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Property will be needed.
    public Type ServiceType { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NLogDependencyResolveException" /> class.
    /// </summary>
    public DependencyResolveException(string message, Type serviceType) : base(CreateFullMessage(serviceType, message))
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
    }

    private static string CreateFullMessage(MemberInfo typeToResolve, string message)
    {
        return $"Cannot resolve the type: '{typeToResolve.Name}'. {message}".Trim();
    }
}
