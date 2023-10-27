using System;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Defines a property, that can be searched.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SearchableAttribute : Attribute
    {
    }
}
