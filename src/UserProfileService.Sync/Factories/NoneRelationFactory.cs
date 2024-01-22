﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Factories;

/// <summary>
///     The factory is doing none relation.
/// </summary>
public class NoneRelationFactory : IRelationFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NoneRelationFactory> _logger;

    /// <summary>
    ///     Creates an object of type <see cref="NoneRelationFactory" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider that is needed to create a relation handler.</param>
    /// <param name="logger">The logger for logging purposes.</param>
    public NoneRelationFactory(IServiceProvider serviceProvider, ILogger<NoneRelationFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public IRelationHandler CreateRelationHandler(string sourceSystem, string relationEntity)
    {
        var handler = _serviceProvider.GetService<IRelationHandler<NoneSyncModel>>();

        return handler;
    }
}
