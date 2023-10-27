using System;

namespace UserProfileService.Common.V2.Exceptions;

public class ObjectNotValidException : Exception
{
    public ObjectNotValidException(string message) : base(message)
    {
    }
}
