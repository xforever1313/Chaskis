from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import io
import platform
import os
import re
import subprocess

import Common

Import('envBase')

def MsBuild(target, source, env):
    flags = env["MSBUILD_FLAGS"]
    return subprocess.call(flags)

def BuildInstallWindows(target, source, env):
    if(any(platform.win32_ver()) == False):
        raise Exception('Can only execute this on Windows')

    flags = env["MSBUILD_FLAGS"]
    status = subprocess.call(flags)

    return status

buildEnv = envBase.Clone()
buildEnv.Append(BUILDERS = {"MsBuild" : Builder(action=MsBuild)})
debugEnv = buildEnv.Clone()
releaseEnv = buildEnv.Clone()

installEnv = envBase.Clone()
installEnv.Append(BUILDERS={"BuildInstall" : Builder(action=BuildInstallWindows)})
installEnv.Append(BUILDERS={"Checksum" : Builder(action=Common.Checksum)})

buildOptions = ['msbuild', envBase['SLN'], '/restore:' + str(envBase['RESTORE'])]

if (envBase['NO_DOTNET']):
    buildOptions += ['/p:NoDotNet=true']

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
    with (io.open(csproj[1], 'r', encoding="UTF-8")) as inFile:
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
installTarget = installEnv.BuildInstall(
    target = os.path.join(envBase['INSTALL_DIR'], 'windows', 'bin', 'x64', 'Release', 'ChaskisInstaller.msi'),
    source = buildTargets['RELEASE']
)

buildTargets['INSTALL'] = installEnv.Checksum(
    target = os.path.join(envBase['INSTALL_DIR'], 'windows', 'bin', 'x64', 'Release', 'ChaskisInstaller.msi.sha256'),
    source = installTarget
)

if (envBase['SAVE_CHECKSUM']):
    buildTargets['INSTALL'] = envBase.Command(
        target = os.path.join(envBase['SAVED_CHECKSUM_DIR'], 'ChaskisInstaller.msi.sha256'),
        source = buildTargets['INSTALL'],
        action = Copy('$TARGET', '$SOURCE')
    )

    envBase.NoClean(buildTargets['INSTALL'])

addCleanTargets(buildTargets['DEBUG'], debugTargets)
addCleanTargets(buildTargets['RELEASE'], releaseTargets)
addCleanTargets(buildTargets['INSTALL'], installTargets)

Return('buildTargets')