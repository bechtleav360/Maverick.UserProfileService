using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Contains ArangoDB specific extension methods for various objects.
/// </summary>
public static class ArangoKeyExtensions
{
    /// <summary>
    ///     Adds an ArangoDB document key to an existing instance <paramref name="instance" />.<br />
    ///     The value of the key is passed by a <paramref name="keySelector" /> function.
    /// </summary>
    /// <returns>The modified <paramref name="instance" /> as <see cref="JObject" />.</returns>
    public static JObject InjectDocumentKey<TObject>(
        this TObject instance,
        Func<TObject, string> keySelector)
    {
        return InjectDocumentKey(instance, keySelector.Invoke(instance));
    }

    /// <summary>
    ///     Adds an ArangoDB document key to an existing instance <paramref name="instance" />.<br />
    ///     The value of the key is passed by a <paramref name="keySelector" /> function.
    /// </summary>
    /// <returns>The modified <paramref name="instance" /> as <see cref="JObject" />.</returns>
    public static JObject InjectDocumentKey<TObject>(
        this TObject instance,
        Func<TObject, string> keySelector,
        JsonSerializerSettings serializerSettings)
    {
        return InjectDocumentKey(instance, keySelector.Invoke(instance), serializerSettings);
    }

    /// <summary>
    ///     Adds an ArangoDB document key to an existing instance <paramref name="instance" />.<br />
    ///     The value of the key is passed by a <paramref name="keySelector" /> function.
    /// </summary>
    /// <returns>The modified <paramref name="instance" /> as <see cref="JObject" />.</returns>
    public static JObject InjectDocumentKey<TObject>(
        TObject instance,
        Func<TObject, string> keySelector,
        JsonSerializer serializer)
    {
        return InjectDocumentKey(instance, keySelector.Invoke(instance), serializer);
    }

    /// <summary>
    ///     Adds an ArangoDB document key to an existing instance <paramref name="instance" />.<br />
    ///     The value of the key is passed by <paramref name="keyValue" />.
    /// </summary>
    /// <returns>The modified <paramref name="instance" /> as <see cref="JObject" />.</returns>
    public static JObject InjectDocumentKey(
        object instance,
        string keyValue)
    {
        return InjectDocumentKey(instance, keyValue, JsonSerializer.CreateDefault());
    }

    /// <summary>
    ///     Adds an ArangoDB document key to an existing instance <paramref name="instance" />.<br />
    ///     The value of the key is passed by <paramref name="keyValue" />.
    /// </summary>
    /// <returns>The modified <paramref name="instance" /> as <see cref="JObject" />.</returns>
    public static JObject InjectDocumentKey(
        object instance,
        string keyValue,
        JsonSerializerSettings serializerSettings)
    {
        return InjectDocumentKey(instance, keyValue, JsonSerializer.CreateDefault(serializerSettings));
    }

    /// <summary>
    ///     Adds an ArangoDB document key to an existing instance <paramref name="instance" />.<br />
    ///     The value of the key is passed by <paramref name="keyValue" />.
    /// </summary>
    /// <returns>The modified <paramref name="instance" /> as <see cref="JObject" />.</returns>
    public static JObject InjectDocumentKey(
        object instance,
        string keyValue,
        JsonSerializer serializer)
    {
        JObject source = JObject.FromObject(instance, serializer);

        source.Merge(
            new JObject
            {
                { AConstants.KeySystemProperty, keyValue }
            });

        return source;
    }
}
