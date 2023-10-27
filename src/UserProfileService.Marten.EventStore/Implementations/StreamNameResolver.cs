using System.Text.RegularExpressions;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Marten.EventStore.Options;

namespace UserProfileService.Marten.EventStore.Implementations;

/// <summary>
/// </summary>
internal class StreamNameResolver : IStreamNameResolver
{
    private readonly MartenEventStoreOptions _eventStoreConfiguration;
    private readonly ILogger<StreamNameResolver> _logger;

    /// <summary>
    /// </summary>
    /// <param name="eventStoreConfiguration"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public StreamNameResolver(
        IOptionsSnapshot<MartenEventStoreOptions> eventStoreConfiguration,
        ILogger<StreamNameResolver> logger)
    {
        _logger = logger;

        _eventStoreConfiguration = eventStoreConfiguration.Value
            ?? throw new ArgumentNullException(nameof(eventStoreConfiguration));
    }

    /// <summary>
    /// </summary>
    /// <param name="objectIdentifier"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public string GetStreamName(ObjectIdent objectIdentifier)
    {
        _logger.EnterMethod();

        if (objectIdentifier == null)
        {
            throw new ArgumentNullException(nameof(objectIdentifier));
        }

        if (string.IsNullOrEmpty(objectIdentifier.Id))
        {
            throw new ArgumentException(
                "The id of the object ident is null or empty. No stream name could be created.",
                nameof(objectIdentifier));
        }

        if (objectIdentifier.Type == ObjectType.Unknown)
        {
            throw new ArgumentException(
                "The object type of the object ident in unknown. No stream name could be created.",
                nameof(objectIdentifier));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "The objectIdent has the value: {objectIdentifier}",
                objectIdentifier.ToLogString().AsArgumentList());
        }

        var streamName =
            $"{_eventStoreConfiguration.StreamNamePrefix}_{objectIdentifier.Id}_{objectIdentifier.Type}";

        _logger.LogInfoMessage(
            "The object indent {objectIdent}, has created the stream name: {streamName}",
            LogHelpers.Arguments(
                objectIdentifier.ToLogString(),
                streamName.ToLogString()));

        return _logger.ExitMethod<string>(streamName);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="streamName" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="streamName" /> is empty or whitespace.<br />-or-<br />
    ///     <paramref name="streamName" /> does not follow the required pattern
    /// </exception>
    /// <exception cref="FormatException">
    ///     <paramref name="streamName" /> does not contain any identifier value<br />-or-<br />
    ///     <paramref name="streamName" /> does not contain any type value<br />-or-<br />
    ///     <paramref name="streamName" /> contains a type value that is not a valid <see cref="ObjectType" />
    /// </exception>
    public ObjectIdent GetObjectIdentUsingStreamName(string streamName)
    {
        _logger.EnterMethod();

        if (streamName == null)
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("streamName cannot be null or whitespace.", nameof(streamName));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "The streamName has the value: {streamName}",
                streamName.ToLogString().AsArgumentList());
        }

        Match match = GetStreamNamePattern().Match(streamName);

        if (!match.Success)
        {
            throw new ArgumentException(
                "streamName does not follow the required pattern.",
                nameof(streamName));
        }

        if (string.IsNullOrWhiteSpace(match.Groups["id"].Value))
        {
            throw new FormatException("Could not parse identifier part from streamName string.");
        }

        if (string.IsNullOrWhiteSpace(match.Groups["type"].Value))
        {
            throw new FormatException("Could not parse type part from streamName string.");
        }

        if (!Enum.TryParse(match.Groups["type"].Value, true, out ObjectType objectType))
        {
            throw new FormatException("Found type part in streamName is not a valid ObjectType enum.");
        }

        var objectIdentResult = new ObjectIdent(
            match.Groups["id"].Value,
            objectType);

        _logger.LogInfoMessage(
            "The stream name: {streamName} has created the objectIdent: {objectIdentResult}",
            LogHelpers.Arguments(streamName.ToLogString(), objectIdentResult.ToLogString()));

        return _logger.ExitMethod(objectIdentResult);
    }

    /// <inheritdoc />
    public Regex GetStreamNamePattern()
    {
        return new Regex($"^{_eventStoreConfiguration.StreamNamePrefix}_(?<id>[^_]+)_(?<type>.*)$");
    }
}
