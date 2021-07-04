FROM fedora

# Update everything
RUN dnf update

# Install the dotnet runtime
RUN dnf install dotnet-runtime-3.1

# Make unelevated user.
RUN useradd --home /home/containeruser/ --create-home containeruser
USER containeruser
