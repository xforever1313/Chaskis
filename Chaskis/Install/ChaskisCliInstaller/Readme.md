CLI Installer
==============

This tool parses our wix XML file and then installs all the files specfied in it in the specified root directory.

This is useful for platforms that do not support WIX.

Our PKGBUILD system calls this under the hood so it copies everything for us.

Usage
======

```
This CLI program is used to install Chaskis.

Usage: ChaskisCliInstaller.exe slnDir rootDir wixXmlFile debug|release

slnDir - Where Chaskis.sln lives
rootDir - Where the install root will be (e.g. /usr/lib/).  The Chaskis folder gets created in here.
wixXmlFile - The Wix XML file to use.
debug|release - Which target we are using.
```
