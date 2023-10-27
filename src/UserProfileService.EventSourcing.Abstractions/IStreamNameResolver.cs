using System.Text.RegularExpressions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.EventSourcing.Abstractions;

/// <summary>
///     The first level stream name resolves the the stream name an <see cref="IUserProfileServiceEvent" />
///     has to be written. An <inheritdoc cref="ObjectIdent" /> is needed to generate/resolve the stream name.
///     That object contains the object type and the id of the object. From that on the stream name is created.
/// </summary>
public interface IStreamNameResolver
{
    /// <summary>
    ///     The method resolves the stream name for an event by an <see cref="ObjectIdent" />.
    /// </summary>
    /// <param name="objectIdentifier">
    ///     The object identifier that contains the type of object and the id of the object. From
    ///     that two properties a stream name will be created.
    /// </param>
    /// <returns>The target stream that is used to write the event to.</returns>
    string GetStreamName(ObjectIdent objectIdentifier);

    /// <summary>
    ///     Returns the object ident related to the provided <paramref name="streamName" />.
    /// </summary>
    /// <param name="streamName">The stream name whose object ident is requested.</param>
    /// <returns>The related object identifier.</returns>
    ObjectIdent GetObjectIdentUsingStreamName(string streamName);

    /// <summary>
    ///     Return the pattern to resolve stream.
    /// </summary>
    /// <returns>The stream name pattern.</returns>
    Regex GetStreamNamePattern();
}
