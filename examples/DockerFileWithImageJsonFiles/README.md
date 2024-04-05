## Docker Build with JSON Files

In these examples, Docker will build the specific Dockerfile located within the service/worker folder. Here, you are free to modify the Dockerfile as needed. For instance, you can alter the base image or adjust other components. All configurations are stored in JSON files. For guidance on configuring the main components, please refer to the [README.md](https://github.com/bechtleav360/Maverick.UserProfileService/blob/main/README.md) file. To initiate the build for the ups-environment, you can use the following command:

```ps1
docker compose --env-file docker-compose-env-vars.env  up
```

## The docker-compose-env-var file
The file is utilized to configure the primary storage components. You can configure parameters such as the user, password, and database, for example. However, please note that if you modify the password or user of any components, you must also update them in the configuration accordingly. Prior to startup, ensure to modify the **WORKING_DIRECTORY_CONTEXT** variable. This variable contains the absolute path to the UserProfileService.

For example, if your UserProfileService code is located under E:\Repos\Maverick.UserProfileService, then your variable should appear as follows:

```env
WORKING_DIRECTORY_CONTEXT='E:\Repos\Maverick.UserProfileService'
```

## The configuration files

In this example, configuration is performed using JSON files. JSON files offer greater readability and understanding compared to env files. These JSON files are mapped to the required services container in the **appsettings.Development.json** file. It's important to note that all examples are intended to run in a development environment.








