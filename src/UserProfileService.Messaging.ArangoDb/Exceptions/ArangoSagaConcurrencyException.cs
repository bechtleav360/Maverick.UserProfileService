using System;
using MassTransit;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Exceptions;

/// <summary>
///     Error thrown when the version of the saga does not match the expected version.
///     Occurs when several messages arrive at the same time and the version has already been changed.
///     The following messages are based on a different status, which is currently no longer in the database.
/// </summary>
[Serializable]
public class ArangoSagaConcurrencyException :
    ConcurrencyException
{
    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaConcurrencyException" />.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="sagaType">Type of saga where the error occurred.</param>
    /// <param name="correlationId">Identifier of the related saga.</param>
    public ArangoSagaConcurrencyException(string message, Type sagaType, Guid correlationId)
        : base(message, sagaType, correlationId)
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaConcurrencyException" />.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="sagaType">Type of saga where the error occurred.</param>
    /// <param name="correlationId">Identifier of the related saga.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ArangoSagaConcurrencyException(
        string message,
        Type sagaType,
        Guid correlationId,
        Exception innerException)
        : base(message, sagaType, correlationId, innerException)
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaConcurrencyException" />.
    /// </summary>
    public ArangoSagaConcurrencyException()
    {
    }
}
