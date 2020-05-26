FROM ubuntu:18.04

RUN apt -y update
RUN apt -y install wget apt-transport-https

# Install dotnet core SDK
# Taken from here: https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1804
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt -y update
RUN apt -y install dotnet-sdk-3.1

# Delete unneeded things:
RUN apt -y remove wget
RUN dpkg -r remove packages-microsoft-prod
RUN rm packages-microsoft-prod.deb
RUN apt -y clean
RUN apt -y autoclean

# Install cake
RUN mkdir /cake/
RUN dotnet tool install --tool-path /cake/ Cake.Tool

ENTRYPOINT [ "/cake/dotnet-cake" ]