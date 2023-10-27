using Marten;
using NpgsqlTypes;
using UserProfileService.Adapter.Marten.EntityModels;
using Weasel.Postgresql;

namespace UserProfileService.Adapter.Marten.DbBuilders;

/// <summary>
///     The registry defines the database model for the
///     the user settings section and the user settings object.
/// </summary>
public class UserSettingsRegistry : MartenRegistry
{
    /// <summary>
    ///     Creates an object of type <see cref="UserSettingsRegistry" />.
    /// </summary>
    public UserSettingsRegistry()
    {
        For<UserSettingObjectDbModel>()
            // The name of the table
            .DocumentAlias("UserSettingObjects")
            // Duplicate creates an column in the table.
            // The column name is the same like in the model
            .Duplicate(p => p.UserId, dbType: NpgsqlDbType.Varchar)
            .Duplicate(p => p.CreatedAt, dbType: NpgsqlDbType.Timestamp)
            .Duplicate(p => p.UpdatedAt, dbType: NpgsqlDbType.Timestamp)
            // A foreign key, that has to be in the other table.
            // Normally we wanted to take the name as foreign key, but marten creates
            // the key on the id column of the user setting section table.
            .ForeignKey<UserSettingSectionDbModel>(
                p => p.UserSettingSection.Id,
                fkd => fkd.OnDelete = CascadeAction.Cascade)
            .ForeignKey<UserDbModel>(
                p => p.UserId,
                foreignKeySetup => foreignKeySetup.OnDelete = CascadeAction.Cascade)
            // Disables all default meta fields that martin creates.
            .Metadata(p => p.DisableInformationalFields());

        For<UserSettingSectionDbModel>()
            .DocumentAlias("UserSettingSections")
            .Duplicate(p => p.Name)
            .Duplicate(p => p.CreatedAt)
            .Metadata(p => p.DisableInformationalFields());

        For<UserDbModel>()
            .DocumentAlias("Users")
            .Metadata(setup => setup.DisableInformationalFields());

        For<ProjectionStateLightDbModel>()
            .DocumentAlias("ProjectionState")
            .Identity(state => state.StreamName)
            .Metadata(setup => setup.DisableInformationalFields());
    }
}
