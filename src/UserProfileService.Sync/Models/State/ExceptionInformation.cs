namespace UserProfileService.Sync.Models.State
{
    /// <summary>
    ///     Contains some information about happened errors during synchronization process.
    /// </summary>
    public class ExceptionInformation
    {
        /// <summary>
        ///     The step where the error happened
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        ///     The current system where the error happened
        /// </summary>
        public string System { get; set; }

        /// <summary>
        ///     The error happened during the synchronization for the given system in the given step
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
