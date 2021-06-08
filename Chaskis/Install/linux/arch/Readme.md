Installing for Arch Linux
=======

With AUR:
----

Chaskis is in the AUR, you can install it with the command ```yaourt -S chaskis```.

Without AUR:
----

* Open a terminal, and run the following commands:
  * ```cake --target=build```
  * ```cake --target=debian_pack```
  * ```cake --target=pkgbuild```
* run ```pacman -U chaskis-version.pkg.tar.xz```
