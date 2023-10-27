using System.Text.Json.Nodes;
using System.Transactions;
using JasperFx.Core;
using Marten;
using Marten.Services;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using UserProfileService.Adapter.Marten.Abstractions;
using UserProfileService.Adapter.Marten.EntityModels;
using UserProfileService.Adapter.Marten.Extensions;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Adapter.Marten.Implementations;

/// <summary>
///     It represents the implementation of <see cref="IVolatileUserSettingsStore" /> that uses Marten with PostgreSQL.
/// </summary>
internal class MartenVolatileUserSettingsStore : IVolatileUserSettingsStore, IVolatileDataReadStore
{
    private readonly IDocumentSession _documentSession;
    private readonly IVolatileDataStore _documentStore;
    private readonly ILogger<MartenVolatileUserSettingsStore> _logger;

    /// <summary>
    ///     Creates a <see cref="MartenVolatileUserSettingsStore" /> object.
    /// </summary>
    /// <param name="documentStore">Interface for querying a document database and unit of work updates</param>
    /// <param name="logger">The logger that is used to create messages with different severities.</param>
    public MartenVolatileUserSettingsStore(
        IVolatileDataStore documentStore,
        ILogger<MartenVolatileUserSettingsStore> logger)
    {
        _documentSession = documentStore.LightweightSession();
        _documentStore = documentStore;
        _logger = logger;
    }

