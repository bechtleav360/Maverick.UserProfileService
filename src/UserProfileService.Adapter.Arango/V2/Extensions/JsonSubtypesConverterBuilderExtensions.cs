using System;
using JsonSubTypes;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class JsonSubtypesConverterBuilderExtensions
{
    internal static JsonSubtypesConverterBuilder RegisterSubtypeByTypeName<T>(this JsonSubtypesConverterBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.RegisterSubtype<T>(typeof(T).Name);
    }
}
