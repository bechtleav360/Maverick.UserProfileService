using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Marten.Abstractions;
using UserProfileService.Adapter.Marten.EntityModels;
using UserProfileService.Adapter.Marten.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Adapter.Marten.Implementations;

/// <summary>
///     It represents the implementation of <see cref="IVolatileUserSettingsService" /> that uses Marten with PostgreSQL.
/// </summary>
internal class MartenVolatileUserSettingsService : IVolatileUserSettingsService
{
    private readonly ILogger<MartenVolatileUserSettingsService> _logger;
    private readonly IMapper _mapper;
    private readonly IOptionsValidateParser _optionsValidateParser;
    private readonly IUserStore _userStore;
    private readonly IVolatileUserSettingsStore _volatileUserSettingsStore;

    /// <summary>
    ///     Creates a <see cref="MartenVolatileUserSettingsStore" /> object.
    /// </summary>
    /// <param name="userStore">
    ///     The user store retrieve a user from a data store. It also can check
    ///     if a user exists.
    /// </param>
    /// <param name="volatileUserSettingsStore">
    ///     The volatile uer settings store contains volatile data
    ///     for a certain user.
    /// </param>
    /// <param name="logger">The logger that is used to create messages with different severities.</param>
    /// <param name="optionsValidateParser"> Validates an parses an query options to another internal query options object.</param>
    /// <param name="mapper">The mapper is used to map object.</param>
    public MartenVolatileUserSettingsService(
        IUserStore userStore,
        IVolatileUserSettingsStore volatileUserSettingsStore,
        ILogger<MartenVolatileUserSettingsService> logger,
        IOptionsValidateParser optionsValidateParser,
        IMapper mapper)
    {
        _userStore = userStore;
        _volatileUserSettingsStore = volatileUserSettingsStore;
        _logger = logger;
        _optionsValidateParser = optionsValidateParser;
        _mapper = mapper;
    }

    private async Task UserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        bool userExists = await _userStore.CheckUserExistsAsync(userId, cancellationToken);

        if (!userExists)
        {
            throw new InstanceNotFoundException($"The user with the userId {userId} could not be found.");
        }

        _logger.LogInfoMessage(
            "The user with the id {userId} could be found.",
            LogHelpers.Arguments(userId.ToLogString()));

        _logger.ExitMethod();
    }

    ///<inheritdoc />
    public async Task<IPaginatedList<UserSettingSection>> GetAllSettingsSectionsAsync(
        string userId,
        QueryOptions paginationAndSorting,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (paginationAndSorting == null)
        {
            throw new ArgumentNullException(nameof(paginationAndSorting));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        await UserExistsAsync(userId, cancellationToken);

        QueryOptionsVolatileModel queryOptions =
            _optionsValidateParser.ParseAndValidateQueryOptions<UserSettingSectionDbModel>(paginationAndSorting);

        List<UserSettingSectionDbModel> userSections =
            await _volatileUserSettingsStore.GetAllSettingSectionForUserAsync(
                userId,
                queryOptions,
                cancellationToken);

        var resultSections = _mapper.Map<List<UserSettingSection>>(userSections);

        return _logger.ExitMethod(resultSections.ToPaginatedList());
    }

    ///<inheritdoc />
    public async Task<IPaginatedList<UserSettingObject>> GetUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        QueryOptions paginationAndSorting,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (paginationAndSorting == null)
        {
            throw new ArgumentNullException(nameof(paginationAndSorting));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(userSettingsSectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingsSectionName));
        }

        await UserExistsAsync(userId, cancellationToken);

        QueryOptionsVolatileModel queryOptionsVolatile =
            _optionsValidateParser.ParseAndValidateQueryOptions<UserSettingObjectDbModel>(paginationAndSorting);

        List<UserSettingObjectDbModel> resultObject =
            await _volatileUserSettingsStore.GetUserSettingsObjectsForUserAsync(
                userId,
                userSettingsSectionName,
                queryOptionsVolatile,
                cancellationToken);

        var mappedResultObject = _mapper.Map<List<UserSettingObject>>(resultObject);

        return _logger.ExitMethod(mappedResultObject.ToPaginatedList());
    }

    ///<inheritdoc />
    public async Task<IPaginatedList<UserSettingObject>> GetAllUserSettingObjectForUserAsync(
        string userId,
        QueryOptions paginationAndSorting,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (paginationAndSorting == null)
        {
            throw new ArgumentNullException(nameof(paginationAndSorting));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        await UserExistsAsync(userId, cancellationToken);

        QueryOptionsVolatileModel queryOptionsVolatile =
            _optionsValidateParser.ParseAndValidateQueryOptions<UserSettingObject>(paginationAndSorting);

        List<UserSettingObjectDbModel> userSettingObjects =
            await _volatileUserSettingsStore.GetAllUserSettingsObjectForUserAsync(
                userId,
                queryOptionsVolatile,
                cancellationToken);

        var mappedUserSettingsObjects = _mapper.Map<List<UserSettingObject>>(userSettingObjects);

        return _logger.ExitMethod(mappedUserSettingsObjects.ToPaginatedList());
    }
}
