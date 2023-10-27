using System;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Defines the date time condition for object assignments.
    /// </summary>
    public class RangeCondition
    {
        /// <summary>
        ///     Time from which the assignment has expired.
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        ///     Time from which the assignment is valid.
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        ///     Create an instance of <see cref="RangeCondition" />.
        /// </summary>
        public RangeCondition()
        {
        }

        /// <summary>
        ///     Create an instance of <see cref="RangeCondition" />.
        /// </summary>
        /// <param name="start">Time from which the assignment is valid. </param>
        /// <param name="end">Time from which the assignment has expired.</param>
        public RangeCondition(DateTime? start, DateTime? end)
        {
            Start = start;
            End = end;
        }

        /// <inheritddoc />
        protected bool Equals(RangeCondition other)
        {
            return Nullable.Equals(Start, other.Start) && Nullable.Equals(End, other.End);
        }

        /// <inheritddoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RangeCondition)obj);
        }

        /// <inheritddoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }
    }
}
