# [Choice] .NET Core version: 5.0, 3.1, 2.1
ARG VARIANT=7.0
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:${VARIANT}

# Update args in docker-compose.yaml to set the UID/GID of the "vscode" user.
ARG USER_UID=1000
ARG USER_GID=$USER_UID
RUN if [ "$USER_GID" != "1000" ] || [ "$USER_UID" != "1000" ]; then groupmod --gid $USER_GID vscode && usermod --uid $USER_UID --gid $USER_GID vscode; fi
USER $USER_UID:$USER_GID
