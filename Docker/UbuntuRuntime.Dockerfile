FROM ubuntu:18.04

RUN apt -y update
RUN apt -y install wget apt-transport-https

# Install dotnet core runtime
# Taken from here: https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1804
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt -y update
RUN apt -y install dotnet-runtime-3.1

# For the cowsay bot plugin
RUN apt -y install cowsay

# Delete unneeded things:
RUN apt -y remove wget
RUN dpkg -r remove packages-microsoft-prod
RUN rm packages-microsoft-prod.deb
RUN apt -y clean
RUN apt -y autoclean

# Create user who does not have root access.
# Chaskis looks at the username to determine if we are in a container or not
# We look for containeruser to be specific.
RUN useradd -d /chaskis -m containeruser
RUN chown -R containeruser:containeruser /chaskis

COPY ./DistPackages/debian/chaskis.deb /tmp/chaskis.deb
RUN apt -y install /tmp/chaskis.deb
RUN ["rm", "/tmp/chaskis.deb"]

USER containeruser
RUN mkdir /chaskis/.config

ENTRYPOINT [ "/usr/lib/Chaskis/bin/Chaskis.Service" ]
