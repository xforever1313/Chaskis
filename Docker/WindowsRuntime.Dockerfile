# Build with "docker build -t chaskis.windows -f .\Docker\WindowsRuntime.Dockerfile ." from the root directory.
# This assumes there is a distro created.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1

# Move the Chaskis MSI over, and install
ENV installLocation="C:\\Program Files\\Chaskis"
COPY .\\DistPackages\\windows\\docker\\Chaskis ${installLocation}

# Switch to unelevated user.
USER ContainerUser

# Sanity check, make sure we installed correctly.
RUN ["C:\\Program Files\\Chaskis\\bin\\Chaskis.exe", "--version"]

ENTRYPOINT [ "C:\\Program Files\\Chaskis\\bin\\Chaskis.Service.exe" ]
