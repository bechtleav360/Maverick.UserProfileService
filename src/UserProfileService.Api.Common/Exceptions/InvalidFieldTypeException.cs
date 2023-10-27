namespace UserProfileService.Api.Common.Exceptions;

/// <summary>
///     Describe an exception that is being thrown when a field type is invalid.
/// </summary>
public class InvalidFieldTypeException : Exception
{
    
    /// <inheritdoc/>
    public InvalidFieldTypeException(string message) : base(message)
    {
    }
}
