###
# Compile
###

chaskisroot=../../../

(cd $chaskisroot && exec msbuild /restore /p:Configuration=Release ./Chaskis.sln)

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
mono ./Install/ChaskisCliInstaller/bin/Release/net471/Chaskis.CliInstaller.exe ./ $libdir ./Install/windows/Product.wxs Release net471 netstandard2.0)

# Control
cp ./control $debiandir/control

###
# Create Package
###
dpkg-deb --build chaskis
