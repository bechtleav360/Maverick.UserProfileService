﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
#if !SILVERLIGHT
#endif

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;

/// <summary>
///     Represents a delegate for serializing an object to a string.
/// </summary>
/// <param name="data">The object to serialize.</param>
/// <returns>The serialized string representation of the object.</returns>
public delegate string Serialize(object data);

/// <summary>
///     Represents a delegate for deserializing a string to an object.
/// </summary>
/// <param name="data">The serialized string data.</param>
/// <returns>The deserialized object.</returns>
public delegate object Deserialize(string data);

/// <summary>
///     Represents parameters for JSON serialization.
/// </summary>
public sealed class JsonParameters
{
    /// <summary>
    ///     Serialize DateTime milliseconds i.e. yyyy-MM-dd HH:mm:ss.nnn (default = false)
    /// </summary>
    public bool DateTimeMilliseconds = false;

    /// <summary>
    ///     Anonymous types have read only properties
    /// </summary>
    public bool EnableAnonymousTypes = false;

    /// <summary>
    ///     Ignore attributes to check for (default : XmlIgnoreAttribute)
    /// </summary>
    public List<Type> IgnoreAttributes = new List<Type>
    {
        typeof(XmlIgnoreAttribute)
    };

    /// <summary>
    ///     Ignore case when processing json and deserializing
    /// </summary>
    [Obsolete("Not needed anymore and will always match")]
    public bool IgnoreCaseOnDeserialize = false;

    /// <summary>
    ///     Inline circular or already seen objects instead of replacement with $i (default = False)
    /// </summary>
    public bool InlineCircularReferences;

    /// <summary>
    ///     Output string key dictionaries as "k"/"v" format (default = False)
    /// </summary>
    public bool KvStyleStringDictionary = false;

    /// <summary>
    ///     If you have parametric and no default constructor for you classes (default = False)
    ///     IMPORTANT NOTE : If True then all initial values within the class will be ignored and will be not set
    /// </summary>
    public bool ParametricConstructorOverride = false;

    /// <summary>
    ///     Serialize null values to the output (default = True)
    /// </summary>
    public bool SerializeNullValues = true;

    /// <summary>
    ///     Maximum depth for circular references in inline mode (default = 20)
    /// </summary>
    public byte SerializerMaxDepth = 20;

    /// <summary>
    ///     Save property/field names as lowercase (default = false)
    /// </summary>
    public bool SerializeToLowerCaseNames = false;

    /// <summary>
    ///     Show the readonly properties of types in the output (default = False)
    /// </summary>
    public bool ShowReadOnlyProperties;

    /// <summary>
    ///     Use escaped unicode i.e. \uXXXX format for non ASCII characters (default = True)
    /// </summary>
    public bool UseEscapedUnicode = true;

    /// <summary>
    ///     Enable fastJSON extensions $types, $type, $map (default = True)
    /// </summary>
    public bool UseExtensions = true;

    /// <summary>
    ///     Use the fast GUID format (default = True)
    /// </summary>
    public bool UseFastGuid = true;

    /// <summary>
    ///     Use the optimized fast Dataset Schema format (default = True)
    /// </summary>
    public bool UseOptimizedDataSetSchema = true;

    /// <summary>
    ///     Use the UTC date format (default = True)
    /// </summary>
    public bool UseUtcDateTime = true;

    /// <summary>
    ///     Output Enum values instead of names (default = False)
    /// </summary>
    public bool UseValuesOfEnums = false;

    /// <summary>
    ///     Use the $types extension to optimise the output json (default = True)
    /// </summary>
    public bool UsingGlobalTypes = true;

    /// <summary>
    ///     Fix inconsistent configuration combinations.
    /// </summary>
    public void FixValues()
    {
        if (UseExtensions == false) // disable conflicting params
        {
            UsingGlobalTypes = false;
            InlineCircularReferences = true;
        }

        if (EnableAnonymousTypes)
        {
            ShowReadOnlyProperties = true;
        }
    }
}

