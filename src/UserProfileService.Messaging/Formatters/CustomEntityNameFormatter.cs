using System.Reflection;
using System.Text.RegularExpressions;
using MassTransit;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Messaging.Formatters;

/// <summary>
///     Implementation of <see cref="IEndpointNameFormatter" />, that modifies the output of another implementation.
/// </summary>
public class CustomEntityNameFormatter : IEndpointNameFormatter, IEntityNameFormatter
{
    private static readonly Regex _versionPattern = new Regex("[vV]?[\\d\\._]+$", RegexOptions.Compiled);
    private readonly IEndpointNameFormatter _endpointFormatter;
    private readonly IEntityNameFormatter _entityFormatter;
    private readonly ServiceMessagingMetadata _serviceMetadata;

    /// <inheritdoc />
    public string Separator => _endpointFormatter.Separator;

    /// <summary>
    ///     Instance of the <see cref="CustomEntityNameFormatter" /> set when called .AddMessaging
    /// </summary>
    public static IEndpointNameFormatter? Instance;

    /// <summary>
    ///     Create a new instance of <see cref="CustomEntityNameFormatter" /> using the given
    ///     <see cref="IEndpointNameFormatter" />
    /// </summary>
    /// <param name="endpointFormatter">provides the base names that will be modified</param>
    /// <param name="entityFormatter"></param>
    /// <param name="metadata">messaging-metadata for the current app</param>
    /// <exception cref="ArgumentException">thrown when <paramref name="metadata" /> contains invalid data</exception>
    public CustomEntityNameFormatter(
        IEndpointNameFormatter endpointFormatter,
        IEntityNameFormatter entityFormatter,
        ServiceMessagingMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.ServiceName))
        {
            throw new ArgumentException(
                "metadata.ServiceName must not be empty",
                nameof(metadata));
        }

        _endpointFormatter = endpointFormatter;
        _entityFormatter = entityFormatter;
        _serviceMetadata = metadata;
    }

    private string GeneratedOrCustomNameFor<T>(string generatedName)
    {
        Type entityType = typeof(T);

        MessageAttribute? messageAttribute =
            entityType.GetCustomAttributes<MessageAttribute>()
                .FirstOrDefault();

        ConsumerAttribute? consumerAttribute =
            entityType.GetCustomAttributes<ConsumerAttribute>()
                .FirstOrDefault();

        // use the names from message- or consumer-attributes if they're set,
        // or use the one we're given.
        return !string.IsNullOrWhiteSpace(messageAttribute?.Name)
            ? messageAttribute.Name
            : !string.IsNullOrWhiteSpace(consumerAttribute?.Name)
                ? consumerAttribute.Name
                : generatedName;
    }

    private string GetCustomPrefixFor<T>()
    {
        Type entityType = typeof(T);

        MessageAttribute? messageAttribute =
            entityType.GetCustomAttributes<MessageAttribute>()
                .FirstOrDefault();

        // for components that don't have a msg-attribute we use the default service-group.
        // if group is explicitly null, we use an empty group.
        // otherwise we use either msg-service-group or default-service-group.
        string serviceGroup =
            messageAttribute is null
                ? _serviceMetadata.ServiceGroup
                : messageAttribute.ServiceGroup switch
                {
                    null => string.Empty,
                    "" => _serviceMetadata.ServiceGroup,
                    _ => messageAttribute.ServiceGroup ?? string.Empty
                };

        // attempt to use ServiceName of message-attribute and fall back to service-default.
        string serviceName = !string.IsNullOrWhiteSpace(messageAttribute?.ServiceName)
            ? messageAttribute.ServiceName
            : _serviceMetadata.ServiceName;

        string prefix = GetPrefix(serviceGroup, serviceName) + "_";

        return prefix;
    }
    
    private string GetPrefix(string group, string name) =>
        string.Join('.', new[] { "maverick", group, name }.Where(s => !string.IsNullOrWhiteSpace(s)))
              .Trim('.');

    private string GetVersionPostfixFor<T>()
    {
        Type entityType = typeof(T);

        MessageAttribute? messageAttribute =
            entityType.GetCustomAttributes<MessageAttribute>()
                .FirstOrDefault();

        ConsumerAttribute? consumerAttribute =
            entityType.GetCustomAttributes<ConsumerAttribute>()
                .FirstOrDefault();

        var customVersion = string.Empty;

        // use version from type-name as default.
        if (_versionPattern.Match(entityType.Name) is { Success: true, Value: var extractedVersion })
        {
            customVersion = extractedVersion;
        }

        // use version from message-attribute ...
        if (!string.IsNullOrWhiteSpace(messageAttribute?.Version))
        {
            customVersion = messageAttribute.Version;
        }

        // ... or from consumer-attribute as override.
        if (!string.IsNullOrWhiteSpace(consumerAttribute?.Version))
        {
            customVersion = consumerAttribute.Version;
        }

        // format version so that it's always 'v-{version}'
        if (!string.IsNullOrWhiteSpace(customVersion))
        {
            customVersion = $"-v{customVersion.ToLowerInvariant().TrimStart('v')}";
        }

        return customVersion;
    }

    private string StripVersionFrom(string name)
    {
        return _versionPattern.Replace(name, string.Empty).Trim('-');
    }

    /// <inheritdoc />
    public string CompensateActivity<T, TLog>()
        where T : class, ICompensateActivity<TLog>
        where TLog : class
    {
        return GeneratedOrCustomNameFor<T>(
            SanitizeName(
                GetCustomPrefixFor<T>()
                + StripVersionFrom(_endpointFormatter.CompensateActivity<T, TLog>())
                + GetVersionPostfixFor<T>()));
    }

    /// <inheritdoc />
    public string Consumer<T>()
        where T : class, IConsumer
    {
        return GeneratedOrCustomNameFor<T>(
            SanitizeName(
                GetCustomPrefixFor<T>()
                + StripVersionFrom(_endpointFormatter.Consumer<T>())
                + GetVersionPostfixFor<T>()));
    }

    /// <inheritdoc />
    public string ExecuteActivity<T, TArguments>()
        where T : class, IExecuteActivity<TArguments>
        where TArguments : class
    {
        return GeneratedOrCustomNameFor<T>(
            SanitizeName(
                GetCustomPrefixFor<T>()
                + StripVersionFrom(_endpointFormatter.ExecuteActivity<T, TArguments>())
                + GetVersionPostfixFor<T>()));
    }

    /// <inheritdoc />
    public string Message<T>()
        where T : class
    {
        return GeneratedOrCustomNameFor<T>(
            SanitizeName(
                GetCustomPrefixFor<T>()
                + StripVersionFrom(_endpointFormatter.Message<T>())
                + GetVersionPostfixFor<T>()));
    }

    /// <inheritdoc />
    public string Saga<T>()
        where T : class, ISaga
    {
        return GeneratedOrCustomNameFor<T>(
            SanitizeName(
                GetCustomPrefixFor<T>()
                + StripVersionFrom(_endpointFormatter.Saga<T>())
                + GetVersionPostfixFor<T>()));
    }

    /// <inheritdoc />
    public string SanitizeName(string name)
    {
        string defaultResult = _endpointFormatter.SanitizeName(name);

        return defaultResult;
    }

    /// <inheritdoc />
    public string TemporaryEndpoint(string tag)
    {
        return SanitizeName(
            GetPrefix(
                _serviceMetadata.ServiceGroup,
                _serviceMetadata.ServiceName)
            + "_"
            + _endpointFormatter.TemporaryEndpoint(tag));
    }

    /// <inheritdoc />
    public string FormatEntityName<T>()
    {
        return GeneratedOrCustomNameFor<T>(
            SanitizeName(
                GetCustomPrefixFor<T>()
                + StripVersionFrom(_entityFormatter.FormatEntityName<T>())
                + GetVersionPostfixFor<T>()));
    }
}
