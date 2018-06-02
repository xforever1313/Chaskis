###
# Compile
###

chaskisroot=../../../

(cd $chaskisroot && nuget restore ./Chaskis.sln)
(cd $chaskisroot && exec msbuild /p:Configuration=Release ./Chaskis.sln)

###
# Copy files
###

pkgdir=`pwd`/chaskis
debiandir=$pkgdir/DEBIAN
bindir=$pkgdir/usr/bin
libdir=$pkgdir/usr/lib

mkdir -p $debiandir
mkdir -p $pkgdir
mkdir -p $bindir
mkdir -p $libdir

# Systemd service
mkdir -p $libdir/systemd/user/
cp ../systemd/chaskis.service $libdir/systemd/user/chaskis.service

# Binary
cp ../bin/chaskis $bindir/chaskis

# Everything else
(cd $chaskisroot &&
mono ./Install/ChaskisCliInstaller/bin/Release/ChaskisCliInstaller.exe ./ $libdir ./Install/windows/Product.wxs Release)

# Control
cp ./control $debiandir/control

###
# Create Package
###
dpkg-deb --build chaskis