/// <summary>
///     Utility class containing functions to (de-)serialize JSON objects.
/// </summary>
public static class Json
{
    /// <summary>
    ///     Globally set-able parameters for controlling the serializer
    /// </summary>
    public static JsonParameters Parameters = new JsonParameters();

    /// <summary>
    ///     Create a formatted json string (beautified) from an object
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static string ToNiceJson(object obj, JsonParameters param)
    {
        string s = ToJson(obj, param);

        return Beautify(s);
    }

    /// <summary>
    ///     Create a json representation for an object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string ToJson(object obj)
    {
        return ToJson(obj, Parameters);
    }

    /// <summary>
    ///     Create a json representation for an object with parameter override on this call
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static string ToJson(object obj, JsonParameters param)
    {
        param.FixValues();
        Type t = null;

        if (obj == null)
        {
            return "null";
        }

        if (obj.GetType().IsGenericType)
        {
            t = Reflection.Instance.GetGenericTypeDefinition(obj.GetType());
        }

        if (t == typeof(Dictionary<,>) || t == typeof(List<>))
        {
            param.UsingGlobalTypes = false;
        }

        // FEATURE : enable extensions when you can deserialize anon types
        if (param.EnableAnonymousTypes)
        {
            param.UseExtensions = false;
            param.UsingGlobalTypes = false;
        }

        return new JsonSerializer(param).ConvertToJson(obj);
    }

    /// <summary>
    ///     Parse a json string and generate a Dictionary&lt;string,object&gt; or List&lt;object&gt; structure
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static object Parse(string json)
    {
        return new JsonParser(json).Decode();
    }
#if net4
        /// <summary>
        /// Create a .net4 dynamic object from the json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(string json)
        {
            return new DynamicJson(json);
        }
#endif
    /// <summary>
    ///     Create a typed generic object from the json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T ToObject<T>(string json)
    {
        return new Deserializer(Parameters).ToObject<T>(json);
    }

    /// <summary>
    ///     Create a typed generic object from the json with parameter override on this call
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static T ToObject<T>(string json, JsonParameters param)
    {
        return new Deserializer(param).ToObject<T>(json);
    }

    /// <summary>
    ///     Create an object from the json
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static object ToObject(string json)
    {
        return new Deserializer(Parameters).ToObject(json, null);
    }

    /// <summary>
    ///     Create an object from the json with parameter override on this call
    /// </summary>
    /// <param name="json"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static object ToObject(string json, JsonParameters param)
    {
        return new Deserializer(param).ToObject(json, null);
    }

    /// <summary>
    ///     Create an object of type from the json
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object ToObject(string json, Type type)
    {
        return new Deserializer(Parameters).ToObject(json, type);
    }

    /// <summary>
    ///     Fill a given object with the json represenation
    /// </summary>
    /// <param name="input"></param>
    /// <param name="json"></param>
    /// <returns></returns>
    public static object FillObject(object input, string json)
    {
        var ht = new JsonParser(json).Decode() as Dictionary<string, object>;

        if (ht == null)
        {
            return null;
        }

        return new Deserializer(Parameters).ParseDictionary(ht, null, input.GetType(), input);
    }

