using System;

namespace UserProfileService.Common.V2.Exceptions;

public class NotValidException : Exception
{
    public NotValidException(string message) : base(message)
    {
    }

    public NotValidException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
