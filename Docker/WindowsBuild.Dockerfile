# Build with "docker build -t chaskis.build.windows -f .\Docker\WindowsBuild.Dockerfile ." from the root directory.
# This file is used to compile Chaskis in a CI Windows Environment.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1

RUN dotnet tool install -g Cake.Tool

# Switch to unelevated user.
USER ContainerUser

ENTRYPOINT [ "dotnet", "cake" ]
