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

Import('envBase')

def MsBuild(target, source, env):
    flags = env["MSBUILD_FLAGS"]
    return subprocess.call(flags)

def BuildInstallWindows(target, source, env):
    '''
    Runs the install build.
    Also creates a sha256 checksum of the installed object.
    The install output should be the first target.  The checksum file
    should be the second one.
    '''
    flags = env["MSBUILD_FLAGS"]
    status = subprocess.call(flags)

    if (status == 0):
        with io.open(str(target[0]), 'rb') as inFile:
            readFile = inFile.read()
            h = hashlib.sha256(readFile)
            hash = h.hexdigest()

        with io.open(str(target[1]), 'w') as outFile:
            outFile.write(hash)

    return status

buildEnv = envBase.Clone()
buildEnv.Append(BUILDERS = {"MsBuild" : Builder(action=MsBuild)})
debugEnv = buildEnv.Clone()
releaseEnv = buildEnv.Clone()

installEnv = envBase.Clone()
installEnv.Append(BUILDERS={"BuildInstall" : Builder(action=BuildInstallWindows)})

buildOptions = ['msbuild', envBase['SLN'], '/restore:' + str(envBase['RESTORE'])]

debugBuildOptions = buildOptions + ['/p:Configuration=Debug']
releaseBuildOptions = buildOptions + ['/p:Configuration=Release']
installBuildOptions = buildOptions + ['/p:Configuration=Install', '/p:Platform=x64']

debugEnv["MSBUILD_FLAGS"] = debugBuildOptions
releaseEnv["MSBUILD_FLAGS"] = releaseBuildOptions
installEnv["MSBUILD_FLAGS"] = installBuildOptions

# Need to get ALL .cs, .csproj, .shproj, .resx, .projitems.  If any of these files change,
# We need to trigger a re-compile.

filePatternStr = r'\.(cs|csproj|shproj|resx|projitems)$'
pattern = re.compile(filePatternStr)

# Sources do not live in these directories, ignore them.
ignoredDirs = re.compile(r'(obj|bin|packages|\.git|\.vs)[\\/].*' + filePatternStr)

csprojDirs = []

sources=[]

for root, dirs, files in os.walk(envBase['REPO_ROOT']):
    for f in files:
        file = os.path.join(root, f)
        if ((bool(pattern.search(file)) == True) and (bool(ignoredDirs.search(file)) == False)):
            sources += [file]
            # Ignore SethCS for .csproj.  Nothing will output from there.
            if (file.endswith('.csproj') and bool(re.search('SethCS', file)) == False): 
                csprojDirs += [(root, file)]

frameworkPattern = re.compile(r'\<TargetFramework\>\s*(?P<framework>[\w\.]+)\s*\</TargetFramework\>')
outputTypePattern = re.compile(r'\<OutputType\>\s*(?P<output>\w+)\s*\</OutputType\>')

debugTargets=[]
releaseTargets=[]
installTargets=[]

for csproj in csprojDirs:
    with (open(csproj[1], 'r', encoding="UTF-8")) as inFile:
        try:
            contents = inFile.read()
            frameworkMatch = frameworkPattern.search(contents)
            framework = frameworkMatch.group('framework')

            outputMatch = outputTypePattern.search(contents)
            if (bool(outputMatch)):
                outputType = outputMatch.group('output')
            else:
                outputType = "dll"

        except:
            print("Error when parsing " + csproj[1])
            raise

    if (outputType.lower() == "exe"):
        extension = ".exe"
    else:
        extension = ".dll"

    fileName = os.path.basename(csproj[1])
    fileName = fileName.replace('.csproj', extension)

    debugTarget = os.path.join(
        csproj[0],
        'bin',
        'Debug',
        framework,
        fileName
    )

    debugTargets += [debugTarget]

    releaseTarget = os.path.join(
        csproj[0],
        'bin',
        'Release',
        framework,
        fileName
    )

    releaseTargets += [releaseTarget]

installTargets = [
    os.path.join(envBase['INSTALL_DIR'], 'windows', 'bin', 'x64', 'Release', 'ChaskisInstaller.msi'),
    os.path.join(envBase['INSTALL_DIR'], 'windows', 'bin', 'x64', 'Release', 'ChaskisInstaller.msi.sha256')
]

def addCleanTargets(buildTarget, targets):
    for target in targets:
        Clean(buildTarget, os.path.dirname(target))

buildTargets = {}
buildTargets['DEBUG'] = debugEnv.MsBuild(target=debugTargets, source=sources)
buildTargets['RELEASE'] = releaseEnv.MsBuild(target=releaseTargets, source=sources)

# This will cause MSBuild to run twice if we're building install for windows... once for release, and once for install.
# Meh, we'll deal with it.  If we simply append the install targets to the release targets,
# scons gets angry, and will throw up a bunch of warnings.
# Maybe there's a smarter way to do it, but I can't think of one D:
buildTargets['INSTALL'] = installEnv.BuildInstall(target=installTargets, source=buildTargets['RELEASE'])

addCleanTargets(buildTargets['DEBUG'], debugTargets)
addCleanTargets(buildTargets['RELEASE'], releaseTargets)
addCleanTargets(buildTargets['INSTALL'], installTargets)

Return('buildTargets')