from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import os
import subprocess

Import('envBase')
nugetEnv = envBase.Clone()

outputDir = os.path.join(nugetEnv['REPO_ROOT'], 'packages')

packageList = [
    'NUnit.ConsoleRunner',
    'OpenCover',
    'ReportGenerator',
    'NetRunner'
]

def NugetBuilder(target, source, env):
    for package in packageList:
        status = subprocess.call(['nuget', 'install', package, '-OutputDirectory', outputDir, '-ExcludeVersion' ])

        if (status != 0):
            return status

nugetEnv.Append(BUILDERS={"Nuget" : Builder(action = NugetBuilder)})

targetList = []
for package in packageList:
    targetList += [os.path.join(outputDir, package, package + '.nupkg')]

nugetTarget = nugetEnv.Nuget(
    target=targetList,
    source=[]
)

Clean(nugetTarget, outputDir)

Return('nugetTarget')