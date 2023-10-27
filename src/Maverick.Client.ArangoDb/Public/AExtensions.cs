using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains extension methods for the ArangoDB client
/// </summary>
public static class AExtensions
{
    /// <summary>
    ///     Extracts an <see cref="ArangoObject{T}" /> from the properties dictionary of an ArangoDB restclient response and
    ///     returns it.
    /// </summary>
    /// <typeparam name="TData">Type of the data object inside the <see cref="ArangoObject{T}" />.</typeparam>
    /// <param name="resultObj">A dictionary of properties from an ArangoDB restclient response.</param>
    /// <returns>The <see cref="ArangoObject{T}" /> suitable to the <paramref name="resultObj" /> dictionary.</returns>
    public static ArangoObject<TData> GetArangoObjectGeneric<TData>(this Dictionary<string, object> resultObj)
        where TData : class, new()
    {
        return resultObj == null || !resultObj.ContainsKey(AConstants.IdSystemProperty)
            ? null
            : GetArangoObjectGeneric(resultObj, resultObj.ToObject<TData>());
    }

    /// <summary>
    ///     Extracts an <see cref="ArangoObject{T}" /> from the properties dictionary of an ArangoDB restclient response and
    ///     returns it.
    /// </summary>
    /// <typeparam name="TData">Type of the data object inside the <see cref="ArangoObject{T}" />.</typeparam>
    /// <param name="resultObj">A dictionary of properties from an ArangoDB restclient response.</param>
    /// <param name="dataObj">The data object to be embedded into the <see cref="ArangoObject{T}" />.</param>
    /// <returns>The <see cref="ArangoObject{T}" /> suitable to the <paramref name="resultObj" /> dictionary.</returns>
    public static ArangoObject<TData> GetArangoObjectGeneric<TData>(
        this Dictionary<string, object> resultObj,
        TData dataObj)
    {
        return resultObj == null
            || dataObj == null
            || !resultObj.ContainsKey(AConstants.IdSystemProperty)
            || !resultObj.ContainsKey(AConstants.KeySystemProperty)
                ? null
                : new ArangoObject<TData>(
                    resultObj.String(AConstants.IdSystemProperty),
                    resultObj.String(AConstants.KeySystemProperty),
                    dataObj);
    }

    /// <summary>
    ///     Extracts a non-generic <see cref="ArangoObject" /> from the properties dictionary of an ArangoDB restclient
    ///     response and returns it.
    /// </summary>
    /// <param name="resultObj">A dictionary of properties from an ArangoDB restclient response.</param>
    /// <param name="dataObj">The data object to be embedded into the non-generic <see cref="ArangoObject" />.</param>
    /// <returns>The <see cref="ArangoObject" /> suitable to the <paramref name="resultObj" /> dictionary.</returns>
    public static ArangoObject GetArangoObject(this Dictionary<string, object> resultObj, object dataObj)
    {
        return resultObj == null
            || dataObj == null
            || !resultObj.ContainsKey(AConstants.IdSystemProperty)
            || !resultObj.ContainsKey(AConstants.KeySystemProperty)
                ? null
                : new ArangoObject(resultObj.Id(), resultObj.Key(), dataObj);
    }

    /// <summary>
    ///     Binds a variable to the query request object <paramref name="queryObj" />, if <paramref name="condition" /> returns
    ///     true. Otherwise ignores the input.
    /// </summary>
    /// <typeparam name="TVal">Type of the value to be added.</typeparam>
    /// <param name="queryObj">Instance of the query object to be used for the request.</param>
    /// <param name="key">The key of the value to be added</param>
    /// <param name="value">The value to be added to the request.</param>
    /// <param name="condition">Conditional function, that is used to verify, if the parameter binding should be done.</param>
    /// <returns>The modified instance of the provided <paramref name="queryObj" />.</returns>
    public static AQuery BindVar<TVal>(this AQuery queryObj, string key, TVal value, Func<TVal, bool> condition)
    {
        return BindVar(queryObj, key, value, condition(value));
    }

    /// <summary>
    ///     Binds a variable to the query request object <paramref name="queryObj" />, if <paramref name="condition" /> is
    ///     true. Otherwise ignores the input.
    /// </summary>
    /// <param name="queryObj">Instance of the query object to be used for the request.</param>
    /// <param name="key">The key of the value to be added</param>
    /// <param name="value">The value to be added to the request.</param>
    /// <param name="condition">Conditional function, that is used to verify, if the parameter binding should be done.</param>
    /// <returns>The modified instance of the provided <paramref name="queryObj" />.</returns>
    public static AQuery BindVar(this AQuery queryObj, string key, object value, bool condition)
    {
        if (queryObj != null && condition)
        {
            queryObj.BindVar(key, value);
        }

        return queryObj;
    }

