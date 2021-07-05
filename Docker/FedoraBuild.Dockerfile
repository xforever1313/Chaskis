FROM fedora

# Update everything + Install dotnet runtime.
RUN dnf update -y && dnf install dotnet-runtime-3.1 fedora-packager fedora-review -y

# Make unelevated user.
RUN useradd --home /home/containeruser/ --create-home containeruser && usermod -a -G mock containeruser
USER containeruser
