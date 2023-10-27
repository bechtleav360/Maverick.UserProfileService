using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

public static class Dictator
{
    /// <summary>
    ///     Creates new schema validator.
    /// </summary>
    public static Schema Schema => new Schema();

    /// <summary>
    ///     Contains global settings which affects various operations.
    /// </summary>
    public static DictatorSettings Settings { get; }

    static Dictator()
    {
        Settings = new DictatorSettings();
    }

    private static List<object> ToList(object inputCollection)
    {
        Type collectionType = inputCollection.GetType();
        var outputCollection = new List<object>();
        var collection = (IList)inputCollection;

        if (collection.Count > 0)
        {
            // create instance of property type
            object collectionInstance = Activator.CreateInstance(collectionType, collection.Count);

            for (var i = 0; i < collection.Count; i++)
            {
                Type elementType = collection[i].GetType();

                // collection is simple array
                if (collectionType.IsArray)
                {
                    outputCollection.Add(collection[i]);
                }
                // collection is generic
                else if (collectionType.IsGenericType && collection is IEnumerable)
                {
                    // generic collection consists of basic types
                    if (elementType.IsPrimitive
                        || elementType == typeof(string)
                        || elementType == typeof(DateTime)
                        || elementType == typeof(decimal))
                    {
                        outputCollection.Add(collection[i]);
                    }
                    // generic collection consists of generic type which should be parsed
                    else
                    {
                        // create instance object based on first element of generic collection
                        object instance = Activator.CreateInstance(
                            collectionType.GetGenericArguments().First(),
                            null);

                        outputCollection.Add(ToDocument(collection[i]));
                    }
                }
                else
                {
                    object obj = Activator.CreateInstance(elementType, collection[i]);

                    outputCollection.Add(obj);
                }
            }
        }

        return outputCollection;
    }

    /// <summary>
    ///     Creates new empty document.
    /// </summary>
    public static Dictionary<string, object> New()
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    ///     Converts specified dictionary list into collection of strongly typed objects.
    /// </summary>
    public static List<T> ToList<T>(List<Dictionary<string, object>> documents)
    {
        var list = new List<T>();

        foreach (Dictionary<string, object> document in documents)
        {
            list.Add(document.ToObject<T>());
        }

        return list;
    }

    /// <summary>
    ///     Converts specified object into document.
    /// </summary>
    public static Dictionary<string, object> ToDocument(object obj)
    {
        Type inputObjectType = obj.GetType();
        var document = new Dictionary<string, object>();

        if (obj is Dictionary<string, object> objects)
        {
            document = objects.Clone();
        }
        else
        {
            foreach (PropertyInfo propertyInfo in inputObjectType.GetProperties(
                         BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldName = propertyInfo.Name;
                object propertyValue = propertyInfo.GetValue(obj);
                IEnumerable<Attribute> customAttributes = propertyInfo.GetCustomAttributes();
                var skipField = false;

                foreach (Attribute attribute in customAttributes)
                {
                    // skip property if it should be ingored
                    if (attribute is IgnoreField)
                    {
                        skipField = true;
                    }
                    // skip property if it should ingore null value
                    else if (attribute is IgnoreNullValue && propertyValue == null)
                    {
                        skipField = true;
                    }
                    // set field name as property alias if present
                    else if (attribute is AliasField)
                    {
                        var aliasFieldAttribute = (AliasField)propertyInfo.GetCustomAttribute(typeof(AliasField));

                        fieldName = aliasFieldAttribute.Alias;
                    }
                }

                if (skipField)
                {
                    continue;
                }

                if (propertyValue == null)
                {
                    document.Object(fieldName, null);
                }
                else if (propertyValue is IDictionary)
                {
                    document.Object(fieldName, propertyValue);
                }
                // property is array or collection
                else if ((propertyInfo.PropertyType.IsArray || propertyInfo.PropertyType.IsGenericType)
                         && propertyValue is IList)
                {
                    document.List(fieldName, ToList(propertyValue));
                }
                // property is class except the string type since string values are parsed differently
                else if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType.Name != "String")
                {
                    document.Object(fieldName, ToDocument(propertyValue));
                }
                // property is basic type
                else
                {
                    document.Object(fieldName, propertyValue);
                }
            }
        }

        return document;
    }

    /// <summary>
    ///     Converts specified object list into collection of documents.
    /// </summary>
    public static List<Dictionary<string, object>> ToDocuments<T>(List<T> objects)
    {
        return objects.Select(item => ToDocument(item)).ToList();
    }
}
