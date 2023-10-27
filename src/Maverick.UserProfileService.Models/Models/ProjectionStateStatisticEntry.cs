namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Represents an entry of a projection state statistic.
    /// </summary>
    public class ProjectionStateStatisticEntry
    {
        /// <summary>
        ///     Average handler time (in ms)
        /// </summary>
        public double AverageTime { get; set; }

        /// <summary>
        ///     Amount of errors occurred till now.
        /// </summary>
        public int Errors { get; set; }

        /// <summary>
        ///     Amount of total events already processed by the related projection
        /// </summary>
        public long Events { get; set; }

        /// <summary>
        ///     The name of the projection
        /// </summary>
        public string Projection { get; set; }

        /// <summary>
        ///     The rate (events per minute)
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        ///     Total projection duration (in minutes)
        /// </summary>
        public long Time { get; set; }
    }
}
