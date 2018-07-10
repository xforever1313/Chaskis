from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import glob
import hashlib
import io
import platform
import os
import re
import subprocess

import Common

Import('envBase')

debEnv = envBase.Clone()

debianWorkDir = os.path.join(debEnv['DEBIAN_INSTALL_DIR'], 'obj')
debianChaskisDir = os.path.join(debianWorkDir, 'chaskis')
debianPkgDir = os.path.join(debianChaskisDir, 'DEBIAN')
debianUsrDir = os.path.join(debianChaskisDir, 'usr')
debianBinDir = os.path.join(debianUsrDir, 'bin')
debianLibDir = os.path.join(debianUsrDir, 'lib')
debianSystemdDir = os.path.join(debianLibDir, 'systemd')
debianSystemdUserDir = os.path.join(debianSystemdDir, 'user')

## Debian Files:
linuxBin = os.path.join(debEnv['LINUX_INSTALL_DIR'], 'bin', 'chaskis')
systemdFile = os.path.join(debEnv['LINUX_INSTALL_DIR'], 'systemd', 'chaskis.service')
controlFile = os.path.join(debEnv['DEBIAN_INSTALL_DIR'], 'control')

###
# Copy Linux Files First
###

linuxBinTarget = debEnv.Command(
    target=os.path.join(debianBinDir, 'chaskis'),
    source=linuxBin,
    action=Copy('$TARGET', '$SOURCE')
)

systemdFileTarget = debEnv.Command(
    target=os.path.join(debianSystemdUserDir, 'chaskis.service'),
    source=systemdFile,
    action=Copy('$TARGET', '$SOURCE')
)

controlFileTarget = debEnv.Command(
    target=os.path.join(debianPkgDir, 'control'),
    source=controlFile,
    action=Copy('$TARGET', '$SOURCE')
)

###
# CLI Target
###

def RunCli(target, source, env):
    if(any(platform.win32_ver())):
        raise Exception('Can only execute this on Linux')
    
    exePath = os.path.join(
        env['CLI_INSTALL_DIR'],
        'bin',
        'Release',
        env['EXE_RUNTIME'],
        'Chaskis.CliInstaller.exe'
    )

    wixXmlFile = os.path.join(
        env['INSTALL_DIR'],
        "windows",
        "Product.wxs.linux"
    )

    args = [
        'mono', # Execute bit is not set by default.
        exePath,
        env['SLN_DIR'],
        debianLibDir,
        wixXmlFile,
        'Release',
        env['EXE_RUNTIME'],
        env['PLUGIN_RUNTIME']
    ]

    return subprocess.call(args)

debEnv.Append(BUILDERS = {"RunCli" : Builder(action=RunCli)})

chaskisCliTarget = debEnv.RunCli(
    target = os.path.join(debianLibDir, 'Chaskis', 'bin', 'Chaskis.exe'),
    source=[linuxBinTarget, systemdFileTarget, controlFileTarget]
)

Clean(chaskisCliTarget, debianWorkDir)

###
# Create DEB
###

def CreateDeb(target, source, env):
    process = subprocess.Popen(
        ['dpkg-deb', '--build', 'chaskis'],
        cwd=debianWorkDir
    )

    return process.wait()

debEnv.Append(BUILDERS = {"Deb" : Builder(action=CreateDeb)})

debTarget = debEnv.Deb(
    target=os.path.join(debianWorkDir, 'chaskis.deb'),
    source=chaskisCliTarget
)

debTarget = debEnv.Command(
    target = os.path.join(debEnv['DEBIAN_INSTALL_DIR'], 'bin', 'chaskis.deb'),
    source=debTarget,
    action=Copy('$TARGET', '$SOURCE')
)

###
# Checksum
###

debEnv.Append(BUILDERS = {"Checksum" : Builder(action=Common.Checksum)})

checksumTarget = debEnv.Checksum(
    target = os.path.join(debEnv['DEBIAN_INSTALL_DIR'], 'bin', 'chaskis.deb.sha256'),
    source=debTarget
)

###
# Targets to return
###
publicTargets = {}
publicTargets['CLI_TARGET'] = chaskisCliTarget
publicTargets['DEB_TARGET'] = checksumTarget

Return('publicTargets')