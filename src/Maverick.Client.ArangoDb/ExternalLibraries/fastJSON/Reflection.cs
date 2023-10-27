using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
#if !SILVERLIGHT
#endif

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;

internal struct Getters
{
    public string Name;

    public string LcName;

    //public string OtherName;
    public Reflection.GenericGetter Getter;
}

internal enum MyPropInfoType
{
    Int,
    Long,
    String,
    Bool,
    DateTime,
    Enum,
    Guid,

    Array,
    ByteArray,
    Dictionary,
    StringKeyDictionary,
    NameValue,
    StringDictionary,
#if !SILVERLIGHT
    Hashtable,
    SortedList,
    DataSet,
    DataTable,
#endif
    Custom,
    Unknown
}

internal struct MyPropInfo
{
    public Type Pt;
    public Type Bt;
    public Type ChangeType;
    public Reflection.GenericSetter Setter;
    public Reflection.GenericGetter Getter;
    public Type[] GenericTypes;
    public string Name;
    public MyPropInfoType Type;
    public bool CanWrite;

    public bool IsClass;
    public bool IsValueType;
    public bool IsGenericType;
    public bool IsStruct;
}

internal sealed class Reflection
{
    internal SafeDictionary<Type, Deserialize> CustomDeserializer = new SafeDictionary<Type, Deserialize>();

    // JSON custom
    internal SafeDictionary<Type, Serialize> CustomSerializer = new SafeDictionary<Type, Serialize>();
    private SafeDictionary<Type, CreateObject> _constrcache = new SafeDictionary<Type, CreateObject>();
    private SafeDictionary<Type, Type> _genericTypeDef = new SafeDictionary<Type, Type>();

    private SafeDictionary<Type, Type[]> _genericTypes = new SafeDictionary<Type, Type[]>();
    private SafeDictionary<Type, Getters[]> _getterscache = new SafeDictionary<Type, Getters[]>();

    private SafeDictionary<string, Dictionary<string, MyPropInfo>> _propertycache =
        new SafeDictionary<string, Dictionary<string, MyPropInfo>>();
    // Sinlgeton pattern 4 from : http://csharpindepth.com/articles/general/singleton.aspx

    private SafeDictionary<Type, string> _tyname = new SafeDictionary<Type, string>();
    private SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>();

    internal delegate object GenericGetter(object obj);

    internal delegate object GenericSetter(object target, object value);

    private delegate object CreateObject();

    public static Reflection Instance { get; } = new Reflection();

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static Reflection()
    {
    }

    private Reflection()
    {
    }

    private Type GetChangeType(Type conversionType)
    {
        if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            return Instance.GetGenericArguments(conversionType)[0]; // conversionType.GetGenericArguments()[0];
        }

