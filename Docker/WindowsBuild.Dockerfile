# Build with "docker build -t chaskis.build.windows -f .\Docker\WindowsBuild.Dockerfile ." from the root directory.
# This file is used to compile Chaskis in a CI Windows Environment.

FROM mcr.microsoft.com/windows/servercore:ltsc2019-amd64

# Alright, we need the following things installed:
# - Chocolatey
# - Dotnet SDK
# - WiX toolset
# - NuGet

# But first, we need to (safely) install Chocolatey.
# We could download the powershell script and run it,
# but I don't like doing that.
# So what we are going to do instead is download the install stuff via
# NuGet.  But first we need NuGet!
# NuGet is downloaded when we first run cake.
# So, let's put that over to the container, install chocolatey, then
# delete everything

RUN mkdir "c:\\workdir"
COPY "tools\\NuGet.CommandLine.5.5.1\\tools\\NuGet.exe" "c:\\workdir\\NuGet.exe"

RUN "c:\\workdir\\NuGet.exe install -ExcludeVersion -OutputDirectory c:\\workdir chocolatey"
RUN ["c:\\windows\\system32\\WindowsPowerShell\\v1.0\\powershell.exe", "c:\\workdir\\chocolatey\\tools\\chocolateyInstall.ps1", "-y"]

RUN "rmdir /S /Q c:\\workdir"

# Now, install everything we need!
RUN [ "C:\\ProgramData\\chocolatey\\choco.exe", "install", "-y", "dotnetcore-sdk" ]

# Opt-out of dotnet telemetry
RUN ["setx", "DOTNET_CLI_TELEMETRY_OPTOUT", "1", "/M"]

RUN [ "C:\\ProgramData\\chocolatey\\choco.exe", "install", "-y", "NuGet.CommandLine" ]

# For WiX.
RUN ["c:\\windows\\system32\\WindowsPowerShell\\v1.0\\powershell.exe", "-Command", "{Install-WindowsFeature Net-Framework-Core}"]
# RUN "C:\\ProgramData\\chocolatey\\choco.exe install -y --allow-empty-checksums --ignore-package-exit-codes DotNet3.5 || echo done"

# This version doesn't hang: http://disq.us/p/1lct9e1
RUN [ "C:\\ProgramData\\chocolatey\\choco.exe", "install", "-y", "-i", "--version", "3.10.3.300702", "wixtoolset" ]

RUN mkdir "c:\\Cake"
RUN dotnet tool install --tool-path c:\\Cake Cake.Tool

# Switch to unelevated user.
USER ContainerUser

# So dotnet runs once and doesn't print out that annoying telemetry message.
RUN "dotnet || echo done"

ENTRYPOINT [ "c:\\Cake\\dotnet-cake" ]
