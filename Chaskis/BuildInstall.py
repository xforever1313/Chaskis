#!/usr/bin/python

'''
This script is a quick way to compile
and install Chaskis to the correct directory for the current user.

This is NOT a way to install on a system-wide basis. Its just a simple script I use
since I am lazy.
'''

import os
import platform
import shutil
import subprocess

###
# Compile
###

if (platform.system() == "Windows"):
    isWindows = True
    msBuild = "MSBuild.exe"
else:
    isWindows = False
    msBuild = "xbuild"

# First, restore any nuget projects, if not windows.
if (isWindows == False):
    returnCode = subprocess.call(["nuget", "restore", "Chaskis.sln"])
    if (returnCode != 0):
        raise Exception("Could not restore nuget packages...")

returnCode = subprocess.call([msBuild, "/p:Configuration=Release", "Chaskis.sln"])
if (returnCode != 0):
    raise Exception("Could not compile Chaskis.")

###
# Install
###

def CopyIfNotExist(sourceFile, destFile):
    if (os.path.exists(destFile) == False):
        Copy(sourceFile, destFile)
    else:
        print(destFile + " already exists, skipping copy.")

def Copy(sourceFile, destFile):
    print("Copying " + sourceFile + " to " + destFile)
    shutil.copy2(sourceFile, destFile)

def CreateDirIfNotExist(dir):
    if (os.path.exists(dir) == False):
        print("Creating Directory: " + dir)
        os.mkdir(dir)

if (isWindows):
    rootDir = os.getenv('APPDATA')
else:
    rootDir = os.path.join(os.path.expanduser("~"), ".config")

rootDir = os.path.join(rootDir, "Chaskis")
CreateDirIfNotExist(rootDir)

binDir = os.path.join(rootDir, "bin")
CreateDirIfNotExist(binDir)

pluginDir = os.path.join(rootDir, "Plugins")
CreateDirIfNotExist(pluginDir)

# Copy main binaries
mainBinDir = os.path.join("ChaskisService", "bin", "Release")

Copy(os.path.join(mainBinDir, "Chaskis.exe"), binDir)
Copy(os.path.join(mainBinDir, "ChaskisService.exe"), binDir)
Copy(os.path.join(mainBinDir, "GenericIrcBot.dll"), binDir)

# Copy Config
configDir = os.path.join("Chaskis", "Config")
CopyIfNotExist(os.path.join(configDir, "SampleIrcConfig.xml"), os.path.join(rootDir, "IrcConfig.xml"))
CopyIfNotExist(os.path.join(configDir, "SamplePluginConfig.xml"), os.path.join(rootDir, "PluginConfig.xml"))

# Copy Plugins
for localPluginDir in os.listdir("Plugins"):
    pluginName = localPluginDir
    localPluginDir = os.path.join("Plugins", localPluginDir)
    if (os.path.isdir(localPluginDir)):

        installPluginDir = os.path.join(pluginDir, pluginName)
        CreateDirIfNotExist(installPluginDir)

        # Copy binaries.
        localBinDir = os.path.join(localPluginDir, "bin", "Release")
        for binaryFile in os.listdir(localBinDir):
            if (binaryFile.endswith(".dll") and binaryFile != "GenericIrcBot.dll"):
                Copy(os.path.join(localBinDir, binaryFile), installPluginDir)

            elif (binaryFile.endswith("x86") or binaryFile.endswith("x64")):
                CreateDirIfNotExist(os.path.join(installPluginDir, binaryFile))
                for innerBinaryFile in os.listdir(os.path.join(localBinDir, binaryFile)):
                    if (innerBinaryFile.endswith(".dll")):
                        Copy(os.path.join(localBinDir, binaryFile, innerBinaryFile), os.path.join(installPluginDir, binaryFile, innerBinaryFile))

        # Copy Config (if any)
        localConfigDir = os.path.join(localPluginDir, "Config")
        if (os.path.exists(localConfigDir)):
            for configFile in os.listdir(localConfigDir):
                if (configFile.startswith("Sample")):
                    CopyIfNotExist(os.path.join(localConfigDir, configFile), os.path.join(installPluginDir, configFile[6:]))
                else:
                    CopyIfNotExist(os.path.join(localConfigDir, configFile), installPluginDir)
