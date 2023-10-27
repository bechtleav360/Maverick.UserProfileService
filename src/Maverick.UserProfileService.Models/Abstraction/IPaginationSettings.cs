namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     Defines an object that contains pagination settings like offset or limit.
    /// </summary>
    public interface IPaginationSettings
    {
        /// <summary>
        ///     The number of items to return.
        /// </summary>
        int Limit { set; get; }

        /// <summary>
        ///     The number of items to skip before starting to collect the result set.
        /// </summary>
        int Offset { set; get; }
    }
}
