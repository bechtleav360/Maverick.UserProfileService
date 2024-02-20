namespace UserProfileService.Sync.Models.State
{
    /// <summary>
    ///     Contains some information about errors that happened during synchronization process.
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
        ///     The error that happened during the synchronization for the given system in the given step
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     The name of the type of the original occurred exception.
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        ///     Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        ///     Gets or sets a link to the help file associated with this exception.
        /// </summary>
        public string HelpLink { get; set; }

        /// <summary>
        ///     Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
        /// </summary>
        public int HResult { get; set; }
    }
}
