FROM fedora

# Install dotnet runtime.
RUN dnf install dotnet-runtime-3.1 fedora-packager fedora-review -y

# Make unelevated user.
RUN useradd --home /home/containeruser/ --create-home containeruser && usermod -a -G mock containeruser
USER containeruser
