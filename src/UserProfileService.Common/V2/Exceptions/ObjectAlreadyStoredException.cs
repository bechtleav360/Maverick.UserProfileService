using System;

namespace UserProfileService.Common.V2.Exceptions;

public class ObjectAlreadyStoredException : Exception
{
    public ObjectAlreadyStoredException(string message) : base(message)
    {
    }
}
