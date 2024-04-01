# https://hub.docker.com/_/microsoft-dotnet
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

# copy all files
COPY /src/ .

# restore
WORKDIR /source/UserProfileService
RUN dotnet restore

# build app
WORKDIR /source
RUN dotnet publish UserProfileService/UserProfileService.csproj -c Release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app ./

# Create a non-privileged user that the app will run under.
# See https://docs.docker.com/develop/develop-images/dockerfile_best-practices/#user
ARG UID=10001
RUN adduser \
    --disabled-password \
    --gecos "" \
    --home "/nonexistent" \
    --shell "/sbin/nologin" \
    --no-create-home \
    --uid "${UID}" \
    appuser
USER appuser

ENTRYPOINT ["dotnet", "UserProfileService.dll"]
