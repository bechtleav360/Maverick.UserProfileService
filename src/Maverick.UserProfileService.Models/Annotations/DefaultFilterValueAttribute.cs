using System;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Represents the default filter property that will be used if a filter values has been set,
    ///     but no filter property name has been provided.
    /// </summary>
    /// <remarks>
    ///     This setting is useful,if the result set shall be filtered by collection property like members.<br />
    ///     i.e. "select all groups whose members are 'foo'" - this is possible, if members have a default property
    ///     However, "select all group whose names of members are 'foo'" would be more precisely
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultFilterValueAttribute : Attribute
    {
    }
}
