using System;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     The object that are processed during the synchronization.
/// </summary>
public class RelationProcessingObject
{
    /// <summary>
    ///     Id of command to process relation object.
    /// </summary>
    public Guid CommandId => Result.Id;

    /// <summary>
    ///     The message of the processed object.
    /// </summary>
    public object Message { get; set; }

    /// <summary>
    ///     Shows a relations between an object that exists.
    ///     The relation can be an assignment, parent or child.
    /// </summary>
    public IRelation Relation { get; set; }

    /// <summary>
    ///     Defines the status of an entity during the synchronization process
    /// </summary>
    public CommandResult Result { get; set; }

    /// <summary>
    ///     Gets the message and cast to the given type.
    /// </summary>
    /// <typeparam name="T">The type that the message is cast to.</typeparam>
    /// <returns></returns>
    public T GetMessage<T>()
    {
        return (T)Message;
    }
}

/// <summary>
///     The object that are processed during the synchronization. The message here
///     has a generic type and is not an <inheritdoc cref="object" />.
/// </summary>
/// <typeparam name="T">The type that the message will receive.</typeparam>
public class RelationProcessingObject<T> : RelationProcessingObject where T : class
{
    /// <summary>
    ///     The message of the processed object.
    /// </summary>
    public new T Message
    {
        get => GetMessage<T>();
        set => base.Message = value;
    }
}
