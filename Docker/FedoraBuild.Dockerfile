FROM fedora

# Update everything + Install dotnet runtime.
RUN dnf update -y && dnf install dotnet-runtime-3.1 -y

# Make unelevated user.
RUN useradd --home /home/containeruser/ --create-home containeruser
USER containeruser
