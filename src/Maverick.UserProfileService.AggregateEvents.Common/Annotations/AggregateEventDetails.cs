using System;

namespace Maverick.UserProfileService.AggregateEvents.Common.Annotations
{
    /// <summary>
    ///     Extends an aggregated <see cref="IUserProfileServiceEvent" /> by additional details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AggregateEventDetails : Attribute
    {
        /// <summary>
        ///     If <c>true</c>, an aggregate <see cref="IUserProfileServiceEvent" /> is defined as resolved. Default: <c>false</c>
        /// </summary>
        public bool IsResolved { get; }

        /// <summary>
        ///     Version information of the related event. Default value: 1
        /// </summary>
        public long VersionInformation { get; } = 1L;

        /// <summary>
        ///     Initializes a new instance of <see cref="AggregateEventDetails" /> with a specified
        ///     <paramref name="versionInformation" /> and a <see cref="IsResolved" /> flag.
        /// </summary>
        /// <param name="versionInformation">Version information of the related event.</param>
        /// <param name="isResolved">If <c>true</c>, an aggregate <see cref="IUserProfileServiceEvent" /> is defined as resolved.</param>
        public AggregateEventDetails(long versionInformation, bool isResolved)
        {
            VersionInformation = versionInformation;
            IsResolved = isResolved;
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="AggregateEventDetails" />.
        /// </summary>
        public AggregateEventDetails()
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="AggregateEventDetails" /> with a specified <see cref="IsResolved" /> flag.
        /// </summary>
        /// <param name="isResolved">If <c>true</c>, an aggregate <see cref="IUserProfileServiceEvent" /> is defined as resolved.</param>
        public AggregateEventDetails(bool isResolved)
        {
            IsResolved = isResolved;
        }
    }
}
