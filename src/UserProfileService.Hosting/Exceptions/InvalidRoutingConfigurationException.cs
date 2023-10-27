using System.Runtime.Serialization;

namespace UserProfileService.Hosting.Exceptions;

/// <summary>
///     Thrown when invalid settings are configured for use in
///     <see cref="ApplicationBuilderExtensions.UseReverseProxyPathBases" />.
/// </summary>
[Serializable]
public sealed class InvalidRoutingConfigurationException : ApplicationException
{
    /// <inheritdoc />
    private InvalidRoutingConfigurationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <inheritdoc />
    public InvalidRoutingConfigurationException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public InvalidRoutingConfigurationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
