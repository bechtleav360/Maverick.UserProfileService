## Docker Build with Environment Variables

In these example docker will build the explicit Dockerfile that is placed under the folder of the service/worker. Here you can feel free to changed the **Dockerfile**. You can changed for example the base image or changed other components. All configuration are now stored in *env* files. For configuration the main components you can refer to this README.md file. The command you can start the build for the ups-environment:

```ps1
docker compose -f .\docker-compose-ups-environment.yml --env-file docker-compose-env-vars.env  up
```

## The docker-compose-env-var file
The file is used to configure the main storing components. You can configure the user, password the database for example. But be aware, if you configure the password or user of the components you have to changed them in the configuration as well. Before you want to startup you should change the **WORKING_DIRECTORY_CONTEXT**. This Variable has the absolut path to the UserProfileService.

For example when your UserProfileSerivce Code is placed under E:\Repos\Maverick.UserProfileService, then your variable should look like:

**WORKING_DIRECTORY_CONTEXT='E:\Repos\Maverick.UserProfileService'**

## The configuration files


