using System;

namespace UserProfileService.Common.V2.Exceptions;

public class AlreadyExistsException : Exception
{
    public AlreadyExistsException()
    {
    }

    public AlreadyExistsException(string message) : base(message)
    {
    }
}