    /// <summary>
    ///     Deep copy an object i.e. clone to a new object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object DeepCopy(object obj)
    {
        return new Deserializer(Parameters).ToObject(ToJson(obj));
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCopy<T>(T obj)
    {
        return new Deserializer(Parameters).ToObject<T>(ToJson(obj));
    }

    /// <summary>
    ///     Create a human readable string from the json
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Beautify(string input)
    {
        return Formatter.PrettyPrint(input);
    }

    /// <summary>
    ///     Register custom type handlers for your own types not natively handled by fastJSON
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serializer"></param>
    /// <param name="deserializer"></param>
    public static void RegisterCustomType(Type type, Serialize serializer, Deserialize deserializer)
    {
        Reflection.Instance.RegisterCustomType(type, serializer, deserializer);
    }

    /// <summary>
    ///     Clear the internal reflection cache so you can start from new (you will loose performance)
    /// </summary>
    public static void ClearReflectionCache()
    {
        Reflection.Instance.ClearReflectionCache();
    }

    internal static long CreateLong(out long num, string s, int index, int count)
    {
        num = 0;
        var neg = false;

        for (var x = 0; x < count; x++, index++)
        {
            char cc = s[index];

            if (cc == '-')
            {
                neg = true;
            }
            else if (cc == '+')
            {
                neg = false;
            }
            else
            {
                num *= 10;
                num += cc - '0';
            }
        }

        if (neg)
        {
            num = -num;
        }

        return num;
    }
}

internal class Deserializer
{
    private readonly Dictionary<object, int> _circobj = new Dictionary<object, int>();
    private readonly Dictionary<int, object> _cirrev = new Dictionary<int, object>();
    private readonly JsonParameters _params;
    private bool _usingglobals;

    public Deserializer(JsonParameters param)
    {
        _params = param;
    }

    private object RootHashTable(List<object> o)
    {
        var h = new Hashtable();

        foreach (Dictionary<string, object> values in o)
        {
            object key = values["k"];
            object val = values["v"];

            if (key is Dictionary<string, object>)
            {
                key = ParseDictionary((Dictionary<string, object>)key, null, typeof(object), null);
            }

            if (val is Dictionary<string, object>)
            {
                val = ParseDictionary((Dictionary<string, object>)val, null, typeof(object), null);
            }

            h.Add(key, val);
        }

        return h;
    }

    private object ChangeType(object value, Type conversionType)
    {
        if (conversionType == typeof(int))
        {
            return (int)(long)value;
        }

        if (conversionType == typeof(long))
        {
            return (long)value;
        }

        if (conversionType == typeof(string))
        {
            return (string)value;
        }

        if (conversionType.IsEnum)
        {
            return CreateEnum(conversionType, value);
        }

        if (conversionType == typeof(DateTime))
        {
            return CreateDateTime((string)value);
        }

        if (Reflection.Instance.IsTypeRegistered(conversionType))
        {
            return Reflection.Instance.CreateCustom((string)value, conversionType);
        }

        // 8-30-2014 - James Brooks - Added code for nullable types.
        if (IsNullable(conversionType))
        {
            if (value == null)
            {
                return value;
            }

            conversionType = UnderlyingTypeOf(conversionType);
        }

        // 8-30-2014 - James Brooks - Nullable Guid is a special case so it was moved after the "IsNullable" check.
        if (conversionType == typeof(Guid))
        {
            return CreateGuid((string)value);
        }

        return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
    }

    private bool IsNullable(Type t)
    {
        if (!t.IsGenericType)
        {
            return false;
        }

        Type g = t.GetGenericTypeDefinition();

        return g.Equals(typeof(Nullable<>));
    }

    private Type UnderlyingTypeOf(Type t)
    {
        return t.GetGenericArguments()[0];
    }

    private object RootList(object parse, Type type)
    {
        Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
        var o = (IList)Reflection.Instance.FastCreateInstance(type);

        foreach (object k in (IList)parse)
        {
            _usingglobals = false;
            object v = k;

            if (k is Dictionary<string, object>)
            {
                v = ParseDictionary(k as Dictionary<string, object>, null, gtypes[0], null);
            }
            else
            {
                v = ChangeType(k, gtypes[0]);
            }

            o.Add(v);
        }

        return o;
    }

    private object RootDictionary(object parse, Type type)
    {
        if (parse is Dictionary<string, object> && type == typeof(Dictionary<string, object>))
        {
            return (Dictionary<string, object>)parse;
        }

        Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
        Type t1 = null;
        Type t2 = null;

        if (gtypes != null)
        {
            t1 = gtypes[0];
            t2 = gtypes[1];
        }

