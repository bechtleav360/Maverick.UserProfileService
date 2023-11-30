namespace UserProfileService.Messaging.DependencyInjection;

/// <summary>
///     Helper-Component that creates new instances of <see cref="ServiceMessagingMetadata" />.
/// </summary>
public static class MessageSourceBuilder
{
    /// <summary>
    ///     Create service-metadata for a standalone application.
    /// </summary>
    /// <param name="name">
    ///     technical name for the application ('bar' instead of 'Company.FooProviders.Bar')
    /// </param>
    /// <param name="group">
    ///     technical name for the application-group ('foo' or 'fooproviders' instead of 'Company.FooProviders.Bar')
    /// </param>
    /// <returns>new instance of <see cref="ServiceMessagingMetadata" /></returns>
    public static ServiceMessagingMetadata GroupedApp(string name, string group)
    {
        name = name ?? throw new ArgumentNullException(nameof(name));
        group = group ?? throw new ArgumentNullException(nameof(group));

        if (name == string.Empty)
        {
            throw new ArgumentException("name must not be empty", nameof(name));
        }

        if (group == string.Empty)
        {
            throw new ArgumentException("group must not be empty", nameof(group));
        }

        return new ServiceMessagingMetadata(
            name,
            group,
            new Uri($"maverick/{group}/{name}", UriKind.Relative));
    }

    /// <summary>
    ///     Create service-metadata for a standalone application.
    /// </summary>
    /// <param name="name">technical name for the application ('foo' instead of 'Company.Name.Space.Foo')</param>
    /// <returns>new instance of <see cref="ServiceMessagingMetadata" /></returns>
    public static ServiceMessagingMetadata StandaloneApp(string name)
    {
        name = name ?? throw new ArgumentNullException(nameof(name));

        if (name == string.Empty)
        {
            throw new ArgumentException("name must not be empty", nameof(name));
        }

        return new ServiceMessagingMetadata(
            name,
            string.Empty,
            new Uri($"{name}", UriKind.Relative));
    }
}
