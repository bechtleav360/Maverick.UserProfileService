using System;
using System.Reflection;
using System.Security;
using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     Represents errors that will occur if an <see cref="IUserProfileServiceEvent" /> (domain event) is not valid.
/// </summary>
public class InvalidDomainEventException : Exception
{
    /// <summary>
    ///     The regarding domain event data that caused the error.
    /// </summary>
    public IUserProfileServiceEvent DomainEvent { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidDomainEventException" /> without specifying any message or further
    ///     data regarding the exception or it's cause.
    /// </summary>
    public InvalidDomainEventException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidDomainEventException" /> with a specified error
    ///     <paramref name="message" />.
    /// </summary>
    /// <param name="message">A message containing information about the error or it's cause.</param>
    public InvalidDomainEventException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidDomainEventException" /> with a specified error
    ///     <paramref name="message" /> and a reference to an inner exception that caused the error.
    /// </summary>
    /// <param name="message">A message containing information about the error or it's cause.</param>
    /// <param name="innerException">A reference to the exception that caused the error.</param>
    public InvalidDomainEventException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidDomainEventException" /> with a specified <paramref name="message" />
    ///     containing information about the error and the <paramref name="domainEvent" /> that caused the error.
    /// </summary>
    /// <param name="message">A message containing information about the error or it's cause.</param>
    /// <param name="domainEvent">The regarding domain event data that caused the error.</param>
    public InvalidDomainEventException(
        string message,
        IUserProfileServiceEvent domainEvent)
        : base(message)
    {
        DomainEvent = domainEvent;
    }

    /// <inheritdoc />
    [SecurityCritical]
    public override string ToString()
    {
        if (DomainEvent == null)
        {
            return base.ToString();
        }

        // only pass ToString() data of the event, if it's ToString() method has been implemented properly
        MethodInfo methodInfo = DomainEvent.GetType().GetMethod(nameof(ToString), Array.Empty<Type>());

        string domainEventDetails =
            methodInfo != null && methodInfo.DeclaringType == DomainEvent.GetType()
                ? $"{Environment.NewLine}{DomainEvent}"
                : $"{DomainEvent.Type} (id: {DomainEvent.EventId})";

        return
            $"{base.ToString()}{Environment.NewLine}Related event: {domainEventDetails}";
    }
}
