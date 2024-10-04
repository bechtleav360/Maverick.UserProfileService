using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Utilities;

/// <summary>
///     An implementation of <see cref="IUserContextStore" /> reading from ASP.Net Cores authentication.
/// </summary>
public class UserContextStore : IUserContextStore
{
    private readonly HttpContext _context;
    private readonly ILogger<UserContextStore> _logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="UserContextStore" />.
    /// </summary>
    /// <param name="logger">The<see cref="ILogger{T}" /> which should be used for logging.</param>
    /// <param name="contextAccessor">Used to retrieve the <see cref="HttpContext" /> to gather auth information from.</param>
    public UserContextStore(
        ILogger<UserContextStore> logger,
        IHttpContextAccessor contextAccessor)
    {
        _logger = logger;
        _context = contextAccessor.HttpContext;
    }

    /// <inheritdoc cref="IUserContextStore.GetIdOfCurrentUser" />
    public string GetIdOfCurrentUser()
    {
        _logger.EnterMethod();

        string userId = _context.GetUserId(_logger);

        return _logger.ExitMethod<string>(userId);
    }
}
