using UserProfileService.Controllers.V2;

namespace UserProfileService.Utilities;

public static class DefaultOperationRedirectionMapping
{
    public static List<OperationMap> GetDefaultMapping()
    {
        return new List<OperationMap>
               {
                   new OperationMap(
                       WellKnownTicketOperations.CreateUserProfile,
                       typeof(UsersController),
                       nameof(UsersController.GetUserProfileAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.CreateGroupProfile,
                       typeof(GroupsController),
                       nameof(GroupsController.GetGroupAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.CreateOrganizationProfile,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetOrganizationAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateUserProfile,
                       typeof(UsersController),
                       nameof(UsersController.GetUserProfileAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateGroupProfile,
                       typeof(GroupsController),
                       nameof(GroupsController.GetGroupAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateOrganizationProfile,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetOrganizationAsync)),
                   new OperationMap(WellKnownTicketOperations.DeleteGroup, null, null),
                   new OperationMap(WellKnownTicketOperations.DeleteOrganization, null, null),
                   new OperationMap(WellKnownTicketOperations.DeleteUser, null, null),
                   new OperationMap(
                       WellKnownTicketOperations.AddUserToRole,
                       typeof(RolesController),
                       nameof(RolesController.GetProfilesForRole)),
                   new OperationMap(
                       WellKnownTicketOperations.AddContainerProfileToRole,
                       typeof(RolesController),
                       nameof(RolesController.GetProfilesForRole)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateProfilesOfRole,
                       typeof(RolesController),
                       nameof(RolesController.GetProfilesForRole)),
                   new OperationMap(
                       WellKnownTicketOperations.AddUserToFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetAssignedProfilesOfFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.AddContainerProfileToFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetAssignedProfilesOfFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateProfilesOfFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetAssignedProfilesOfFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateRolesOfUser,
                       typeof(UsersController),
                       nameof(UsersController.GetAssignedObjectsOfUserByRoleAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateFunctionsOfUser,
                       typeof(UsersController),
                       nameof(UsersController.GetAssignedObjectsOfUserByFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateContainerProfilesOfUser,
                       typeof(UsersController),
                       nameof(UsersController.GetUserProfileAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.RemoveUserFromRole,
                       typeof(RolesController),
                       nameof(RolesController.GetProfilesForRole)),
                   new OperationMap(
                       WellKnownTicketOperations.RemoveContainerProfileFromRole,
                       typeof(RolesController),
                       nameof(RolesController.GetProfilesForRole)),
                   new OperationMap(
                       WellKnownTicketOperations.RemoveUserFromFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetAssignedProfilesOfFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.RemoveContainerProfileFromFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetAssignedProfilesOfFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.AssignProfilesToGroupProfile,
                       typeof(GroupsController),
                       nameof(GroupsController.GetChildrenOfGroupAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateProfilesToGroupProfileAssignments,
                       typeof(GroupsController),
                       nameof(GroupsController.GetChildrenOfGroupAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.AssignProfilesToOrganizationProfile,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetChildrenOfOrganizationAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateProfilesToOrganizationProfileAssignments,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetChildrenOfOrganizationAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.RemoveProfilesAssignmentsFromGroupProfile,
                       typeof(GroupsController),
                       nameof(GroupsController.GetChildrenOfGroupAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.RemoveProfileAssignmentsFromOrganizationProfile,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetChildrenOfOrganizationAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.CreateRole,
                       typeof(RolesController),
                       nameof(RolesController.GetRoleAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateRole,
                       typeof(RolesController),
                       nameof(RolesController.GetRoleAsync)),
                   new OperationMap(WellKnownTicketOperations.DeleteRole, null, null),
                   new OperationMap(
                       WellKnownTicketOperations.CreateTag,
                       typeof(TagsController),
                       nameof(TagsController.GetTagAsync)),
                   new OperationMap(WellKnownTicketOperations.DeleteTag, null, null),
                   new OperationMap(
                       WellKnownTicketOperations.CreateFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetFunctionAsync)),
                   new OperationMap(WellKnownTicketOperations.DeleteFunction, null, null),
                   new OperationMap(
                       WellKnownTicketOperations.CreateUserTags,
                       typeof(UsersController),
                       nameof(UsersController.GetUserProfileAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.CreateGroupTags,
                       typeof(GroupsController),
                       nameof(GroupsController.GetGroupAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.CreateOrganizationTags,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetOrganizationsAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.CreateRoleTags,
                       typeof(RolesController),
                       nameof(RolesController.GetRoleAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.CreateFunctionTags,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteTagsFromUser,
                       typeof(UsersController),
                       nameof(UsersController.GetUserProfileAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteTagsFromGroup,
                       typeof(GroupsController),
                       nameof(GroupsController.GetGroupAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteTagsFromOrganization,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetOrganizationAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteTagsFromRole,
                       typeof(RolesController),
                       nameof(RolesController.GetRoleAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteTagsFromFunction,
                       typeof(FunctionsController),
                       nameof(FunctionsController.GetFunctionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteCustomPropertiesForOrganization,
                       typeof(OrganizationsController),
                       nameof(OrganizationsController.GetOrganizationAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.SetConfigForGroup,
                       typeof(GroupsController),
                       nameof(GroupsController.GetGroupConfig)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateConfigForGroup,
                       typeof(GroupsController),
                       nameof(GroupsController.GetGroupConfig)),
                   new OperationMap(WellKnownTicketOperations.RemoveConfigFromGroup, null, null),
                   new OperationMap(
                       WellKnownTicketOperations.SetConfigForUser,
                       typeof(UsersController),
                       nameof(UsersController.GetUserConfig)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateConfigForUser,
                       typeof(UsersController),
                       nameof(UsersController.GetUserConfig)),
                   new OperationMap(WellKnownTicketOperations.RemoveConfigFromUser, null, null),
                   new OperationMap(WellKnownTicketOperations.SetConfigForProfiles, null, null),
                   new OperationMap(
                       WellKnownTicketOperations.CreateUserSettingSection,
                       typeof(UserSettingsController),
                       nameof(UserSettingsController.GetUserSettingObjectsForSectionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.UpdateUserSettingObject,
                       typeof(UserSettingsController),
                       nameof(UserSettingsController.GetUserSettingObjectsForSectionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteUserSettingObject,
                       typeof(UserSettingsController),
                       nameof(UserSettingsController.GetUserSettingObjectsForSectionAsync)),
                   new OperationMap(
                       WellKnownTicketOperations.DeleteUserSettingSection,
                       typeof(UserSettingsController),
                       nameof(UserSettingsController.GetUserSettingObjectsForSectionAsync))
               };
    }
}
