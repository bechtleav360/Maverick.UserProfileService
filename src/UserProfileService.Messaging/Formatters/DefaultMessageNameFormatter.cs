using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using MassTransit.Transports;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Messaging.Formatters;

/// <summary>
///     Implementation of <see cref="IMessageNameFormatter" />,
///     that doesn't include namespaces for types decorated with <see cref="MessageAttribute" />
/// </summary>
public class DefaultMessageNameFormatter : IMessageNameFormatter
{
    private readonly ConcurrentDictionary<Type, string> _cache;
    private readonly string _genericArgumentSeparator;
    private readonly string _genericTypeSeparator;
    private readonly string _namespaceSeparator;
    private readonly string _nestedTypeSeparator;

    /// <summary>
    ///     Create a new instance of <see cref="DefaultMessageNameFormatter" />
    /// </summary>
    /// <param name="genericArgumentSeparator">separator used for generic arguments</param>
    /// <param name="genericTypeSeparator">separator used for generic types</param>
    /// <param name="namespaceSeparator">separator used for namespaces</param>
    /// <param name="nestedTypeSeparator">separator used for nested types</param>
    public DefaultMessageNameFormatter(
        string genericArgumentSeparator,
        string genericTypeSeparator,
        string namespaceSeparator,
        string nestedTypeSeparator)
    {
        _genericArgumentSeparator = genericArgumentSeparator;
        _genericTypeSeparator = genericTypeSeparator;
        _namespaceSeparator = namespaceSeparator;
        _nestedTypeSeparator = nestedTypeSeparator;

        _cache = new ConcurrentDictionary<Type, string>();
    }
    
    private string CreateMessageName(Type type)
    {
        if (type.GetTypeInfo().IsGenericTypeDefinition)
        {
            throw new ArgumentException("An open generic type cannot be used as a message name");
        }

        var sb = new StringBuilder("");

        return GetMessageName(sb, type, null);
    }

    private string GetMessageName(StringBuilder sb, Type type, string? scope)
    {
        TypeInfo typeInfo = type.GetTypeInfo();

        if (typeInfo.IsGenericParameter)
        {
            return "";
        }

        if (typeInfo.Namespace != null)
        {
            string? ns = typeInfo.Namespace;

            // this line was changed from the original implementation,
            // to include an additional check for MessageAttribute
            if (!ns.Equals(scope) && !typeInfo.GetCustomAttributes<MessageAttribute>().Any())
            {
                sb.Append(ns);
                sb.Append(_namespaceSeparator);
            }
        }

        if (typeInfo.IsNested)
        {
            GetMessageName(sb, typeInfo.DeclaringType!, typeInfo.Namespace);
            sb.Append(_nestedTypeSeparator);
        }

        if (typeInfo.IsGenericType)
        {
            string name = typeInfo.GetGenericTypeDefinition().Name;

            //remove `1
            int index = name.IndexOf('`');

            if (index > 0)
            {
                name = name.Remove(index);
            }

            sb.Append(name);
            sb.Append(_genericTypeSeparator);

            Type[] arguments = typeInfo.GetGenericArguments();

            for (var i = 0; i < arguments.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(_genericArgumentSeparator);
                }

                GetMessageName(sb, arguments[i], typeInfo.Namespace);
            }

            sb.Append(_genericTypeSeparator);
        }
        else
        {
            sb.Append(typeInfo.Name);
        }

        return sb.ToString();
    }
    /// <inheritdoc />
    public string GetMessageName(Type type)
    {
        return new string(_cache.GetOrAdd(type, CreateMessageName));
    }
}
