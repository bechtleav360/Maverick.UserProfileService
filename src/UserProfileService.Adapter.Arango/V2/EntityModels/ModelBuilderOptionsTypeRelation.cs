using System;
using System.Collections.Generic;
using System.Linq;
using UserProfileService.Adapter.Arango.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a type relation configuration for model builders.
/// </summary>
public class ModelBuilderOptionsTypeRelation : IModelBuilderSubclass
{
    /// <summary>
    ///     Gets or sets the name of the edge collection.
    /// </summary>
    public string EdgeCollection { get; private set; }

    /// <summary>
    ///     Gets the properties from the "from" side of the relation.
    /// </summary>
    public string[] FromProperties { get; }

    /// <summary>
    ///     Gets the type of the "from" side of the relation.
    /// </summary>
    public Type FromType { get; }

    /// <summary>
    ///     Gets the properties from the "to" side of the relation.
    /// </summary>
    public string[] ToProperties { get; }

    internal Type ToType { get; }

    /// <inheritdoc />
    public List<IModelBuilderSubclass> Children { get; } = new List<IModelBuilderSubclass>();

    /// <inheritdoc />
    public ModelBuilderOptions Options { get; }

    /// <inheritdoc />
    public IModelBuilderSubclass Parent { get; }

    private ModelBuilderOptionsTypeRelation(
        Type fromType,
        Type toType,
        IModelBuilderSubclass parent,
        string edgeCollection,
        IEnumerable<string> fromProperties,
        IEnumerable<string> toProperties)
    {
        FromType = fromType;
        ToType = toType;
        EdgeCollection = edgeCollection;
        Parent = parent;
        FromProperties = fromProperties.ToArray();
        ToProperties = toProperties.ToArray();
    }

    private ModelBuilderOptionsTypeRelation(
        Type fromType,
        Type toType,
        IModelBuilderSubclass parent,
        ModelBuilderOptions root)
    {
        FromType = fromType;
        ToType = toType;
        Options = root;
        Parent = parent;
    }

    internal static ModelBuilderOptionsTypeRelation Create<TFrom, TTo>(
        ModelBuilderOptions root,
        IModelBuilderSubclass parent)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        return new ModelBuilderOptionsTypeRelation(
            typeof(TFrom),
            typeof(TTo),
            parent,
            root);
    }

    internal static ModelBuilderOptionsTypeRelation Create<TFrom, TTo>(
        string edgeCollectionName,
        IModelBuilderSubclass parent,
        IEnumerable<string> fromProperties,
        IEnumerable<string> toProperties)
    {
        if (edgeCollectionName == null)
        {
            throw new ArgumentNullException(nameof(edgeCollectionName));
        }

        if (string.IsNullOrWhiteSpace(edgeCollectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(edgeCollectionName));
        }

        return new ModelBuilderOptionsTypeRelation(
            typeof(TFrom),
            typeof(TTo),
            parent,
            edgeCollectionName,
            fromProperties.ToArray(),
            toProperties.ToArray());
    }

    /// <summary>
    ///     Determines whether this <see cref="ModelBuilderOptionsTypeRelation"/> is equal to another instance.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    protected bool Equals(ModelBuilderOptionsTypeRelation other)
    {
        return FromType == other.FromType && ToType == other.ToType;
    }

    /// <inheritdoc />
    public void Build(string collectionPrefix, string queryCollectionPrefix)
    {
        if (collectionPrefix == null)
        {
            throw new ArgumentNullException(nameof(collectionPrefix));
        }

        if (EdgeCollection != null)
        {
            EdgeCollection = EdgeCollection.GetPrefixedCollectionName(collectionPrefix);

            return;
        }

        if (string.IsNullOrWhiteSpace(Options.GetCollectionName(FromType)))
        {
            throw new KeyNotFoundException(
                $"No valid key collectionName for type '{FromType.FullName}' found in provided {nameof(ModelBuilderOptions)}.");
        }

        if (string.IsNullOrWhiteSpace(Options.GetCollectionName(ToType)))
        {
            throw new KeyNotFoundException(
                $"No valid key collectionName for type '{ToType.FullName}' found in provided {nameof(ModelBuilderOptions)}.");
        }

        EdgeCollection =
            $"{Options.GetCollectionName(FromType)}_{Options.GetCollectionName(ToType)}".GetPrefixedCollectionName(
                collectionPrefix);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ModelBuilderOptionsTypeRelation)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(FromType, ToType);
    }
}
