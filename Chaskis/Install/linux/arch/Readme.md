Installing for Arch Linux
=======

With AUR:
----

Chaskis is in the AUR, you can install it with the command ```yaourt -S chaskis```.

Without AUR:
----

* Edit ```PKGBUILD``` in this directory and replace ```sha256sums=('PUT THIS IN!')``` with ```sha256sums=('SKIP')```.  You may also download the source from GitHub and replace it with the actual sha256sum if desired.
* run ```makepkg```.  This will download the source from GitHub and attempt to build chaskis.
 * run ```pacman -U chaskis-version.pkg.tar.xz```