    /// <summary>
    ///     Returns the internal ArangoDB id (aka document handle).
    /// </summary>
    /// <param name="collection">Name of the collection</param>
    /// <param name="key">The key property</param>
    /// <returns>Internal id</returns>
    public static string GetArangoInternalId(string collection, string key)
    {
        return $"{collection}{AConstants.DocumentHandleSeparator}{key.EscapeKeyString()}";
    }

    /// <summary>
    ///     Returns an escaped version of the key string.
    /// </summary>
    /// <param name="key">String to be escaped for the ArangoDB key property.</param>
    /// <returns>Escaped version of the input string.</returns>
    public static string EscapeKeyString(this string key)
    {
        return key?.Replace("/", "%2F");
    }

    /// <summary>
    ///     Returns an unescaped version of the key string
    /// </summary>
    /// <param name="key">String to be escaped for the ArangoDB key property.</param>
    /// <returns>Escaped version of the input string.</returns>
    public static string UnescapeKeyString(this string key)
    {
        return key?.Replace("%2F", "/");
    }

    /// <summary>
    ///     Serializes the <paramref name="object" /> as json string and injects the result of
    ///     <paramref name="keyProjection" /> as ArangoDB key property to this json.
    /// </summary>
    /// <typeparam name="TObject">Type of the object to be serialized.</typeparam>
    /// <param name="object">The object to be serialized.</param>
    /// <param name="keyProjection">
    ///     A function that will used to get the ArangoDB key string to be added to the resulting json
    ///     string. (NOTE: All diacritics of the characters will be removed (i.e. Ö will be O).
    /// </param>
    /// <param name="converters">Json converters to be used.</param>
    /// <param name="settings">Json settings to be used.</param>
    /// <returns>Serialized version of the object as json string</returns>
    public static string SerializeObjectAndInjectKey<TObject>(
        this TObject @object,
        Func<TObject, string> keyProjection,
        JsonConverter[] converters = null,
        JsonSerializerSettings settings = null)
    {
        return SerializeObjectAndInjectKey(@object, keyProjection(@object), converters, settings);
    }

    /// <summary>
    ///     /// Serializes the <paramref name="object" /> as json string and injects the <paramref name="key" /> as property to
    ///     this json.
    /// </summary>
    /// <typeparam name="TObject">Type of the object to be serialized.</typeparam>
    /// <param name="object">The object to be serialized.</param>
    /// <param name="key">
    ///     The ArangoDB key property to be added to the resulting json string. (NOTE: All diacritics of the
    ///     characters will be removed (i.e. Ö will be O).
    /// </param>
    /// <param name="converters">Json converters to be used.</param>
    /// <param name="settings">Json settings to be used.</param>
    /// <returns>Serialized version of the object as json string</returns>
    public static string SerializeObjectAndInjectKey<TObject>(
        this TObject @object,
        string key,
        JsonConverter[] converters = null,
        JsonSerializerSettings settings = null)
    {
        string normalizedKey = key.RemoveDiacritics();

        return SerializeObjectAndInjectProperties(
            @object,
            new Dictionary<string, object>
            {
                { AConstants.KeySystemProperty, normalizedKey }
            },
            converters,
            settings);
    }

    /// <summary>
    ///     /// Serializes the <paramref name="object" /> as json string and injects the result of
    ///     <paramref name="keyProjection" /> as ArangoDB key property and additional properties to this json.
    /// </summary>
    /// <typeparam name="TObject">Type of the object to be serialized.</typeparam>
    /// <param name="object">The object to be serialized.</param>
    /// <param name="keyProjection">
    ///     A function that will used to get the ArangoDB key string to be added to the resulting json
    ///     string. (NOTE: All diacritics of the characters will be removed (i.e. Ö will be O).
    /// </param>
    /// <param name="converters">Json converters to be used.</param>
    /// <param name="settings">Json settings to be used.</param>
    /// <param name="additionalPropertiesToInject">
    ///     A dictionary of properties, that should be merged to the serialized
    ///     <paramref name="object" />.
    /// </param>
    /// <returns>Serialized version of the object as json string</returns>
    public static string SerializeObjectAndInjectKeyAndProperties<TObject>(
        this TObject @object,
        Func<TObject, string> keyProjection,
        Dictionary<string, object> additionalPropertiesToInject,
        JsonConverter[] converters = null,
        JsonSerializerSettings settings = null)
    {
        if (@object == null)
        {
            throw new ArgumentNullException(nameof(@object));
        }

        if (keyProjection == null)
        {
            throw new ArgumentNullException(nameof(keyProjection));
        }

        return SerializeObjectAndInjectKeyAndProperties(
            @object,
            keyProjection(@object),
            additionalPropertiesToInject,
            converters,
            settings);
    }