        if (parse is Dictionary<string, object>)
        {
            var o = (IDictionary)Reflection.Instance.FastCreateInstance(type);

            foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)parse)
            {
                object v;
                object k = ChangeType(kv.Key, t1);

                if (kv.Value is Dictionary<string, object>)
                {
                    v = ParseDictionary(kv.Value as Dictionary<string, object>, null, t2, null);
                }

                else if (t2.IsArray)
                {
                    v = CreateArray((List<object>)kv.Value, t2, t2.GetElementType(), null);
                }

                else if (kv.Value is IList)
                {
                    v = CreateGenericList((List<object>)kv.Value, t2, t1, null);
                }

                else
                {
                    v = ChangeType(kv.Value, t2);
                }

                o.Add(k, v);
            }

            return o;
        }

        if (parse is List<object>)
        {
            return CreateDictionary(parse as List<object>, type, gtypes, null);
        }

        return null;
    }

    private StringDictionary CreateSd(Dictionary<string, object> d)
    {
        var nv = new StringDictionary();

        foreach (KeyValuePair<string, object> o in d)
        {
            nv.Add(o.Key, (string)o.Value);
        }

        return nv;
    }

    private NameValueCollection CreateNv(Dictionary<string, object> d)
    {
        var nv = new NameValueCollection();

        foreach (KeyValuePair<string, object> o in d)
        {
            nv.Add(o.Key, (string)o.Value);
        }

        return nv;
    }

    private void ProcessMap(object obj, Dictionary<string, MyPropInfo> props, Dictionary<string, object> dic)
    {
        foreach (KeyValuePair<string, object> kv in dic)
        {
            MyPropInfo p = props[kv.Key];
            object o = p.Getter(obj);
            var t = Type.GetType((string)kv.Value);

            if (t == typeof(Guid))
            {
                p.Setter(obj, CreateGuid((string)o));
            }
        }
    }

    private int CreateInteger(string s, int index, int count)
    {
        var num = 0;
        var neg = false;

        for (var x = 0; x < count; x++, index++)
        {
            char cc = s[index];

            if (cc == '-')
            {
                neg = true;
            }
            else if (cc == '+')
            {
                neg = false;
            }
            else
            {
                num *= 10;
                num += cc - '0';
            }
        }

        if (neg)
        {
            num = -num;
        }

        return num;
    }

    private object CreateEnum(Type pt, object v)
    {
        // TODO : optimize create enum
#if !SILVERLIGHT
        return Enum.Parse(pt, v.ToString());
#else
            return Enum.Parse(pt, v, true);
#endif
    }

    private Guid CreateGuid(string s)
    {
        if (s.Length > 30)
        {
            return new Guid(s);
        }

        return new Guid(Convert.FromBase64String(s));
    }

    private DateTime CreateDateTime(string value)
    {
        var utc = false;
        //                   0123456789012345678 9012 9/3
        // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  Z
        int year;
        int month;
        int day;
        int hour;
        int min;
        int sec;
        var ms = 0;

        year = CreateInteger(value, 0, 4);
        month = CreateInteger(value, 5, 2);
        day = CreateInteger(value, 8, 2);
        hour = CreateInteger(value, 11, 2);
        min = CreateInteger(value, 14, 2);
        sec = CreateInteger(value, 17, 2);

        if (value.Length > 21 && value[19] == '.')
        {
            ms = CreateInteger(value, 20, 3);
        }

        if (value[value.Length - 1] == 'Z')
        {
            utc = true;
        }

        if (_params.UseUtcDateTime == false && utc == false)
        {
            return new DateTime(
                year,
                month,
                day,
                hour,
                min,
                sec,
                ms);
        }

        return new DateTime(
            year,
            month,
            day,
            hour,
            min,
            sec,
            ms,
            DateTimeKind.Utc).ToLocalTime();
    }

    private object CreateArray(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
    {
        var col = Array.CreateInstance(bt, data.Count);

        // create an array of objects
        for (var i = 0; i < data.Count; i++)
        {
            object ob = data[i];

            if (ob == null)
            {
                continue;
            }

            if (ob is IDictionary)
            {
                col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null), i);
            }
            else if (ob is ICollection)
            {
                col.SetValue(CreateArray((List<object>)ob, bt, bt.GetElementType(), globalTypes), i);
            }
            else
            {
                col.SetValue(ChangeType(ob, bt), i);
            }
        }

        return col;
    }

    private object CreateGenericList(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
    {
        object foo = Reflection.Instance.FastCreateInstance(pt);
        var col = (IList)foo;

        //IList col = new List<object>();

        // create an array of objects
        foreach (object ob in data)
        {
            if (ob is IDictionary)
            {
                col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null));
            }

            else if (ob is List<object>)
            {
                if (bt.IsGenericType)
                {
                    col.Add((List<object>)ob); //).ToArray());
                }
                else
                {
                    col.Add(((List<object>)ob).ToArray());
                }
            }
            else
            {
                col.Add(ChangeType(ob, bt));
            }
        }

        return col;
    }

    private object CreateStringKeyDictionary(
        Dictionary<string, object> reader,
        Type pt,
        Type[] types,
        Dictionary<string, object> globalTypes)
    {
        var col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
        Type t1 = null;
        Type t2 = null;

        if (types != null)
        {
            t1 = types[0];
            t2 = types[1];
        }

        foreach (KeyValuePair<string, object> values in reader)
        {
            string key = values.Key;
            object val = null;

            if (values.Value is Dictionary<string, object>)
            {
                val = ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2, null);
            }

            else if (types != null && t2.IsArray)
            {
                if (values.Value is Array)
                {
                    val = values.Value;
                }
                else
                {
                    val = CreateArray((List<object>)values.Value, t2, t2.GetElementType(), globalTypes);
                }
            }
            else if (values.Value is IList)
            {
                val = CreateGenericList((List<object>)values.Value, t2, t1, globalTypes);
            }

            else
            {
                val = ChangeType(values.Value, t2);
            }

            col.Add(key, val);
        }

        return col;
    }

    private object CreateDictionary(
        List<object> reader,
        Type pt,
        Type[] types,
        Dictionary<string, object> globalTypes)
    {
        var col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
        Type t1 = null;
        Type t2 = null;

        if (types != null)
        {
            t1 = types[0];
            t2 = types[1];
        }

        foreach (Dictionary<string, object> values in reader)
        {
            object key = values["k"];
            object val = values["v"];

            if (key is Dictionary<string, object>)
            {
                key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null);
            }
            else
            {
                key = ChangeType(key, t1);
            }

            if (val is Dictionary<string, object>)
            {
                val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null);
            }
            else
            {
                val = ChangeType(val, t2);
            }

            col.Add(key, val);
        }

        return col;
    }

    internal object ParseDictionary(
        Dictionary<string, object> d,
        Dictionary<string, object> globaltypes,
        Type type,
        object input)
    {
        if (type == typeof(Dictionary<string, object>))
        {
            return d;
        }

        object tn = "";

        if (type == typeof(NameValueCollection))
        {
            return CreateNv(d);
        }

        if (type == typeof(StringDictionary))
        {
            return CreateSd(d);
        }

        if (d.TryGetValue("$i", out tn))
        {
            object v = null;
            _cirrev.TryGetValue((int)(long)tn, out v);

            return v;
        }

        if (d.TryGetValue("$types", out tn))
        {
            _usingglobals = true;
            globaltypes = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)tn)
            {
                globaltypes.Add((string)kv.Value, kv.Key);
            }
        }

        bool found = d.TryGetValue("$type", out tn);
