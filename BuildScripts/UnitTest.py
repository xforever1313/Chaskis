from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import glob
import os
import subprocess

Import('envBase')

testEnv = envBase.Clone()
nunitExePath = os.path.join(
    testEnv["PACKAGES"],
    'NUnit.ConsoleRunner',
    'tools',
    'nunit3-console.exe'
)

openCoverPath = os.path.join(
    testEnv['PACKAGES'],
    'OpenCover',
    'tools',
    'OpenCover.Console.exe'
)

codeCoverageDir = os.path.join(envBase['REPO_ROOT'], 'CodeCoverage')

def RunUnitTest(target, source, env):
    args = [nunitExePath]
    targetStr = str(target[0])
    args += ['/result:' + targetStr]

    for s in source:
        args += [str(s)]

    return subprocess.call(args)

def RunUnitTestWithCoverage(target, source, env):
    raise Exception("OpenCover doesn't work with protable pdbs yet.  Can't Execute :(")
    Execute(
        Delete(
            glob.glob( os.path.join( codeCoverageDir, '*' ) )
        )
    )

    nunitTarget = str(target[0])
    openCoverTarget = str(target[1])

    args = [openCoverPath]
    args += ['-register:user']
    args += ['-filter:+[*]Chaskis*'] # Search for all namespaces with Chaskis.
    args += ['-returntargetcode'] # Will return whatever nunit returns.
    
    searchDirArgs = '-searchdirs:'
    for s in source:
        searchDirArgs += os.path.dirname(str(s)) + ';'

    args += [searchDirArgs.strip()]
    args += ['-target:' + nunitExePath]

    targetArgs = '-targetargs:/result:' + nunitTarget
    for s in source:
        targetArgs += ' ' + str(s)

    args += [targetArgs]

    args += ['-output:' + openCoverTarget]

    print (os.getcwd())
    print (args)
    return subprocess.call(args)

testEnv.Append(BUILDERS={"UnitTest" : Builder(action=RunUnitTest)})
testEnv.Append(BUILDERS={"TestCoverage" : Builder(action=RunUnitTestWithCoverage)})

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

if (envBase['CODE_COVERAGE']):
    targets = [
        os.path.join(envBase['REPO_ROOT'], 'TestResult', 'TestResult.xml'),
        os.path.join(envBase['REPO_ROOT'], 'CodeCoverage', 'coverage.xml')
    ]
    unitTestTarget = testEnv.TestCoverage(
        target = targets,
        source=sources
    )

    # TODO: Add ReportGenerator when OpenCover's issues are fixed.
else:
    unitTestTarget = testEnv.UnitTest(
        target=os.path.join(envBase['REPO_ROOT'], 'TestResult', 'TestResult.xml'),
        source=sources
    )

AlwaysBuild(unitTestTarget)

Return('unitTestTarget')