    /// <summary>
    ///     Create a user section in the volatile store.
    /// </summary>
    /// <param name="userSettingSectionName">The user settings section name that should be created in the volatile store.</param>
    /// <param name="transaction">The transaction inside the operation will be processed.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps  <see cref="UserSettingSection" /> that
    ///     represents the user settings section that was created.
    /// </returns>
    private async Task<UserSettingSectionDbModel> CreateUserSettingsSectionAsync(
        string userSettingSectionName,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userSettingSectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingSectionName));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "The method was called with the section name: {sectionName}",
                userSettingSectionName.AsArgumentList());
        }

        IDocumentSession documentSession = transaction != null
            ? _documentStore.LightweightSession(SessionOptions.ForTransaction(transaction))
            : _documentSession;

        UserSettingSectionDbModel? existingSection = await documentSession.Query<UserSettingSectionDbModel>()
            .FirstOrDefaultAsync(
                section => section.Name.EqualsIgnoreCase(userSettingSectionName),
                cancellationToken);

        if (existingSection != null)
        {
            _logger.LogDebugMessage(
                "User section {sectionName} already exists - skipping method",
                userSettingSectionName.AsArgumentList());

            return _logger.ExitMethod(existingSection);
        }

        var settingSection = new UserSettingSectionDbModel(userSettingSectionName);

        _logger.LogDebugMessage(
            "The settings section to save in the database: {settingsSection}.",
            settingSection.ToLogString().AsArgumentList());

        documentSession.Store(settingSection);

        _logger.LogInfoMessage(
            "a user setting section with the name {userSettingsSection} has been stored in the database.",
            userSettingSectionName.AsArgumentList());

        await documentSession.SaveChangesAsync(cancellationToken);

        return _logger.ExitMethod(settingSection);
    }

    private async Task EnsureEmptySectionsAreDeletedAsync(
        IDocumentSession session,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IAsyncEnumerable<UserSettingSectionDbModel> foundSectionsEnumerable = session
            .Query<UserSettingSectionDbModel>()
            .ToAsyncEnumerable(cancellationToken);

        // the "session" holds the first query of all sections and cannot be used until the 
        // "async foreach" has been finished
        // innerSession will handle the clean up while session will retrieve the next document
        await using IDocumentSession innerSession = _documentStore.LightweightSession();

        await foreach (UserSettingSectionDbModel? section in foundSectionsEnumerable)
        {
            if (string.IsNullOrEmpty(section.Id))
            {
                continue;
            }

            string sectionId = section.Id;

            bool sectionUsed = await innerSession.Query<UserSettingObjectDbModel>()
                .AnyAsync(o => o.UserSettingSection.Id == sectionId, cancellationToken);

            if (sectionUsed)
            {
                continue;
            }

            innerSession.Delete<UserSettingSectionDbModel>(sectionId);

            // delete the entry with no relations immediately
            await innerSession.SaveChangesAsync(cancellationToken);

            _logger.LogInfoMessage(
                "User settings section {sectionName} deleted, because it is not used anymore by any user.",
                section.Name.AsArgumentList());
        }

        _logger.LogInfoMessage(
            "Cleanup of user setting sections finished.",
            LogHelpers.Arguments());

        _logger.ExitMethod();
    }

    private async Task<TResult> ExecuteInsideTransactionAsync<TResult>(
        Func<IDocumentSession, NpgsqlTransaction, CancellationToken, Task<TResult>> method,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_documentSession.Connection == null)
        {
            throw new TransactionException("cannot start transaction, because a connection object is missing.");
        }

        await using NpgsqlTransaction transaction = await _documentSession
            .Connection
            .BeginTransactionAsync(cancellationToken);

        await using IDocumentSession documentSession =
            _documentStore.LightweightSession(SessionOptions.ForTransaction(transaction));

        try
        {
            TResult result = await method.Invoke(documentSession, transaction, cancellationToken);

            await transaction.CommitAsync(CancellationToken.None);

            return _logger.ExitMethod(result);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception inner)
            {
                _logger.LogInfoMessage(
                    "Error during rollback of transaction. Message: {errorMessage}",
                    LogHelpers.Arguments(inner.Message));
            }

            throw;
        }
    }

    ///<inheritdoc />
    public async Task<List<UserSettingSectionDbModel>> GetAllSettingSectionForUserAsync(
        string userId,
        QueryOptionsVolatileModel paginationQueryObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(null, nameof(userId));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "The method was called with userId: {userId} and paginationOption: {paginationObject}.",
                LogHelpers.Arguments(userId, paginationQueryObject.ToLogString()));
        }

        _logger.LogInfoMessage(
            "Trying to get all section names for the userId: {userId} from database",
            userId.AsArgumentList());

        IEnumerable<UserSettingSectionDbModel> sectionObjects = await _documentSession
            .Query<UserSettingObjectDbModel>()
            .Where(u => u.UserId == userId)
            .Select(p => p.UserSettingSection)
            .Distinct()
            .ApplyOptions(
                paginationQueryObject,
                cancellationToken);

        _logger.LogInfoMessage(
            "Found sections: {sections} for userId: {userId}",
            LogHelpers.Arguments(sectionObjects.ToLogString(), userId));

        _logger.LogDebugMessage(
            "Found {sectionsResult} for user with {userId}",
            LogHelpers.Arguments(sectionObjects.ToLogString(), userId));

        return _logger.ExitMethod(sectionObjects.ToList());
    }

    ///<inheritdoc />
    public async Task DeleteSettingsSectionForUserAsync(
        string userId,
        string userSettingSectionName,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(userSettingSectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingSectionName));
        }

        _logger.LogDebugMessage(
            "Trying to delete all setting objects of section {sectionName} related to user {userId}.",
            LogHelpers.Arguments(userSettingSectionName, userId));

        _documentSession
            .DeleteWhere<UserSettingObjectDbModel>(
                p => p.UserId == userId
                    && p.UserSettingSection.Name.EqualsIgnoreCase(userSettingSectionName));

        await _documentSession.SaveChangesAsync(cancellationToken);

        await EnsureEmptySectionsAreDeletedAsync(
            _documentSession,
            cancellationToken);

        _logger.LogInfoMessage(
            "Setting objects deleted (section {sectionName} of user {userId})",
            LogHelpers.Arguments(userSettingSectionName, userId));

        _logger.ExitMethod();
    }

    ///<inheritdoc />
    public async Task<IList<UserSettingObjectDbModel>> CreateUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        JsonArray userSettingsValues,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(userSettingsSectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingsSectionName));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "The method was called with userId: {userId}, userSettingsSectionName {userSettingsSectionName} and settingObject: {userSettingsValue}",
                LogHelpers.Arguments(
                    userId,
                    userSettingsSectionName,
                    userSettingsValues.ToLogString()));
        }

        List<UserSettingObjectDbModel> inserted = await ExecuteInsideTransactionAsync(
            async (documentSession, transaction, ct) =>
            {
                UserSettingSectionDbModel sectionExists =
                    await CreateUserSettingsSectionAsync(userSettingsSectionName, transaction, ct);

                var entriesToInsert = new List<UserSettingObjectDbModel>();

                foreach (JsonNode? node in userSettingsValues)
                {
                    if (node is not JsonObject jsonObject)
                    {
                        _logger.LogWarnMessage(
                            "Provided JSON array contains an invalid value type inside provided JSON array - it will be ignored - correlation id = {correlationId}",
                            LogHelpers.Arguments(
                                userId,
                                userSettingsSectionName));

                        continue;
                    }

                    entriesToInsert.Add(
                        new UserSettingObjectDbModel(
                            jsonObject,
                            sectionExists,
                            userId));
                }

                if (entriesToInsert.Count == 0)
                {
                    _logger.LogInfoMessage(
                        "No entries have to be inserted [user id = {userId}; section name = {sectionName}]",
                        LogHelpers.Arguments(userId, userSettingsSectionName));

                    return _logger.ExitMethod(entriesToInsert);
                }

                _logger.LogDebugMessage(
                    "Attempting to insert {entriesAmount} entries to the database [user id = {userId}; section name = {sectionName}]",
                    LogHelpers.Arguments(entriesToInsert.Count, userId, userSettingsSectionName));

                documentSession.Insert(entriesToInsert.ToArray());

                await documentSession.SaveChangesAsync(ct);

                return entriesToInsert;
            },
            cancellationToken);

        _logger.LogInfoMessage(
            "Amount of entries added to database [user id = {userId}; section name = {sectionName}]: {entriesAmount} elements",
            LogHelpers.Arguments(userId, userSettingsSectionName, inserted.Count));

        return _logger.ExitMethod(inserted);
    }

    ///<inheritdoc />
    public async Task DeleteUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingsId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(userSettingsSectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingsSectionName));
        }

        if (string.IsNullOrWhiteSpace(userSettingsId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingsId));
        }

        _logger.LogDebugMessage(
            "Trying to delete setting object {objectId} of section {sectionName} related to user {userId}.",
            LogHelpers.Arguments(userSettingsId, userSettingsSectionName, userId));

        _documentSession
            .DeleteWhere<UserSettingObjectDbModel>(
                p =>
                    p.UserId == userId
                    && p.UserSettingSection.Name.EqualsIgnoreCase(userSettingsSectionName)
                    && p.Id == userSettingsId);

        await _documentSession.SaveChangesAsync(cancellationToken);

        await EnsureEmptySectionsAreDeletedAsync(
            _documentSession,
            cancellationToken);

        _logger.LogInfoMessage(
            "Setting object {objectId} deleted (section {sectionName} of user {userId})",
            LogHelpers.Arguments(userSettingsId, userSettingsSectionName, userId));

        _logger.ExitMethod();
    }

    ///<inheritdoc />
    public async Task<UserSettingObjectDbModel> UpdateUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingsId,
        JsonObject userSettingsValue,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(userSettingsSectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingsSectionName));
        }

        if (string.IsNullOrWhiteSpace(userSettingsId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userSettingsId));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Method was called with userId: {userId}, settingSectionName: {userSettingsSectionName}, settingsId: {userSettingsId}",
                LogHelpers.Arguments(userId, userSettingsSectionName, userSettingsId));
        }

        UserSettingObjectDbModel? userSettingObjectOld =
            await _documentSession.Query<UserSettingObjectDbModel>()
                .FirstOrDefaultAsync(
                    p => p.Id.EqualsIgnoreCase(userSettingsId)
                        && p.UserSettingSection.Name.EqualsIgnoreCase(userSettingsSectionName)
                        && p.Id.EqualsIgnoreCase(userSettingsId),
                    cancellationToken);

        if (userSettingObjectOld == null)
        {
            throw new InstanceNotFoundException(
                $"The user settings object for the userId: {userId}, section name: {userSettingsSectionName} and settingsId: {userSettingsId} could not be found!");
        }

        _logger.LogInfoMessage(
            "The user object for the userId: {userId}, section name: {userSettingsSectionName} and settingsId: {userSettingsId} could be found.",
            LogHelpers
                .Arguments(userId, userSettingsSectionName, userSettingsId));

        var userSettingObjectNew = new UserSettingObjectDbModel(userSettingObjectOld)
        {
            UpdatedAt = DateTime.Now,
            UserSettingsObject = userSettingsValue
        };

        _logger.LogInfoMessage("Trying to update the settings object with a new value.", LogHelpers.Arguments());

        _logger.LogDebugMessage(
            "The new object that updated the old one: {userSettingObjectNew}",
            LogHelpers.Arguments(userSettingObjectNew.ToLogString()));

        _documentSession.Update(userSettingObjectNew);

        await _documentSession.SaveChangesAsync(cancellationToken);

        return _logger.ExitMethod(userSettingObjectNew);
    }

    ///<inheritdoc />
    public async Task<List<UserSettingObjectDbModel>> GetUserSettingsObjectsForUserAsync(
        string userId,
        string userSettingsSectionName,
        QueryOptionsVolatileModel paginationAndSorting,
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

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Method was called with userId: {userId} and settingSectionName: {userSettingsSectionName}.",
                LogHelpers.Arguments(userId, userSettingsSectionName));
        }

        List<UserSettingObjectDbModel> resultItems = (await _documentSession.Query<UserSettingObjectDbModel>()
                .Where(
                    p => p.UserId == userId
                        && p.UserSettingSection.Name == userSettingsSectionName)
                .ApplyOptions(paginationAndSorting, cancellationToken))
            .ToList();

        if (!resultItems.Any())
        {
            _logger.LogInfoMessage(
                "No settings object could be found for the userId: {userId} and the section name:{sectionName}",
                LogHelpers.Arguments(userId, userSettingsSectionName));
        }

        return _logger.ExitMethod(resultItems);
    }

    ///<inheritdoc />
    public async Task<List<UserSettingObjectDbModel>> GetAllUserSettingsObjectForUserAsync(
        string userId,
        QueryOptionsVolatileModel paginationAndSorting,
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

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Method was called with userId: {userId}.",
                LogHelpers.Arguments(userId));
        }

        List<UserSettingObjectDbModel> allUserObjects = (await _documentSession.Query<UserSettingObjectDbModel>()
                .Where(p => p.UserId == userId)
                .ApplyOptions(paginationAndSorting, cancellationToken))
            .ToList();

        if (!allUserObjects.Any())
        {
            _logger.LogInfoMessage(
                "There were not settings objects found for the user with the userId: {userId}",
                LogHelpers.Arguments(userId));
        }
        else
        {
            _logger.LogInfoMessage(
                "Found {contObject} {objects} for the userId {userId}.",
                LogHelpers.Arguments(
                    allUserObjects.Count,
                    allUserObjects.Count > 1 ? "user setting objects" : "user setting object",
                    userId));
        }

        return _logger.ExitMethod(allUserObjects);
    }

    /// <inheritdoc />
    public async Task<bool> CheckUserExistsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForDebug())
        {
            _logger.LogDebugMessage("Checking if user {userId} exists.", userId.AsArgumentList());
        }

        bool userExists = await _documentSession.Query<UserDbModel>()
            .AnyAsync(u => u.Id == userId, cancellationToken);

        return _logger.ExitMethod(userExists);
    }

    /// <inheritdoc />
    public async Task<bool> CheckUserSettingObjectExistsAsync(
        string userId,
        string sectionName,
        string objectId,
        CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForDebug())
        {
            _logger.LogDebugMessage(
                "Checking if user settings object {objectId} exists [user id = {userId}; section = {sectionName}]",
                LogHelpers.Arguments(userId, sectionName, objectId));
        }

        bool objectExists = await _documentSession.Query<UserSettingObjectDbModel>()
            .AnyAsync(
                o => o.Id == objectId
                    && o.UserId == userId
                    && o.UserSettingSection.Name.EqualsIgnoreCase(sectionName),
                cancellationToken);

        return _logger.ExitMethod(objectExists);
    }
}
