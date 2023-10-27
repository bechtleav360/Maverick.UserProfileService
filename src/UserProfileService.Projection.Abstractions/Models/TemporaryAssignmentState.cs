namespace UserProfileService.Projection.Abstractions.Models;
//   active: start less equal than now() AND end equals max() or null
//   inactive: end less than now()
//   notProcessed: start greater than now()
//   activeWithExpiration. start less equal than now() AND end greater now() BUT less than max()
//
//    [---inactive---]
//                              [-----------active--------------]
//            [---inactive----]
//                          [---active w/ expiration---]
//                                       [--not processed--]
//                          
// MIN/NULL <------ PAST ------>| NOW |<----- FUTURE --------> MAX/NULL

/// <summary>
///     The possible state values of stored temporary assignments.
/// </summary>
public enum TemporaryAssignmentState
{
    /// <summary>
    ///     Indicates that the item has not been processed yet.
    /// </summary>
    NotProcessed = 0,

    /// <summary>
    ///     Represents an inactive temporary assignment.
    /// </summary>
    Inactive = 1,

    /// <summary>
    ///     Represents an active temporary assignment that won't be inactive any more.
    /// </summary>
    Active = 2,

    /// <summary>
    ///     Represents a temporary assignment, that is currently active but will be inactive in the future.
    /// </summary>
    ActiveWithExpiration = 3,

    /// <summary>
    ///     Represents a temporary assignment, that is not valid because an error occurred. Further information can be found
    ///     in <see cref="FirstLevelProjectionTemporaryAssignment.LastErrorMessage" />.
    /// </summary>
    ErrorOccurred = 4
}