    /// <summary>
    ///     /// Serializes the <paramref name="object" /> as json string and injects the <paramref name="key" /> as property
    ///     and additional properties to this json.
    /// </summary>
    /// <typeparam name="TObject">Type of the object to be serialized.</typeparam>
    /// <param name="object">The object to be serialized.</param>
    /// <param name="key">
    ///     The ArangoDB key property to be added to the resulting json string. (NOTE: All diacritics of the
    ///     characters will be removed (i.e. Ö will be O).
    /// </param>
    /// <param name="converters">Json converters to be used.</param>
    /// <param name="settings">Json settings to be used.</param>
    /// <param name="additionalPropertiesToInject">
    ///     A dictionary of properties, that should be merged to the serialized
    ///     <paramref name="object" />.
    /// </param>
    /// <returns>Serialized version of the object as json string</returns>
    public static string SerializeObjectAndInjectKeyAndProperties<TObject>(
        this TObject @object,
        string key,
        Dictionary<string, object> additionalPropertiesToInject,
        JsonConverter[] converters = null,
        JsonSerializerSettings settings = null)
    {
        if (@object == null)
        {
            throw new ArgumentNullException(nameof(@object));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        Dictionary<string, object> newProps =
            additionalPropertiesToInject?.ToDictionary(kv => kv.Key, kv => kv.Value)
            ?? new Dictionary<string, object>();

        string normalizedKey = key.RemoveDiacritics();

        if (newProps.ContainsKey(AConstants.KeySystemProperty))
        {
            newProps[AConstants.KeySystemProperty] = normalizedKey;
        }
        else
        {
            newProps.Add(AConstants.KeySystemProperty, normalizedKey);
        }

        return SerializeObjectAndInjectProperties(@object, newProps, converters, settings);
    }

    /// <summary>
    ///     Serializes the <paramref name="object" /> as json string and injects all entries of the dictionary of properties to
    ///     this json.
    /// </summary>
    /// <typeparam name="TObject">Type of the object to be serialized.</typeparam>
    /// <param name="object">Object to be serialized.</param>
    /// <param name="propertiesToInject">
    ///     A dictionary of properties, that should be merged to the serialized
    ///     <paramref name="object" />.
    /// </param>
    /// <param name="converters">Json converters to be used.</param>
    /// <param name="settings">Json settings to be used.</param>
    /// <returns>Serialized version of the object as json string</returns>
    public static string SerializeObjectAndInjectProperties<TObject>(
        this TObject @object,
        Dictionary<string, object> propertiesToInject,
        JsonConverter[] converters = null,
        JsonSerializerSettings settings = null)
    {
        JObject jObj = JObject.FromObject(
            @object,
            JsonSerializer.CreateDefault(
                settings
                ?? new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.None
                }));

        if (propertiesToInject != null)
        {
            foreach (KeyValuePair<string, object> keyVal in propertiesToInject)
            {
                if (!string.IsNullOrEmpty(keyVal.Key) && jObj.Property(keyVal.Key) == null)
                {
                    jObj.Add(keyVal.Key, JToken.FromObject(keyVal.Value));
                }
            }
        }

        return converters != null
            ? jObj.ToString(Formatting.None, converters)
            : jObj.ToString(Formatting.None);
    }

    /// <summary>
    ///     Removes the diacritics from characters of a string and returns the modified string (i.e. Ö will be O).
    /// </summary>
    /// <param name="s">Input string to be modified.</param>
    /// <returns>Modified version of <paramref name="s" /></returns>
    public static string RemoveDiacritics(this string s)
    {
        string normalizedString = s.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     Gets the collection type name stored in a response dictionary.
    /// </summary>
    /// <param name="responseDirectory">Response object of a request to ArangoDB as dictionary.</param>
    /// <returns>The name of the collection type.</returns>
    public static string GetCollectionType(this Dictionary<string, object> responseDirectory)
    {
        if (responseDirectory == null || !responseDirectory.ContainsKey(AConstants.IdSystemProperty))
        {
            return null;
        }

        return responseDirectory
            .String(AConstants.IdSystemProperty)
            .Split(AConstants.DocumentHandleSeparator.First())
            .FirstOrDefault();
    }
}