#if !SILVERLIGHT
        if (found == false && type == typeof(object))
        {
            return d; // CreateDataset(d, globaltypes);
        }
#endif
        if (found)
        {
            if (_usingglobals)
            {
                object tname = "";

                if (globaltypes != null && globaltypes.TryGetValue((string)tn, out tname))
                {
                    tn = tname;
                }
            }

            type = Reflection.Instance.GetTypeFromCache((string)tn);
        }

        if (type == null)
        {
            throw new Exception("Cannot determine type");
        }

        string typename = type.FullName;
        object o = input;

        if (o == null)
        {
            if (_params.ParametricConstructorOverride)
            {
                o = FormatterServices.GetUninitializedObject(type);
            }
            else
            {
                o = Reflection.Instance.FastCreateInstance(type);
            }
        }

        var circount = 0;

        if (_circobj.TryGetValue(o, out circount) == false)
        {
            circount = _circobj.Count + 1;
            _circobj.Add(o, circount);
            _cirrev.Add(circount, o);
        }

        Dictionary<string, MyPropInfo> props = Reflection.Instance.Getproperties(
            type,
            typename,
            Reflection.Instance.IsTypeRegistered(type));

        foreach (KeyValuePair<string, object> kv in d)
        {
            string n = kv.Key;
            object v = kv.Value;
            string name = n.ToLower();

            if (name == "$map")
            {
                ProcessMap(o, props, (Dictionary<string, object>)d[name]);

                continue;
            }

            MyPropInfo pi;

            // object doesn't contain property of specified name
            if (props.TryGetValue(name, out pi) == false)
            {
                // we need to look if it has alias field
                PropertyInfo aliasFieldProperty = type
                    .GetProperties()
                    .FirstOrDefault(
                        property =>
                            Attribute.IsDefined(property, typeof(AliasField))
                            && property.GetCustomAttribute<AliasField>().Alias == name);

                if (aliasFieldProperty == null)
                {
                    continue;
                }

                // alias field is present - set it up so it can be processed
                pi = Reflection.Instance.CreateMyProp(
                    aliasFieldProperty.PropertyType,
                    aliasFieldProperty.Name,
                    false);

                pi.Setter = Reflection.CreateSetMethod(aliasFieldProperty.PropertyType, aliasFieldProperty);

                if (pi.Setter != null)
                {
                    pi.CanWrite = true;
                }

                pi.Getter = Reflection.CreateGetMethod(aliasFieldProperty.PropertyType, aliasFieldProperty);
            }

            if (pi.CanWrite)
            {
                //object v = d[n];

                if (v != null)
                {
                    object oset = null;

                    switch (pi.Type)
                    {
                        case MyPropInfoType.Int:
                            oset = (int)(long)v;

                            break;
                        case MyPropInfoType.Long:
                            oset = (long)v;

                            break;
                        case MyPropInfoType.String:
                            oset = (string)v;

                            break;
                        case MyPropInfoType.Bool:
                            oset = (bool)v;

                            break;
                        case MyPropInfoType.DateTime:
                            oset = CreateDateTime((string)v);

                            break;
                        case MyPropInfoType.Enum:
                            oset = CreateEnum(pi.Pt, v);

                            break;
                        case MyPropInfoType.Guid:
                            oset = CreateGuid((string)v);

                            break;

                        case MyPropInfoType.Array:
                            if (!pi.IsValueType)
                            {
                                oset = CreateArray((List<object>)v, pi.Pt, pi.Bt, globaltypes);
                            }

                            // what about 'else'?
                            break;
                        case MyPropInfoType.ByteArray:
                            oset = Convert.FromBase64String((string)v);

                            break;
#if !SILVERLIGHT
                        case MyPropInfoType.DataSet:
                            oset = CreateDataset((Dictionary<string, object>)v, globaltypes);

                            break;
                        case MyPropInfoType.DataTable:
                            oset = CreateDataTable((Dictionary<string, object>)v, globaltypes);

                            break;
                        case MyPropInfoType.SortedList:
                            oset = CreateSortedList((List<object>)v, pi, globaltypes);

                            break;
                        case MyPropInfoType.Hashtable: // same case as Dictionary
#endif
                        case MyPropInfoType.Dictionary:
                            oset = CreateDictionary((List<object>)v, pi.Pt, pi.GenericTypes, globaltypes);

                            break;
                        case MyPropInfoType.StringKeyDictionary:
                            oset = CreateStringKeyDictionary(
                                (Dictionary<string, object>)v,
                                pi.Pt,
                                pi.GenericTypes,
                                globaltypes);

                            break;
                        case MyPropInfoType.NameValue:
                            oset = CreateNv((Dictionary<string, object>)v);

                            break;
                        case MyPropInfoType.StringDictionary:
                            oset = CreateSd((Dictionary<string, object>)v);

                            break;
                        case MyPropInfoType.Custom:
                            oset = Reflection.Instance.CreateCustom((string)v, pi.Pt);

                            break;
                        default:
                        {
                            if (pi.IsGenericType && pi.IsValueType == false && v is List<object>)
                            {
                                oset = CreateGenericList((List<object>)v, pi.Pt, pi.Bt, globaltypes);
                            }

                            else if ((pi.IsClass || pi.IsStruct) && v is Dictionary<string, object>)
                            {
                                oset = ParseDictionary(
                                    (Dictionary<string, object>)v,
                                    globaltypes,
                                    pi.Pt,
                                    pi.Getter(o));
                            }

                            else if (v is List<object>)
                            {
                                oset = CreateArray((List<object>)v, pi.Pt, typeof(object), globaltypes);
                            }

                            else if (pi.IsValueType)
                            {
                                oset = ChangeType(v, pi.ChangeType);
                            }

                            else
                            {
                                oset = v;
                            }
                        }

                            break;
                    }

                    o = pi.Setter(o, oset);
                }
            }
        }

        return o;
    }

    public T ToObject<T>(string json)
    {
        Type t = typeof(T);
        object o = ToObject(json, t);

        if (t.IsArray)
        {
            if ((o as ICollection).Count == 0) // edge case for "[]" -> T[]
            {
                Type tt = t.GetElementType();
                object oo = Array.CreateInstance(tt, 0);

                return (T)oo;
            }

            return (T)o;
        }

        return (T)o;
    }

    public object ToObject(string json)
    {
        return ToObject(json, null);
    }

    public object ToObject(string json, Type type)
    {
        //_params = Parameters;
        _params.FixValues();
        Type t = null;

        if (type != null && type.IsGenericType)
        {
            t = Reflection.Instance.GetGenericTypeDefinition(type);
        }

        if (t == typeof(Dictionary<,>) || t == typeof(List<>))
        {
            _params.UsingGlobalTypes = false;
        }

        _usingglobals = _params.UsingGlobalTypes;

        object o = new JsonParser(json).Decode();

        if (o == null)
        {
            return null;
        }
#if !SILVERLIGHT
        if (type != null && type == typeof(DataSet))
        {
            return CreateDataset(o as Dictionary<string, object>, null);
        }

        if (type != null && type == typeof(DataTable))
        {
            return CreateDataTable(o as Dictionary<string, object>, null);
        }
#endif
        if (o is IDictionary)
        {
            if (type != null && t == typeof(Dictionary<,>)) // deserialize a dictionary
            {
                return RootDictionary(o, type);
            }

            return ParseDictionary(o as Dictionary<string, object>, null, type, null);
        }

        if (o is List<object>)
        {
            if (type != null && t == typeof(Dictionary<,>)) // kv format
            {
                return RootDictionary(o, type);
            }

            if (type != null && t == typeof(List<>)) // deserialize to generic list
            {
                return RootList(o, type);
            }

            if (type == typeof(Hashtable))
            {
                return RootHashTable((List<object>)o);
            }

            return (o as List<object>).ToArray();
        }

        if (type != null && o.GetType() != type)
        {
            return ChangeType(o, type);
        }

        return o;
    }

