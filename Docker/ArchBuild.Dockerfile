FROM archlinux

# Update everything
RUN pacman -Syy --noconfirm
RUN pacman -Syu --noconfirm

# Install the dotnet SDK
RUN pacman -S dotnet-sdk --noconfirm

# Make unelevated user.
# Must be in all lower case on Arch (not the case for Ubuntu).
RUN useradd --home /home/containeruser/ --create-home containeruser
USER containeruser

# Install cake
RUN mkdir /home/containeruser/cake/
RUN dotnet tool install --tool-path /home/containeruser/cake/ Cake.Tool

ENTRYPOINT [ "/home/containeruser/cake/dotnet-cake" ]
