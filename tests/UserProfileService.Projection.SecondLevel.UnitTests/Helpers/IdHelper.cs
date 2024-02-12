using System;
using System.Collections.Generic;
using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Helpers;

public static class IdHelper
{
    public static List<ExternalIdentifier> GenerateExternalIdList(
        string id = null,
        string source = null)
    {
        return
            string.IsNullOrEmpty(id)
                ? new List<ExternalIdentifier>()
                : new List<ExternalIdentifier>
                {
                    new ExternalIdentifier(id, source ?? "test")
                };
    }

    public static List<ExternalIdentifier> AddIdentifier(
        this List<ExternalIdentifier> externalIdentifiers,
        string id,
        string source)
    {
        if (externalIdentifiers == null)
        {
            return GenerateExternalIdList(id, source);
        }

        externalIdentifiers.Add(new ExternalIdentifier(id, source));

        return externalIdentifiers;
    }

    public static string GetId(object idObject)
    {
        PropertyInfo idProperty = idObject
            .GetType()
            .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);

        if (idProperty == null)
        {
            throw new Exception("Could not find an ID property in test model.");
        }

        return (string)idProperty.GetValue(idObject);
    }

    public static string GetRelatedProfileId(string profileIdWithEntityName)
    {
        if (string.IsNullOrWhiteSpace(profileIdWithEntityName))
        {
            throw new ArgumentNullException(nameof(profileIdWithEntityName));
        }

        try
        {
            return profileIdWithEntityName.Split('#')[1];
        }
        catch (Exception)
        {
            return null;
        }
    }
}