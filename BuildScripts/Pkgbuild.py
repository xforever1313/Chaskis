from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import hashlib
import io
import platform
import os
import re
import subprocess

import Common

Import('envBase')

pkgBuildEnv = envBase.Clone()

archWorkDir = os.path.join(pkgBuildEnv['ARCH_INSTALL_DIR'], 'obj')
originalPkgBuild = os.path.join(pkgBuildEnv['ARCH_INSTALL_DIR'], 'PKGBUILD')
outputFolder = os.path.join(pkgBuildEnv['ARCH_INSTALL_DIR'], 'bin')

# According the the makepkg documentation https://www.archlinux.org/pacman/makepkg.8.html
# the build script must be in the same directory makepkg is called from.  We really
# don't want all of that stuff to crowd the repo, so we'll stuff all the building into
# the obj folder. However, this means we need to first copy the original PKGBUILD file
# into the obj folder.

pkgbuildCopyTarget = pkgBuildEnv.Command(
    target=os.path.join(archWorkDir, "PKGBUILD"),
    source=originalPkgBuild,
    action=Copy("$TARGET", "$SOURCE")
)

###
# Make Package
###

# Read the PKGBUILD folder to determine the target info.
# The outputted target will be packgetname-version-rel-arch.pkg.tar.xz

def RunMakePkg(target, source, env):
    args =['makepkg']

    process = subprocess.Popen(
        args,
        cwd=os.path.dirname(str(source[0]))
    )

    return process.wait()

pkgBuildEnv.Append(BUILDERS={"MakePkg" : Builder(action=RunMakePkg)})

with io.open(originalPkgBuild, 'r', encoding='utf-8') as inFile:
    contents = inFile.read()

def Match(pattern, contents, groupName):
    searchResult = re.search(pattern, contents)
    return searchResult.group(groupName)

pkgName = Match(r'pkgname\s*=\s*(?P<pkgname>\w+)', contents, 'pkgname')
pkgVer = Match(r'pkgver\s*=\s*(?P<pkgver>[\d\.]+)', contents, 'pkgver')
pkgRel = Match(r'pkgrel\s*=\s*(?P<pkgrel>\d+)', contents, 'pkgrel')
arch = Match(r"arch\s*=\s*\('(?P<arch>\w+)'\)", contents, 'arch')

expectedPkgName = pkgName + '-' + pkgVer + '-' + pkgRel + '-' + arch + '.pkg.tar.xz'

makepkgTarget = pkgBuildEnv.MakePkg(
    target=os.path.join(archWorkDir, expectedPkgName),
    source=pkgbuildCopyTarget
)

Clean(makepkgTarget, archWorkDir)

# Next, copy the created package to the bin folder.

makepkgTarget = pkgBuildEnv.Command(
    target=os.path.join(outputFolder, expectedPkgName),
    source=makepkgTarget,
    action=Copy('$TARGET', '$SOURCE')
)

###
# Checksum
###

pkgBuildEnv.Append(BUILDERS={"Checksum" : Builder(action=Common.Checksum)})

checksumTarget = pkgBuildEnv.Checksum(
    target=os.path.join(outputFolder, expectedPkgName + '.sha256'),
    source=makepkgTarget
)

if (pkgBuildEnv['SAVE_CHECKSUM']):
    checksumTarget = pkgBuildEnv.Command(
        target = os.path.join(pkgBuildEnv['SAVED_CHECKSUM_DIR'], expectedPkgName + '.sha256'),
        source = checksumTarget,
        action = Copy('$TARGET', '$SOURCE')
    )

    pkgBuildEnv.NoClean(checksumTarget)

###
# .SRCINFO
###

def CreateSrcInfo(target, source, env):
    args = ['makepkg', '--printsrcinfo']

    process = subprocess.Popen(
        args,
        cwd=os.path.dirname(str(source[0])),
        stdout=subprocess.PIPE
    )

    output = process.communicate()[0]

    status = process.wait()

    with io.open(str(target[0]), 'w', encoding='utf-8') as outFile:
        outFile.write(Common.ToUnicodeString(output))

    return status

pkgBuildEnv.Append(BUILDERS={"CreateSrcInfo" : Builder(action=CreateSrcInfo)})

srcInfoTarget = pkgBuildEnv.CreateSrcInfo(
    target=os.path.join(outputFolder, '.SRCINFO'),
    source=pkgbuildCopyTarget
)

targetsToReturn = [checksumTarget, srcInfoTarget]

Return('targetsToReturn')
