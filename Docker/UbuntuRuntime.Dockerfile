FROM ubuntu:18.04

# Cowsay needed for cowsay bot plugin.
RUN apt -y update && \
    apt -y install wget apt-transport-https cowsay

# Install dotnet core runtime
# Taken from here: https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1804
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt -y update && \
    apt -y install dotnet-runtime-3.1

# Delete unneeded things:
RUN apt -y remove wget && \
    dpkg -r remove packages-microsoft-prod && \
    rm packages-microsoft-prod.deb && \
    apt -y clean && \
    apt -y autoclean

# Create user who does not have root access.
# Chaskis looks at the username to determine if we are in a container or not
# We look for containeruser to be specific.
RUN useradd -d /chaskis -m containeruser && \
    chown -R containeruser:containeruser /chaskis

# Install .deb file.
COPY ./DistPackages/debian/chaskis.deb /tmp/chaskis.deb
RUN apt -y install /tmp/chaskis.deb && \
    rm /tmp/chaskis.deb

USER containeruser
RUN mkdir /chaskis/.config

ENTRYPOINT [ "/usr/lib/Chaskis/bin/Chaskis.Service" ]
