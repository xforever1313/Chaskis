# Build with "docker build -t chaskis.build.windows -f .\Docker\WindowsBuild.Dockerfile ." from the root directory.
# This file is used to compile Chaskis in a CI Windows Environment.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# Switch to unelevated user.
USER ContainerUser

# Tools are installed on a per-user basis, need to
# install Cake.Tool after switching users.
RUN dotnet tool install -g Cake.Tool

ENTRYPOINT [ "dotnet", "cake" ]
