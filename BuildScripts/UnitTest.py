from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import glob
import platform
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

reportGeneratorPath = os.path.join(
    testEnv['PACKAGES'],
    'ReportGenerator',
    'tools',
    'net47',
    'ReportGenerator.exe'
)

codeCoverageDir = os.path.join(envBase['REPO_ROOT'], 'CodeCoverage')

def RunUnitTest(target, source, env):
    args = [nunitExePath]
    targetStr = str(target[0])
    args += ['/result:' + targetStr]

    for s in source:
        args += [str(s)]

    if(any(platform.win32_ver()) == False):
        args = ['mono'] + args

    return subprocess.call(args)

def RunUnitTestWithCoverage(target, source, env):
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

    return subprocess.call(args)

def GenerateCoverageInfo(target, source, env):
    index_html = str(target[0])

    args = [reportGeneratorPath]

    args += ['-reports:' + str(source[1])]
    args += ['-targetdir:' + codeCoverageDir]

    return subprocess.call(args)

testEnv.Append(BUILDERS={"UnitTest" : Builder(action=RunUnitTest)})
testEnv.Append(BUILDERS={"TestCoverage" : Builder(action=RunUnitTestWithCoverage)})
testEnv.Append(BUILDERS={"CoverageReport" : Builder(action=GenerateCoverageInfo)})

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
    coverageTarget = testEnv.TestCoverage(
        target = targets,
        source=sources
    )

    AlwaysBuild(coverageTarget)

    unitTestTarget = testEnv.CoverageReport(
        target=os.path.join(codeCoverageDir, 'index.html'),
        source=coverageTarget
    )
else:
    unitTestTarget = testEnv.UnitTest(
        target=os.path.join(envBase['REPO_ROOT'], 'TestResult', 'TestResult.xml'),
        source=sources
    )

    AlwaysBuild(unitTestTarget)

Return('unitTestTarget')