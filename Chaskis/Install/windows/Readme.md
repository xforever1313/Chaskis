Wix Installation
============

This project creates the .msi file for installing on Windows.  However, dependencies must be installed first:

Dependencies
=====
The WiX Toolset must be installed in order for this project to show up in the solution file; otherwise it will remain unloaded.  It can either be installed with chocolatey via ```choco install wixtoolset``` or downloaded from their [site](http://wixtoolset.org/releases/)

You must also install the Visual Studio Extension.  The link can be found on their [site](http://wixtoolset.org/releases/) or in ```Extensions and Updates``` in Visual Studio.  The name is ```Wix Toolset Visual Studio 2017 Extension```.

Finally, to build the .msi, select the ```Install``` target while building, and the platform should be ```x64```.
