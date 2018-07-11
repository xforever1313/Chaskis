import glob
import os
import platform
import re
import shutil
import subprocess
import sys

###
# Constants
###

WORKING_DIRECTORY = os.getcwd()
BUILD_SCRIPTS_DIR = os.path.join(WORKING_DIRECTORY, 'BuildScripts')
DEFAULT_PLUGIN_RUNTIME = "netstandard2.0"
DEFAULT_EXE_RUNTIME = "net471"
DEFAULT_PORT=10013

###
# Arguments
###

code_coverage = ARGUMENTS.get('codecoverage', '0') == '1'
restore = ARGUMENTS.get('no_restore', '0') != '1'
port = ARGUMENTS.get('port', DEFAULT_PORT)
save_checksum = ARGUMENTS.get('save_checksum', '0') == '1'

###
# Help
###

Help(
'''
Targets:
nuget - Installs nuget packages that are not installed during the restore process.
template - Runs the script the templates various files.
debug - Build debug target (ANY CPU)
release - Build release target (ANY CPU)
install - Build install target.  If on Windows, this compiles with WIX.  If on Linux, this builds the .deb.
run_test - Runs the unit tests.
regression_test - Runs the regression tests
pkgbuild - Generates the Arch Linux package.  Can only be ran on Arch Linux systems successfully.

Arguments:
codecoverage - Set to '1' if you want to run code coverage with ReportGenerator.  WINDOWS ONLY!
no_restore - Set to '1' to skip the restore step while building.
port - The port to use while running regression tests.  Defaulted to ''' + str(DEFAULT_PORT) + '''
save_checksum - Valid during install or pkgbuild targets.  Saves off the checksum of the generated file in the BuildScripts folder.
'''
)

###
# Environments
###
envBase = Environment()
envBase['ENV']['PATH']=os.environ['PATH'] # Look in path for tools
envBase['REPO_ROOT'] = os.path.join(WORKING_DIRECTORY)
envBase['SAVED_CHECKSUM_DIR'] = os.path.join(envBase['REPO_ROOT'], 'SavedChecksums')
envBase['SLN_DIR'] = os.path.join(envBase["REPO_ROOT"], 'Chaskis')
envBase['SLN'] = os.path.join(envBase["SLN_DIR"], 'Chaskis.sln')
envBase['PACKAGES'] = os.path.join(envBase["REPO_ROOT"], "packages")
envBase['CHASKIS_EXE_DIR'] = os.path.join(envBase['SLN_DIR'], 'Chaskis')
envBase['CHASKIS_CORE_DIR'] = os.path.join(envBase['SLN_DIR'], 'ChaskisCore')
envBase['PLUGINS_DIR'] = os.path.join(envBase['SLN_DIR'], 'Plugins')
envBase['UNIT_TESTS_DIR'] = os.path.join(envBase['SLN_DIR'], 'UnitTests')
envBase['REGRESSION_TEST_DIR'] = os.path.join(envBase['SLN_DIR'], 'RegressionTests')
envBase['INSTALL_DIR'] = os.path.join(envBase['SLN_DIR'], 'Install')
envBase['LINUX_INSTALL_DIR'] = os.path.join(envBase['INSTALL_DIR'], 'linux')
envBase['DEBIAN_INSTALL_DIR'] = os.path.join(envBase['LINUX_INSTALL_DIR'], 'debian')
envBase['ARCH_INSTALL_DIR'] = os.path.join(envBase['LINUX_INSTALL_DIR'], 'arch')
envBase['CLI_INSTALL_DIR'] = os.path.join(envBase['INSTALL_DIR'], 'ChaskisCliInstaller')
envBase['PLUGIN_RUNTIME'] = DEFAULT_PLUGIN_RUNTIME
envBase['EXE_RUNTIME'] = DEFAULT_EXE_RUNTIME
envBase['CODE_COVERAGE'] = code_coverage
envBase['RESTORE'] = restore
envBase['PORT'] = port
envBase['SAVE_CHECKSUM'] = save_checksum

###
# SConscripts
###

nugetTarget = SConscript(
    os.path.join(BUILD_SCRIPTS_DIR, "Nuget.py"),
    exports='envBase'
)

templateTarget = SConscript(
    os.path.join(BUILD_SCRIPTS_DIR, 'Templatize.py'),
    exports='envBase'
)

buildTargets= SConscript(
    os.path.join(BUILD_SCRIPTS_DIR, "Build.py"),
    exports='envBase'
)

if (any(platform.win32_ver())):
    # On Windows, we can just use the install target.
    installTarget = buildTargets['INSTALL']
else:
    # On Linux, we need to generate the .deb file, which requires
    # Extra steps.
    installTargets = SConscript(
        os.path.join(BUILD_SCRIPTS_DIR, 'Deb.py'),
        exports='envBase'
    )

    #.deb file depends on the release target to be built first.
    Depends(installTargets['CLI_TARGET'], buildTargets['RELEASE'])

    installTarget = installTargets['DEB_TARGET']

unitTestTargets = SConscript(
    os.path.join(BUILD_SCRIPTS_DIR, 'UnitTest.py'),
    exports='envBase'
)

# Unit tests depends on Nuget packages to be installed
# and the Debug build to be there.
Depends(unitTestTargets, nugetTarget)
Depends(unitTestTargets, buildTargets['DEBUG'])

regressionTestTargets = SConscript(
    os.path.join(envBase['REGRESSION_TEST_DIR'], 'SConscript'),
    exports='envBase'
)

launchFitnesseTarget = regressionTestTargets['LAUNCH_FITNESSE']
regressionTestTarget = regressionTestTargets['REGRESSION_TEST']

# Regression tests depends on Nuget packages to be installed
# and the Debug build to be there.
Depends(regressionTestTargets['BOOTSTRAP'], nugetTarget)
Depends(regressionTestTargets['DIST'], buildTargets['DEBUG'])

pkgBuildTarget = SConscript(
    os.path.join(BUILD_SCRIPTS_DIR, 'Pkgbuild.py'),
    exports='envBase'
)

###
# Aliases
###
Alias('nuget', nugetTarget)
Alias('template', templateTarget)
Alias('debug', buildTargets['DEBUG'])
Alias('release', buildTargets['RELEASE'])
Alias('install', installTarget)
Alias('unit_test', unitTestTargets)
Alias('launch_fitnesse', launchFitnesseTarget)
Alias('regression_test', regressionTestTarget)
Alias('pkgbuild', pkgBuildTarget)