        return conversionType;
    }

    internal void ResetPropertyCache()
    {
        _propertycache = new SafeDictionary<string, Dictionary<string, MyPropInfo>>();
    }

    internal void ClearReflectionCache()
    {
        _tyname = new SafeDictionary<Type, string>();
        _typecache = new SafeDictionary<string, Type>();
        _constrcache = new SafeDictionary<Type, CreateObject>();
        _getterscache = new SafeDictionary<Type, Getters[]>();
        _propertycache = new SafeDictionary<string, Dictionary<string, MyPropInfo>>();
        _genericTypes = new SafeDictionary<Type, Type[]>();
        _genericTypeDef = new SafeDictionary<Type, Type>();
    }

    internal object CreateCustom(string v, Type type)
    {
        Deserialize d;
        CustomDeserializer.TryGetValue(type, out d);

        return d(v);
    }

    internal void RegisterCustomType(Type type, Serialize serializer, Deserialize deserializer)
    {
        if (type != null && serializer != null && deserializer != null)
        {
            CustomSerializer.Add(type, serializer);
            CustomDeserializer.Add(type, deserializer);
            // reset property cache
            Instance.ResetPropertyCache();
        }
    }

    internal bool IsTypeRegistered(Type t)
    {
        if (CustomSerializer.Count == 0)
        {
            return false;
        }

        Serialize s;

        return CustomSerializer.TryGetValue(t, out s);
    }

    internal string GetTypeAssemblyName(Type t)
    {
        var val = "";

        if (_tyname.TryGetValue(t, out val))
        {
            return val;
        }

        string s = t.AssemblyQualifiedName;
        _tyname.Add(t, s);

        return s;
    }

    internal Type GetTypeFromCache(string typename)
    {
        Type val = null;

        if (_typecache.TryGetValue(typename, out val))
        {
            return val;
        }

        var t = Type.GetType(typename);
        //if (t == null) // RaptorDB : loading runtime assemblies
        //{
        //    t = Type.GetType(typename, (name) => {
        //        return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
        //    }, null, true);
        //}
        _typecache.Add(typename, t);

        return t;
    }

    internal object FastCreateInstance(Type objtype)
    {
        try
        {
            CreateObject c = null;

            if (_constrcache.TryGetValue(objtype, out c))
            {
                return c();
            }

            if (objtype.IsClass)
            {
                var dynMethod = new DynamicMethod("_", objtype, null);
                ILGenerator ilGen = dynMethod.GetILGenerator();
                ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                ilGen.Emit(OpCodes.Ret);
                c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                _constrcache.Add(objtype, c);
            }
            else // structs
            {
                var dynMethod = new DynamicMethod("_", typeof(object), null);
                ILGenerator ilGen = dynMethod.GetILGenerator();
                LocalBuilder lv = ilGen.DeclareLocal(objtype);
                ilGen.Emit(OpCodes.Ldloca_S, lv);
                ilGen.Emit(OpCodes.Initobj, objtype);
                ilGen.Emit(OpCodes.Ldloc_0);
                ilGen.Emit(OpCodes.Box, objtype);
                ilGen.Emit(OpCodes.Ret);
                c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                _constrcache.Add(objtype, c);
            }

            return c();
        }
        catch (Exception exc)
        {
            throw new Exception(
                string.Format(
                    "Failed to fast create instance for type '{0}' from assembly '{1}'",
                    objtype.FullName,
                    objtype.AssemblyQualifiedName),
                exc);
        }
    }

    internal static GenericSetter CreateSetField(Type type, FieldInfo fieldInfo)
    {
        var arguments = new Type[2];
        arguments[0] = arguments[1] = typeof(object);

        var dynamicSet = new DynamicMethod("_", typeof(object), arguments, type);

        ILGenerator il = dynamicSet.GetILGenerator();

        if (!type.IsClass) // structs
        {
            LocalBuilder lv = il.DeclareLocal(type);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, type);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloca_S, lv);
            il.Emit(OpCodes.Ldarg_1);

            if (fieldInfo.FieldType.IsClass)
            {
                il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            }

            il.Emit(OpCodes.Stfld, fieldInfo);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Box, type);
            il.Emit(OpCodes.Ret);
        }
        else
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            if (fieldInfo.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            }

            il.Emit(OpCodes.Stfld, fieldInfo);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
        }

        return (GenericSetter)dynamicSet.CreateDelegate(typeof(GenericSetter));
    }

    internal static GenericSetter CreateSetMethod(Type type, PropertyInfo propertyInfo)
    {
        MethodInfo setMethod = propertyInfo.GetSetMethod();

        if (setMethod == null)
        {
            return null;
        }

        var arguments = new Type[2];
        arguments[0] = arguments[1] = typeof(object);

        var setter = new DynamicMethod("_", typeof(object), arguments);
        ILGenerator il = setter.GetILGenerator();

        if (!type.IsClass) // structs
        {
            LocalBuilder lv = il.DeclareLocal(type);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, type);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloca_S, lv);
            il.Emit(OpCodes.Ldarg_1);

            if (propertyInfo.PropertyType.IsClass)
            {
                il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            }
            else
            {
                il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            }

            il.EmitCall(OpCodes.Call, setMethod, null);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Box, type);
        }
        else
        {
            if (!setMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                il.Emit(OpCodes.Ldarg_1);

                if (propertyInfo.PropertyType.IsClass)
                {
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                }

                il.EmitCall(OpCodes.Callvirt, setMethod, null);
                il.Emit(OpCodes.Ldarg_0);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);

                if (propertyInfo.PropertyType.IsClass)
                {
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                }

                il.Emit(OpCodes.Call, setMethod);
            }
        }

        il.Emit(OpCodes.Ret);

        return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
    }

    internal static GenericGetter CreateGetField(Type type, FieldInfo fieldInfo)
    {
        var dynamicGet = new DynamicMethod("_", typeof(object), new[] { typeof(object) }, type);

        ILGenerator il = dynamicGet.GetILGenerator();

        if (!type.IsClass) // structs
        {
            LocalBuilder lv = il.DeclareLocal(type);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, type);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloca_S, lv);
            il.Emit(OpCodes.Ldfld, fieldInfo);

            if (fieldInfo.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
        }
        else
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldInfo);

            if (fieldInfo.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
        }

        il.Emit(OpCodes.Ret);

        return (GenericGetter)dynamicGet.CreateDelegate(typeof(GenericGetter));
    }

    internal static GenericGetter CreateGetMethod(Type type, PropertyInfo propertyInfo)
    {
        MethodInfo getMethod = propertyInfo.GetGetMethod();

        if (getMethod == null)
        {
            return null;
        }

        var getter = new DynamicMethod("_", typeof(object), new[] { typeof(object) }, type);

        ILGenerator il = getter.GetILGenerator();

        if (!type.IsClass) // structs
        {
            LocalBuilder lv = il.DeclareLocal(type);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, type);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloca_S, lv);
            il.EmitCall(OpCodes.Call, getMethod, null);

            if (propertyInfo.PropertyType.IsValueType)
            {
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }
        }
        else
        {
            if (!getMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                il.EmitCall(OpCodes.Callvirt, getMethod, null);
            }
            else
            {
                il.Emit(OpCodes.Call, getMethod);
            }

            if (propertyInfo.PropertyType.IsValueType)
            {
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }
        }

        il.Emit(OpCodes.Ret);

        return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
    }

    internal Getters[] GetGetters(
        Type type,
        bool showReadOnlyProperties,
        List<Type> ignoreAttributes) //JSONParameters param)
    {
        Getters[] val = null;

        if (_getterscache.TryGetValue(type, out val))
        {
            return val;
        }

        PropertyInfo[] props =
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        var getters = new List<Getters>();

        foreach (PropertyInfo p in props)
        {
            if (p.GetIndexParameters().Length > 0)
            {
                // Property is an indexer
                continue;
            }

            if (!p.CanWrite && showReadOnlyProperties == false)
            {
                continue;
            }

            if (ignoreAttributes != null)
            {
                var found = false;

                foreach (Type ignoreAttr in ignoreAttributes)
                {
                    if (p.IsDefined(ignoreAttr, false))
                    {
                        found = true;

                        break;
                    }
                }

                if (found)
                {
                    continue;
                }
            }

            GenericGetter g = CreateGetMethod(type, p);

            if (g != null)
            {
                getters.Add(
                    new Getters
                    {
                        Getter = g,
                        Name = p.Name,
                        LcName = p.Name.ToLower()
                    });
            }
        }

        FieldInfo[] fi = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

        foreach (FieldInfo f in fi)
        {
            if (ignoreAttributes != null)
            {
                var found = false;

                foreach (Type ignoreAttr in ignoreAttributes)
                {
                    if (f.IsDefined(ignoreAttr, false))
                    {
                        found = true;

                        break;
                    }
                }

                if (found)
                {
                    continue;
                }
            }

            if (f.IsLiteral == false)
            {
                GenericGetter g = CreateGetField(type, f);

                if (g != null)
                {
                    getters.Add(
                        new Getters
                        {
                            Getter = g,
                            Name = f.Name,
                            LcName = f.Name.ToLower()
                        });
                }
            }
        }

        val = getters.ToArray();
        _getterscache.Add(type, val);

        return val;
    }

    public Type GetGenericTypeDefinition(Type t)
    {
        Type tt = null;

        if (_genericTypeDef.TryGetValue(t, out tt))
        {
            return tt;
        }

        tt = t.GetGenericTypeDefinition();
        _genericTypeDef.Add(t, tt);

        return tt;
    }

    public Type[] GetGenericArguments(Type t)
    {
        Type[] tt = null;

        if (_genericTypes.TryGetValue(t, out tt))
        {
            return tt;
        }

        tt = t.GetGenericArguments();
        _genericTypes.Add(t, tt);

        return tt;
    }

    public Dictionary<string, MyPropInfo> Getproperties(Type type, string typename, bool customType)
    {
        Dictionary<string, MyPropInfo> sd = null;

        if (_propertycache.TryGetValue(typename, out sd))
        {
            return sd;
        }

        sd = new Dictionary<string, MyPropInfo>();
        PropertyInfo[] pr = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        foreach (PropertyInfo p in pr)
        {
            if (p.GetIndexParameters().Length > 0)
            {
                // Property is an indexer
                continue;
            }

            MyPropInfo d = CreateMyProp(p.PropertyType, p.Name, customType);
            d.Setter = CreateSetMethod(type, p);

            if (d.Setter != null)
            {
                d.CanWrite = true;
            }

            d.Getter = CreateGetMethod(type, p);
            sd.Add(p.Name.ToLower(), d);
        }

        FieldInfo[] fi = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        foreach (FieldInfo f in fi)
        {
            MyPropInfo d = CreateMyProp(f.FieldType, f.Name, customType);

            if (f.IsLiteral == false)
            {
                d.Setter = CreateSetField(type, f);

                if (d.Setter != null)
                {
                    d.CanWrite = true;
                }

                d.Getter = CreateGetField(type, f);
                sd.Add(f.Name.ToLower(), d);
            }
        }

        _propertycache.Add(typename, sd);

        return sd;
    }

    public MyPropInfo CreateMyProp(Type t, string name, bool customType)
    {
        var d = new MyPropInfo();
        var dType = MyPropInfoType.Unknown;

        if (t == typeof(int) || t == typeof(int?))
        {
            dType = MyPropInfoType.Int;
        }
        else if (t == typeof(long) || t == typeof(long?))
        {
            dType = MyPropInfoType.Long;
        }
        else if (t == typeof(string))
        {
            dType = MyPropInfoType.String;
        }
        else if (t == typeof(bool) || t == typeof(bool?))
        {
            dType = MyPropInfoType.Bool;
        }
        else if (t == typeof(DateTime) || t == typeof(DateTime?))
        {
            dType = MyPropInfoType.DateTime;
        }
        else if (t.IsEnum)
        {
            dType = MyPropInfoType.Enum;
        }
        else if (t == typeof(Guid) || t == typeof(Guid?))
        {
            dType = MyPropInfoType.Guid;
        }
        else if (t == typeof(StringDictionary))
        {
            dType = MyPropInfoType.StringDictionary;
        }
        else if (t == typeof(NameValueCollection))
        {
            dType = MyPropInfoType.NameValue;
        }
        else if (t.IsArray)
        {
            d.Bt = t.GetElementType();

            if (t == typeof(byte[]))
            {
                dType = MyPropInfoType.ByteArray;
            }
            else
            {
                dType = MyPropInfoType.Array;
            }
        }
        else if (t.Name.Contains("Dictionary"))
        {
            d.GenericTypes = Instance.GetGenericArguments(t); // t.GetGenericArguments();

            if (d.GenericTypes.Length > 0 && d.GenericTypes[0] == typeof(string))
            {
                dType = MyPropInfoType.StringKeyDictionary;
            }
            else
            {
                dType = MyPropInfoType.Dictionary;
            }
        }
#if !SILVERLIGHT
        else if (t == typeof(Hashtable))
        {
            dType = MyPropInfoType.Hashtable;
        }
        else if (t.Name.Contains("SortedList"))
        {
            dType = MyPropInfoType.SortedList;
        }
        else if (t == typeof(DataSet))
        {
            dType = MyPropInfoType.DataSet;
        }
        else if (t == typeof(DataTable))
        {
            dType = MyPropInfoType.DataTable;
        }
#endif
        else if (customType)
        {
            dType = MyPropInfoType.Custom;
        }

        if (t.IsValueType && !t.IsPrimitive && !t.IsEnum && t != typeof(decimal))
        {
            d.IsStruct = true;
        }

        d.IsClass = t.IsClass;
        d.IsValueType = t.IsValueType;

        if (t.IsGenericType)
        {
            d.IsGenericType = true;
            d.Bt = t.GetGenericArguments()[0];
        }

        d.Pt = t;
        d.Name = name;
        d.ChangeType = GetChangeType(t);
        d.Type = dType;

        return d;
    }
}
