using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Implementations;
using UserProfileService.Common.V2.Utilities;

namespace UserProfileService.Extensions;

/// <summary>
///     Contains service collection extensions of various (miscellaneous) purposes.
/// </summary>
internal static class MiscServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the <see cref="DefaultCursorApiProvider" /> as <see cref="ICursorApiProvider" /> to existing
    ///     <paramref name="services" />.
    /// </summary>
    /// <remarks>
    ///     It will use an <see cref="EntityViewJsonSerializerSettingsProvider" /> as
    ///     <see cref="IJsonSerializerSettingsProvider" /> implementation.
    /// </remarks>
    /// <param name="services">The service collection to be modified.</param>
    internal static void AddDefaultCursorApiProvider(this IServiceCollection services)
    {
        services.AddScoped<ICursorApiProvider>(
            p => new DefaultCursorApiProvider(
                p.GetRequiredService<ITempStore>(),
                p.GetRequiredService<ILogger<DefaultCursorApiProvider>>(),
                new EntityViewJsonSerializerSettingsProvider()));
    }
}
