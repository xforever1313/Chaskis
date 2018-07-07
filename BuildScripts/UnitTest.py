from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import glob
import os
import subprocess

Import('envBase')

testEnv = envBase.Clone()
exePath = os.path.join( testEnv["PACKAGES"], 'NUnit.ConsoleRunner', 'tools', 'nunit3-console.exe' )

def RunUnitTest(target, source, env):
    args = [exePath]
    targetStr = str(target[0])
    args += ['/result:' + targetStr]

    for s in source:
        args += [str(s)]

    return subprocess.call(args)

testEnv.Append(BUILDERS={"UnitTest" : Builder(action=RunUnitTest)})

# Get all .csproj files in the Unit Test folder
csprojs = glob.glob(os.path.join(testEnv['UNIT_TESTS_DIR'], '*', '*.csproj'))

sources = []
for csproj in csprojs:
    sources += [
        os.path.join(
            os.path.dirname(csproj),
            'bin',
            'Debug',
            envBase['EXE_RUNTIME'],
            os.path.basename(csproj).replace('csproj', 'dll')
        )
    ]

unitTestTarget = testEnv.UnitTest(
    target=os.path.join(envBase['REPO_ROOT'], 'TestResult', 'TestResult.xml'),
    source=sources
)

AlwaysBuild(unitTestTarget)

Return('unitTestTarget')