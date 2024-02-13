﻿using System;
using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Extension class that contains ArangoDB specific extension methods, Dictator implementation code can be found in external libraries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    ///     Checks if specified field path has valid document ID value in the format of `collection/key`.
    /// </summary>
    public static bool IsId(this Dictionary<string, object> dictionary, string fieldPath)
    {
        var isValid = false;

        try
        {
            object fieldValue =
                ExternalLibraries.dictator.DictionaryExtensions.GetFieldValue(dictionary, fieldPath);

            if (fieldValue is string)
            {
                return ADocument.IsId((string)fieldValue);
            }
        }
        catch (Exception)
        {
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    ///     Checks if specified field path has valid document key value.
    /// </summary>
    public static bool IsKey(this Dictionary<string, object> dictionary, string fieldPath)
    {
        var isValid = false;

        try
        {
            object fieldValue =
                ExternalLibraries.dictator.DictionaryExtensions.GetFieldValue(dictionary, fieldPath);

            if (fieldValue is string)
            {
                return ADocument.IsKey((string)fieldValue);
            }
        }
        catch (Exception)
        {
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    ///     Checks if `_id` field is present and has valid format.
    /// </summary>
    public static bool HasId(this Dictionary<string, object> dictionary)
    {
        return !string.IsNullOrEmpty(Id(dictionary));
    }

    /// <summary>
    ///     Retrieves value of `_id` field. If the field is missing or has invalid format null value is returned.
    /// </summary>
    public static string Id(this Dictionary<string, object> dictionary)
    {
        string id;

        try
        {
            id = ExternalLibraries.dictator.DictionaryExtensions.String(dictionary, "_id");

            if (!ADocument.IsId(id))
            {
                id = null;
            }
        }
        catch (Exception)
        {
            id = null;
        }

        return id;
    }

    /// <summary>
    ///     Stores `_id` field value.
    /// </summary>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    public static Dictionary<string, object> Id(this Dictionary<string, object> dictionary, string id)
    {
        if (!ADocument.IsId(id))
        {
            throw new ArgumentException("Specified id value (" + id + ") has invalid format.");
        }

        ExternalLibraries.dictator.DictionaryExtensions.SetFieldValue(dictionary, "_id", id);

        return dictionary;
    }

    /// <summary>
    ///     Checks if `_key` field is present and has valid format.
    /// </summary>
    public static bool HasKey(this Dictionary<string, object> dictionary)
    {
        return !string.IsNullOrEmpty(Key(dictionary));
    }

    /// <summary>
    ///     Retrieves value of `_key` field. If the field is missing or has invalid format null value is returned.
    /// </summary>
    public static string Key(this Dictionary<string, object> dictionary)
    {
        string key;

        try
        {
            key = ExternalLibraries.dictator.DictionaryExtensions.String(dictionary, "_key");

            if (!ADocument.IsKey(key))
            {
                key = null;
            }
        }
        catch (Exception)
        {
            key = null;
        }

        return key;
    }

    /// <summary>
    ///     Stores `_key` field value.
    /// </summary>
    /// <exception cref="ArgumentException">Specified key value has invalid format.</exception>
    public static Dictionary<string, object> Key(this Dictionary<string, object> dictionary, string key)
    {
        if (!ADocument.IsKey(key))
        {
            throw new ArgumentException("Specified key value (" + key + ") has invalid format.");
        }

        ExternalLibraries.dictator.DictionaryExtensions.SetFieldValue(dictionary, "_key", key);

        return dictionary;
    }

    /// <summary>
    ///     Checks if `_rev` field is present and has valid format.
    /// </summary>
    public static bool HasRev(this Dictionary<string, object> dictionary)
    {
        return !string.IsNullOrEmpty(Rev(dictionary));
    }

    /// <summary>
    ///     Retrieves value of `_rev` field. If the field is missing or has invalid format null value is returned.
    /// </summary>
    public static string Rev(this Dictionary<string, object> dictionary)
    {
        string rev;

        try
        {
            rev = ExternalLibraries.dictator.DictionaryExtensions.String(dictionary, "_rev");

            if (!ADocument.IsRev(rev))
            {
                rev = null;
            }
        }
        catch (Exception)
        {
            rev = null;
        }

        return rev;
    }

    /// <summary>
    ///     Stores `_rev` field value.
    /// </summary>
    /// <exception cref="ArgumentException">Specified rev value has invalid format.</exception>
    public static Dictionary<string, object> Rev(this Dictionary<string, object> dictionary, string rev)
    {
        if (!ADocument.IsRev(rev))
        {
            throw new ArgumentException("Specified rev value (" + rev + ") has invalid format.");
        }

        ExternalLibraries.dictator.DictionaryExtensions.SetFieldValue(dictionary, "_rev", rev);

        return dictionary;
    }

    /// <summary>
    ///     Checks if `_from` field is present and has valid format.
    /// </summary>
    public static bool HasFrom(this Dictionary<string, object> dictionary)
    {
        return !string.IsNullOrEmpty(From(dictionary));
    }

    /// <summary>
    ///     Retrieves value of `_from` field. If the field is missing or has invalid format null value is returned.
    /// </summary>
    public static string From(this Dictionary<string, object> dictionary)
    {
        string from;

        try
        {
            from = ExternalLibraries.dictator.DictionaryExtensions.String(dictionary, "_from");

            if (!ADocument.IsId(from))
            {
                from = null;
            }
        }
        catch (Exception)
        {
            from = null;
        }

        return from;
    }

    /// <summary>
    ///     Stores `_from` field value.
    /// </summary>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    public static Dictionary<string, object> From(this Dictionary<string, object> dictionary, string id)
    {
        if (!ADocument.IsId(id))
        {
            throw new ArgumentException("Specified id value (" + id + ") has invalid format.");
        }

        ExternalLibraries.dictator.DictionaryExtensions.SetFieldValue(dictionary, "_from", id);

        return dictionary;
    }

    /// <summary>
    ///     Checks if `_to` field is present and has valid format.
    /// </summary>
    public static bool HasTo(this Dictionary<string, object> dictionary)
    {
        return !string.IsNullOrEmpty(To(dictionary));
    }

    /// <summary>
    ///     Retrieves value of `_to` field. If the field is missing or has invalid format null value is returned.
    /// </summary>
    public static string To(this Dictionary<string, object> dictionary)
    {
        string to;

        try
        {
            to = ExternalLibraries.dictator.DictionaryExtensions.String(dictionary, "_to");

            if (!ADocument.IsId(to))
            {
                to = null;
            }
        }
        catch (Exception)
        {
            to = null;
        }

        return to;
    }

    /// <summary>
    ///     Stores `_to` field value.
    /// </summary>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    public static Dictionary<string, object> To(this Dictionary<string, object> dictionary, string id)
    {
        if (!ADocument.IsId(id))
        {
            throw new ArgumentException("Specified id value (" + id + ") has invalid format.");
        }

        ExternalLibraries.dictator.DictionaryExtensions.SetFieldValue(dictionary, "_to", id);

        return dictionary;
    }
}
