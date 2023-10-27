using System;
using UserProfileService.Adapter.Arango.V2.Abstractions;

namespace UserProfileService.Adapter.Arango.V2.Annotations;

/// <summary>
///     Defines a virtual property in an ArangoDb entity model. This will be mapped to a given existent property,
///     because it has no field in the database/collection.<br />
///     There are two possible ways to set up a filter - simple filter property name-value combination or a
///     <see cref="IVirtualPropertyResolver" />.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class VirtualPropertyAttribute : Attribute
{
    /// <summary>
    ///     The name of the property to be used as filter (like profile kind).
    /// </summary>
    public string FilterPropertyName { get; }

    /// <summary>
    ///     The value of the property that is valid for the filter (i.e. value of profile kind).
    /// </summary>
    public object FilterPropertyValue { get; }

    /// <summary>
    ///     Name of the mapped property that should be stored in the database collection.
    /// </summary>
    public string NameRealProperty { get; }

    /// <summary>
    ///     The parent type of the attribute (i.e. UserEntityModel).
    /// </summary>
    public Type ParentType { get; }

    /// <summary>
    ///     The mapped resolver that will contain all information for filtering.
    /// </summary>
    public IVirtualPropertyResolver Resolver { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="VirtualPropertyAttribute" />.
    /// </summary>
    public VirtualPropertyAttribute(
        Type parentType,
        string nameRealProperty,
        string filterPropertyName,
        object filterPropertyValue)
    {
        ParentType = parentType ?? throw new ArgumentNullException(nameof(parentType));
        NameRealProperty = nameRealProperty ?? throw new ArgumentNullException(nameof(nameRealProperty));
        FilterPropertyName = filterPropertyName ?? throw new ArgumentNullException(nameof(filterPropertyName));
        FilterPropertyValue = filterPropertyValue ?? throw new ArgumentNullException(nameof(filterPropertyValue));
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="VirtualPropertyAttribute" />.
    /// </summary>
    public VirtualPropertyAttribute(
        Type parentType,
        string nameRealProperty,
        Type resolverType)
    {
        if (resolverType == null)
        {
            throw new ArgumentNullException(nameof(resolverType));
        }

        ParentType = parentType ?? throw new ArgumentNullException(nameof(parentType));
        NameRealProperty = nameRealProperty ?? throw new ArgumentNullException(nameof(nameRealProperty));

        if (!typeof(IVirtualPropertyResolver).IsAssignableFrom(resolverType))
        {
            throw new Exception(
                $"Internal error while constructing '{resolverType.Name}'. The resolver type cannot be inherited from IVirtualPropertyFilterResolver.");
        }

        if (resolverType.IsInterface)
        {
            throw new Exception(
                $"Internal error while constructing '{resolverType.Name}'. The resolver type cannot be an interface.");
        }

        try
        {
            Resolver = (IVirtualPropertyResolver)Activator.CreateInstance(resolverType);
        }
        catch (Exception e)
        {
            throw new Exception($"Internal error while constructing '{resolverType.Name}'. {e.Message}", e);
        }
    }
}
