A Windows agent must be configured correctly, these are the instructions on how to do that.

First, install the following chocolatey packages:
 - docker-desktop
 - docker-cli
 - dotnetcore-sdk

The following Windows features must be turned on:
 - Containers
 - Hyper-V
 - OpenSSH Server (Search for Optional Features in the Windows 10).  Can also install with Chocolatey via "choco install openssh -params "/SSHServerFeature /SSHAgentFeature""
   This is so Jenkins can remote into the Windows machine to build.

Jenkins Agent User Config:
 - The user Jenkins will SSH into the Windows machine as must be in the docker-users and adminstrator group.
   If the user is not in the administrator group, dockercli.exe will not work in an SSH environment.  I have no idea why, it just won't.
 - Generate an SSH key with "ssh key-gen".  Make sure the public key is in the user's .ssh/authorized_keys file.
 - In c:\Program Data\ssh\sshd_config, comment out the following lines at the bottom (or make it work, not sure how though).
   #Match Group administrators
   #       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys

   If this is uncommented, Jenkins will NOT be able to SSH in as your user; ssh wil reject it.  This is because the user Jenkins SSHes into
   is in the admin group.
   ... Docker for Windows is not a fun experience.

Power Settings (Possibly not needed)

Yes, we even need to change the Power Settings because Windows tries to be "helpful" but can put Docker into a bad state.
This is to workaround a problem called out here: https://stackoverflow.com/questions/40668908/running-docker-for-windows-error-when-exposing-ports
 - Hit start and search for "Power".  Click on "Power & Sleep" settings.
 - On the right column, hit "Additional Power Settings".
 - On the Window that pops up, hit "Choose what the power buttons do"
 - Uncheck "Turn off fast startup"

 Before running
 - Ensure the Docker Engine is running by going to Start -> searching for "Services" and hitting enter.
   You can have it start automatically by setting the Startup Type to "Automatic".
 - Also make sure the "Docker Desktop Service" is also running.
