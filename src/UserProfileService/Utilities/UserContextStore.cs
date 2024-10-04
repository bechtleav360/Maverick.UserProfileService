using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Utilities;

/// <summary>
///     An implementation of <see cref="IUserContextStore" /> reading from ASP.Net Cores authentication.
/// </summary>
public class UserContextStore : IUserContextStore
{
    private readonly HttpContext _context;
    private readonly ILogger<UserContextStore> _logger;
    private readonly Lazy<Task<string>> _userId;

    /// <summary>
    ///     Initializes a new instance of <see cref="UserContextStore" />.
    /// </summary>
    /// <param name="logger">The<see cref="ILogger{T}" /> which should be used for logging.</param>
    /// <param name="contextAccessor">Used to retrieve the <see cref="HttpContext" /> to gather auth information from.</param>
    /// <param name="readService">A <see cref="IReadService" /> which will be used in order to gather the ups-internal id.</param>
    public UserContextStore(
        ILogger<UserContextStore> logger,
        IHttpContextAccessor contextAccessor,
        IReadService readService)
    {
        _logger = logger;
        _context = contextAccessor.HttpContext;
        _userId = new Lazy<Task<string>>(GetUserIdInternal);
    }

    private async Task<string> GetUserIdInternal()
    {
        _logger.EnterMethod();

        string userId = _context.GetUserId(_logger);
            
        return _logger.ExitMethod<string>(userId);
    }
    

    /// <inheritdoc cref="IUserContextStore.GetIdOfCurrentUserAsync" />
    public Task<string> GetIdOfCurrentUserAsync()
    {
        return _userId.Value;
    }
}