#if !SILVERLIGHT
    private DataSet CreateDataset(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
    {
        var ds = new DataSet();
        ds.EnforceConstraints = false;
        ds.BeginInit();

        // read dataset schema here
        object schema = reader["$schema"];

        if (schema is string)
        {
            TextReader tr = new StringReader((string)schema);
            ds.ReadXmlSchema(tr);
        }
        else
        {
            var ms = (DataSetSchema)ParseDictionary(
                (Dictionary<string, object>)schema,
                globalTypes,
                typeof(DataSetSchema),
                null);

            ds.DataSetName = ms.Name;

            for (var i = 0; i < ms.Info.Count; i += 3)
            {
                if (ds.Tables.Contains(ms.Info[i]) == false)
                {
                    ds.Tables.Add(ms.Info[i]);
                }

                ds.Tables[ms.Info[i]].Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
            }
        }

        foreach (KeyValuePair<string, object> pair in reader)
        {
            if (pair.Key == "$type" || pair.Key == "$schema")
            {
                continue;
            }

            var rows = (List<object>)pair.Value;

            if (rows == null)
            {
                continue;
            }

            DataTable dt = ds.Tables[pair.Key];
            ReadDataTable(rows, dt);
        }

        ds.EndInit();

        return ds;
    }

    private void ReadDataTable(List<object> rows, DataTable dt)
    {
        dt.BeginInit();
        dt.BeginLoadData();
        var guidcols = new List<int>();
        var datecol = new List<int>();

        foreach (DataColumn c in dt.Columns)
        {
            if (c.DataType == typeof(Guid) || c.DataType == typeof(Guid?))
            {
                guidcols.Add(c.Ordinal);
            }

            if (_params.UseUtcDateTime && (c.DataType == typeof(DateTime) || c.DataType == typeof(DateTime?)))
            {
                datecol.Add(c.Ordinal);
            }
        }

        foreach (List<object> row in rows)
        {
            var v = new object[row.Count];
            row.CopyTo(v, 0);

            foreach (int i in guidcols)
            {
                var s = (string)v[i];

                if (s != null && s.Length < 36)
                {
                    v[i] = new Guid(Convert.FromBase64String(s));
                }
            }

            if (_params.UseUtcDateTime)
            {
                foreach (int i in datecol)
                {
                    var s = (string)v[i];

                    if (s != null)
                    {
                        v[i] = CreateDateTime(s);
                    }
                }
            }

            dt.Rows.Add(v);
        }

        dt.EndLoadData();
        dt.EndInit();
    }

    private DataTable CreateDataTable(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
    {
        var dt = new DataTable();

        // read dataset schema here
        object schema = reader["$schema"];

        if (schema is string)
        {
            TextReader tr = new StringReader((string)schema);
            dt.ReadXmlSchema(tr);
        }
        else
        {
            var ms = (DataSetSchema)ParseDictionary(
                (Dictionary<string, object>)schema,
                globalTypes,
                typeof(DataSetSchema),
                null);

            dt.TableName = ms.Info[0];

            for (var i = 0; i < ms.Info.Count; i += 3)
            {
                dt.Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
            }
        }

        foreach (KeyValuePair<string, object> pair in reader)
        {
            if (pair.Key == "$type" || pair.Key == "$schema")
            {
                continue;
            }

            var rows = (List<object>)pair.Value;

            if (rows == null)
            {
                continue;
            }

            if (!dt.TableName.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            ReadDataTable(rows, dt);
        }

        return dt;
    }

    private object CreateSortedList(List<object> reader, MyPropInfo pi, Dictionary<string, object> globalTypes)
    {
        Type pt = pi.Pt;

        var col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);

        Type[] types = pt.GetGenericArguments();
        Type t1 = null;
        Type t2 = null;

        if (types != null)
        {
            t1 = types[0];
            t2 = types[1];
        }

        foreach (Dictionary<string, object> values in reader)
        {
            object key = values["k"];
            object val = values["v"];

            if (key is Dictionary<string, object>)
            {
                key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null);
            }
            else
            {
                key = ChangeType(key, t1);
            }

            if (val is Dictionary<string, object>)
            {
                val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null);
            }
            else
            {
                val = ChangeType(val, t2);
            }

            col.Add(key, val);
        }

        return col;
    }
#endif
}
