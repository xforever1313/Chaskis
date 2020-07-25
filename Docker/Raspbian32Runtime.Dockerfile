FROM raspbian/stretch

RUN apt -y update
RUN apt -y install wget apt-transport-https

# Install the dotnet core runtime
RUN mkdir /dotnet/
RUN wget https://download.visualstudio.microsoft.com/download/pr/61d5be60-d855-4125-bce5-668dca9aefce/f91eb908b962e442532b6cd9e534a082/dotnet-runtime-3.1.6-linux-arm.tar.gz -O dotnet.tar.gz
RUN tar -xvzf dotnet.tar.gz -C /dotnet/
RUN rm dotnet.tar.gz

# For the cowsay bot plugin
RUN apt -y install cowsay

# Needed because otherwise, Chaskis will fail due to an error
# message that says "Couldn't find a valid ICU package installed ont eh system"
# ...Thank you to this stackoverflow:
# https://stackoverflow.com/questions/59119904/process-terminated-couldnt-find-a-valid-icu-package-installed-on-the-system-in
RUN apt -y install libicu57

# Next install Chaskis.
RUN wget https://files.shendrick.net/projects/chaskis/releases/latest/debian/chaskis.deb -O chaskis.deb
RUN apt -y install ./chaskis.deb
RUN ["rm", "chaskis.deb"]

# no longer need wget
RUN apt -y remove wget
RUN apt -y clean
RUN apt -y autoclean

# Create user who does not have root access.
# Chaskis looks at the username to determine if we are in a container or not
# We look for containeruser to be specific.
RUN useradd -d /chaskis -m containeruser
RUN chown -R containeruser:containeruser /chaskis

USER containeruser
RUN mkdir /chaskis/.config

# Sanity Check
RUN ["/dotnet/dotnet", "/usr/lib/Chaskis/bin/Chaskis.dll", "--version"]

ENTRYPOINT [ "/dotnet/dotnet", "/usr/lib/Chaskis/bin/Chaskis.Service.dll" ]
