FROM ubuntu:18.04

RUN apt -y update
RUN apt -y install wget apt-transport-https

# Install dotnet core runtime
# Taken from here: https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1804
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt -y update
RUN apt -y install dotnet-runtime-3.1

# Delete unneeded things:
RUN apt -y remove wget
RUN dpkg -r remove packages-microsoft-prod
RUN rm packages-microsoft-prod.deb
RUN apt -y clean
RUN apt -y autoclean

# Create user who does not have root access.
RUN useradd -d /chaskis -m chaskis
RUN chown -R chaskis:chaskis /chaskis

COPY ./DistPackages/debian/chaskis.deb /tmp/chaskis.deb
RUN apt -y install /tmp/chaskis.deb
RUN ["rm", "/tmp/chaskis.deb"]

USER chaskis
RUN mkdir /chaskis/.config

RUN ls -la /usr/lib/Chaskis/bin/

ENTRYPOINT [ "dotnet", "/usr/lib/Chaskis/bin/Chaskis.Service.dll" ]
