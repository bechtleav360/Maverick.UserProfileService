using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     The exception that is thrown when an instance was not found in the backend during an read or write operation.
/// </summary>
public class InstanceNotFoundException : Exception
{
    /// <summary>
    ///     Gets or sets a code, that can be used to categorize the error.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the instance that could not be found.
    /// </summary>
    public string RelatedId { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceNotFoundException" /> class, without setting the
    ///     <see cref="Code" /> and the <see cref="Exception.Message" /> properties.
    /// </summary>
    public InstanceNotFoundException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceNotFoundException" /> class with a specified error message,
    ///     but without setting the <see cref="Code" /> property.
    /// </summary>
    /// <param name="message">The error message string.</param>
    public InstanceNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceNotFoundException" /> class with a specified error message and
    ///     a reference to the inner exception that is the cause of this exception. The <see cref="Code" /> property won't be
    ///     set.
    /// </summary>
    /// <param name="message">The error message string.</param>
    /// <param name="inner">The inner exception reference.</param>
    public InstanceNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceNotFoundException" /> class with a specified error message and
    ///     a specified error code.
    /// </summary>
    /// <param name="code">An error code that can be used to categorize the error.</param>
    /// <param name="message">The error message string.</param>
    public InstanceNotFoundException(string code, string message) : base(message)
    {
        Code = code;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceNotFoundException" /> class with a specified error message, an
    ///     id of the regarding instance and a specified error code.
    /// </summary>
    /// <param name="code">An error code that can be used to categorize the error.</param>
    /// <param name="message">The error message string.</param>
    /// <param name="relatedId">The identifier of the instance that could not be found.</param>
    public InstanceNotFoundException(string code, string relatedId, string message) : base(message)
    {
        Code = code;
        RelatedId = relatedId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceNotFoundException" /> class with a specified error message, a
    ///     specified error code and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="code">An error code that can be used to categorize the error.</param>
    /// <param name="message">The error message string.</param>
    /// <param name="inner">The inner exception reference.</param>
    public InstanceNotFoundException(string code, string message, Exception inner)
        : base(message, inner)
    {
        Code = code;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceNotFoundException" /> class with a specified error message, a
    ///     specified error code, an id of the regarding instance and a reference to the inner exception that is the cause of
    ///     this exception.
    /// </summary>
    /// <param name="code">An error code that can be used to categorize the error.</param>
    /// <param name="relatedId">The identifier of the instance that could not be found.</param>
    /// <param name="message">The error message string.</param>
    /// <param name="inner">The inner exception reference.</param>
    public InstanceNotFoundException(string code, string relatedId, string message, Exception inner)
        : base(message, inner)
    {
        Code = code;
        RelatedId = relatedId;
    }

    /// <summary>Creates and returns a string representation of the current exception.</summary>
    /// <returns>A string representation of the current exception.</returns>
    /// <PermissionSet>
    ///     <IPermission
    ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
    ///         version="1" PathDiscovery="*AllFiles*" />
    /// </PermissionSet>
    public override string ToString()
    {
        string s = GetType().Name + ": " + Message;

        if (!string.IsNullOrEmpty(Code) || !string.IsNullOrEmpty(RelatedId))
        {
            s += "(";
        }

        if (!string.IsNullOrEmpty(Code))
        {
            s = $"{s}Code: {Code}";
        }

        if (!string.IsNullOrEmpty(RelatedId))
        {
            s = $"{s}{(!string.IsNullOrEmpty(Code) ? "; " : "")}Related id:{RelatedId}";
        }

        if (!string.IsNullOrEmpty(Code) || !string.IsNullOrEmpty(RelatedId))
        {
            s += ")";
        }

        if (InnerException != null)
        {
            s = s + " ---> " + InnerException;
        }

        if (StackTrace != null)
        {
            s += Environment.NewLine + StackTrace;
        }

        return s;
    }
}
