FROM archlinux

# Update everything
RUN pacman -Syy --noconfirm && pacman -Syu --noconfirm

# Install the dotnet SDK
RUN pacman -S dotnet-sdk-3.1 fakeroot base-devel --noconfirm

# Make unelevated user.
# Must be in all lower case on Arch (not the case for Ubuntu).
RUN useradd --home /home/containeruser/ --create-home containeruser
USER containeruser
