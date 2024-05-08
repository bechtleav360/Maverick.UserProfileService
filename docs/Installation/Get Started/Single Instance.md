# Single Instance
If you want to start the user profile service from the codebase and you have met all the [requirements](../Requirements.md), you can launch it from the console. First, clone the code from GitHub:

`git clone https://github.com/bechtleav360/Maverick.UserProfileService.git`

Then you can start each components with the command:
`dotnet run`

You need to navigate to the correct folder:
 
 * For starting UserProfile-API: `../Maverick.UserProfileService/src/UserProfileService`
 * For startingstarting Saga-Worker: `../Maverick.UserProfileServicesrc/src/UserProfileService.Saga.Worker`
 * For starting UPS-Sync: `../Maverick.UserProfileServicesrc/src/UserProfileService.Sync`

## Starting the UPS from an IDE
To start the UPS with an IDE, you can use either [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/de/downloads/) or [Rider](https://www.jetbrains.com/de-de/rider/). Please note that Rider offers only a 30-day trial period. You can open the solution `UserProfileService.sln` located in the folder `../Maverick.UserProfileService`.

