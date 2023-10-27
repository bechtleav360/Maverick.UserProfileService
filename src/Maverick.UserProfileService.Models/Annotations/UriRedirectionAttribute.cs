using System;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Attribute used to determine the target route for url properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UriRedirectionAttribute : Attribute
    {
        /// <summary>
        ///     Target url pattern
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UriRedirectionAttribute" /> class.
        /// </summary>
        /// <param name="pattern">Target url pattern</param>
        public UriRedirectionAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}
