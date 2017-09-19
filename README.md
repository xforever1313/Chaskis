Chaskis 
==========
A generic framework written in C# for making IRC Bots.

Build Status
--------------
[![Build Status](https://travis-ci.org/xforever1313/Chaskis.svg?branch=master)](https://travis-ci.org/xforever1313/Chaskis) [![Build status](https://ci.appveyor.com/api/projects/status/n8sbo1ay6wr2xxyc/branch/master?svg=true)](https://ci.appveyor.com/project/xforever1313/chaskis/branch/master)

Packages
--------------

[![NuGet](https://img.shields.io/nuget/v/ChaskisCore.svg)](https://www.nuget.org/packages/ChaskisCore/) [![Chocolatey](https://img.shields.io/chocolatey/v/chaskis.svg)](https://chocolatey.org/packages/chaskis/) [![AUR](https://img.shields.io/aur/version/chaskis.svg)](https://aur.archlinux.org/packages/chaskis/)

About
--------
Chaskis is a framework for creating IRC Bots in an easy way.  It is a plugin-based architecture written in C# that can be run on Windows or Linux (with the use of Mono).  Users of the bot can add or remove plugins to run, or even write their own.

Chaskis is named after the [Chasqui](https://en.wikipedia.org/wiki/Chasqui), messengers who ran trails in the Inca Empire to deliver messages.

Install Instructions
----------------------

### Windows ###
Run the Windows [installer](https://files.shendrick.net/projects/chaskis/releases/).  This will install Chaskis to C:\Program Files\Chaskis.  The service will also be installed but NOT enabled.

You can also install via [chocolatey](https://chocolatey.org/packages/chaskis/) by running ```choco install chaskis```.

### Linux ###

 * **Arch** - Install with the AUR: ```yaourt -S chaskis```
 * **Fedora** - You need to download the RPM manually or compile it yourself with the .spec file.  See [#22](https://github.com/xforever1313/Chaskis/issues/22) as to why.

Configuration
---------------
Once Chaskis is installed, run ```Chaskis.exe --bootstrap``` to create an empty configuration in side of your Application Data folder.  On Windows, this is ```C:\Users\you\AppData\Roaming\Chaskis```.  On Linux, this is ```/home/you/.config/Chaskis```.  If you wish to install a default config else where, specify that in the ```--chaskisroot argument``` (e.g. ```Chaskis.exe --bootstrap --chaskisroot=/home/you/chakisconfig```).

Note, if running Chaskis as a Service, you MUST store your user's configuration in the Application Data folder.

After running Chaskis.exe with the bootstrap argument, default configurations will appear in the folder.  They are XML Files, and their instructions live as comments in those files.  Plugin configuration lives in the Plugins folder.

Running
---------------
### Chaskis.exe ###

There are two ways to run Chaskis.  The first is with ```Chaskis.exe```. By default, this will look for configuration in your Application Data folder, but you can override this by passing in the ```--chaskisroot``` argument (e.g. ```Chaskis.exe --chaskisroot=/home/you/chakisconfig```).  You can run multiple instances of Chaskis.exe per user this way.  Running Chaskis in a tool such as tmux or screen an keep it running in the background.

### ChaskisService.exe ###

The other way to run Chaskis is by the service.  The advantage of a service is you can tell Chaskis to run when your system starts up.  The disadvantage is you can only have on configuration per user, which lives in the user's Application Data folder.

* [Windows Instructions](https://github.com/xforever1313/Chaskis/wiki/Running-as-a-Windows-Service)
* [Linux Instructions](https://github.com/xforever1313/Chaskis/wiki/Running-as-a-Linux-Service)

Writing Plugins
----------------

Visit our Wiki page [here](https://github.com/xforever1313/Chaskis/wiki/Writing-Plugins).

