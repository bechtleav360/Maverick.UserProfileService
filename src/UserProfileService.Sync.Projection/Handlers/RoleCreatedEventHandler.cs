﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

internal class RoleCreatedEventHandler : SyncBaseEventHandler<RoleCreated>
{
    private readonly IMapper _mapper;

    /// <summary>
    ///     Create a new instance of <see cref="RoleCreatedEventHandler" />
    /// </summary>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    /// <param name="profileService">An instance of <see cref="IProfileService" /> used to handle role operations.</param>
    /// <param name="mapper">A mapper used to convert objects</param>
    public RoleCreatedEventHandler(
        ILogger<RoleCreatedEventHandler> logger,
        IProfileService profileService,
        IMapper mapper) : base(logger, profileService)
    {
        _mapper = mapper;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        RoleCreated eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        Logger.LogInfoMessage(
            "Creating a new role with the id: {functionId} in the sync database",
            LogHelpers.Arguments(eventObject.Id));

        try
        {
            var createdRole = _mapper.Map<RoleSync>(eventObject);
            await ProfileService.CreateRoleAsync(createdRole, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by creating a new role with the id: {userId} in the sync database",
                LogHelpers.Arguments(eventObject.Id));

            throw;
        }
        finally
        {
            Logger.ExitMethod();
        }
    }
}
