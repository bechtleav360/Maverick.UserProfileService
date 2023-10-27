using System;
using System.Reflection;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class TypePropertyType
{
    internal Type MapperType { get; }
    internal PropertyInfo PropertyToBeMappedFrom { get; }
    internal PropertyInfo PropertyToBeMappedTo { get; }
    internal Type ResolvingType { get; }

    private TypePropertyType(
        Type resolvingType,
        PropertyInfo propertyToBeMappedFrom,
        Type mapperType,
        PropertyInfo propertyToBeMappedTo)
    {
        ResolvingType = resolvingType;
        PropertyToBeMappedFrom = propertyToBeMappedFrom;
        MapperType = mapperType;
        PropertyToBeMappedTo = propertyToBeMappedTo;
    }

    public static TypePropertyType Create(
        Type resolvingType,
        PropertyInfo propertyToBeMappedFrom,
        Type mapperType,
        PropertyInfo propertyToBeMappedTo)
    {
        return new TypePropertyType(resolvingType, propertyToBeMappedFrom, mapperType, propertyToBeMappedTo);
    }
}
