using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Common.Implementations;

/// <summary>
///     Describes a service that validates the given entities with information from the database in scope of volatile data
///     sets.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global --> Is registered as implementation in the program.
public class VolatileRepoValidationService : IVolatileRepoValidationService
{
    private readonly ILogger<VolatileRepoValidationService> _logger;
    private readonly IVolatileDataReadStore _volatileDataStore;

    /// <summary>
    ///     Initializes a new instance of <see cref="VolatileRepoValidationService" />.
    /// </summary>
    /// <param name="volatileDataStore">
    ///     Contains methods to retrieve data from volatile data sets.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public VolatileRepoValidationService(
        IVolatileDataReadStore volatileDataStore,
        ILogger<VolatileRepoValidationService> logger)
    {
        _volatileDataStore = volatileDataStore;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateUserSettingObjectExistsAsync(
        string? userId,
        string? sectionName,
        string? objectId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(userId, nameof(userId));
        Guard.IsNotNullOrEmpty(sectionName, nameof(sectionName));
        Guard.IsNotNullOrEmpty(objectId, nameof(objectId));

        bool exists = await _volatileDataStore.CheckUserSettingObjectExistsAsync(
            userId,
            sectionName,
            objectId,
            cancellationToken);

        if (exists)
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResult = new ValidationAttribute(
            objectId,
            $"User with id '{userId}' does not have any user settings object with id '{objectId}' inside section '{sectionName}'.");

        return new ValidationResult(validationResult);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateProfileExistsAsync(
        string? userId,
        string? member = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(userId, nameof(userId));

        bool exists = await _volatileDataStore.CheckUserExistsAsync(
            userId,
            cancellationToken);

        if (exists)
        {
            return _logger.ExitMethod(new ValidationResult());
        }

        var validationResult = new ValidationAttribute(
            nameof(userId),
            $"User with id '{userId}' does not exists.");

        return _logger.ExitMethod(new ValidationResult(validationResult));
    }
}
