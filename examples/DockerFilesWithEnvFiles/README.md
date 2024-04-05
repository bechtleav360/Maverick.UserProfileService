## Docker Build with Environment Variables

In these examples, Docker will build the specific Dockerfile located within the service/worker folder. Here, you are free to modify the Dockerfile as needed. For instance, you can alter the base image or adjust other components. All configurations are now stored in env files. For guidance on configuring the main components, please refer to the [README.md](https://github.com/bechtleav360/Maverick.UserProfileService/blob/main/README.md) file. To initiate the build for the ups-environment, you can use the following command:

```ps1
docker compose --env-file docker-compose-env-vars.env  up
```

## The docker-compose-env-var file
The file is utilized to configure the primary storage components. You can configure parameters such as the user, password, and database, for example. However, please note that if you modify the password or user of any components, you must also update them in the configuration accordingly. Prior to startup, ensure to modify the WORKING_DIRECTORY_CONTEXT variable. This variable contains the absolute path to the UserProfileService.

For example, if your UserProfileService code is located under E:\Repos\Maverick.UserProfileService, then your variable should appear as follows:

**WORKING_DIRECTORY_CONTEXT='E:\Repos\Maverick.UserProfileService'**

## The configuration files
In this environment, we utilize .env files. These files contain variables that hold configurations for the running services. Typically, services can be configured using JSON files. Such configurations can also be observed in the examples folder. JSON configurations can be readily transformed into environment files. Below, we have the logging configuration:

```json
  "Logging": {
    "EnableLogFile": false,
    "LogFileMaxHistory": 3,
    "LogFilePath": "logs",
    "LogFormat": "text",
    "LogLevel": {
      "default": "Information"
    }
```

To represent this as an environment variable, it would look like:

```env
LOGGING__ENABLELOGFILE=false
LOGGING__LOGFILEMAXHISTORY=3
LOGGING__LOGFILEPATH=logs
LOGGING__LOGFORMAT=text
LOGGING__LOGLEVEL__DEFAULT=Information
```


The "**:**" separator will be replaced by "**=**" and the opening bracket "**{**" will be substituted with "**__**". If arrays are involved, we represent the first element of the array as "0", the second as "1", and so forth:

An example for a json array:

```json
   "fruits": [
    "Apple",
    "Banana",
    "Orange",
    "Strawberry",
    "Grape"
  ]
```

To represent this as an environment variable, it would look like:

```env
FRUIT_0=Apple
FRUIT_1=Banana
FRUIT_2=Orange
FRUIT_3=Strawberry
FRUIT_4=Grape
```









