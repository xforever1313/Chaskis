import glob
import os
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

###
# Arguments
###

code_coverage = ARGUMENTS.get('codecoverage', '0') == '1'
restore = ARGUMENTS.get('no_restore', '0') != '1'

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

Arguments:
codecoverage - Set to '1' if you want to run code coverage with ReportGenerator.  WINDOWS ONLY!
no_restore - Set to '1' to skip the restore step while building.
'''
)

###
# Environments
###
envBase = Environment()
envBase['ENV']['PATH']=os.environ['PATH'] # Look in path for tools
envBase['REPO_ROOT'] = os.path.join(WORKING_DIRECTORY)
envBase['SLN_DIR'] = os.path.join(envBase["REPO_ROOT"], 'Chaskis')
envBase['SLN'] = os.path.join(envBase["SLN_DIR"], 'Chaskis.sln')
envBase['PACKAGES'] = os.path.join(envBase["REPO_ROOT"], "packages")
envBase['CHASKIS_EXE_DIR'] = os.path.join(envBase['SLN_DIR'], 'Chaskis')
envBase['CHASKIS_CORE_DIR'] = os.path.join(envBase['SLN_DIR'], 'ChaskisCore')
envBase['PLUGINS_DIR'] = os.path.join(envBase['SLN_DIR'], 'Plugins')
envBase['UNIT_TESTS_DIR'] = os.path.join(envBase['SLN_DIR'], 'UnitTests')
envBase['REGRESSION_TEST_DIR'] = os.path.join(envBase['SLN_DIR'], 'RegressionTests')
envBase['INSTALL_DIR'] = os.path.join(envBase['SLN_DIR'], 'Install')
envBase['PLUGIN_RUNTIME'] = DEFAULT_PLUGIN_RUNTIME
envBase['EXE_RUNTIME'] = DEFAULT_EXE_RUNTIME
envBase['CODE_COVERAGE'] = code_coverage
envBase['RESTORE'] = restore

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

unitTestTargets = SConscript(
    os.path.join(BUILD_SCRIPTS_DIR, 'UnitTest.py'),
    exports='envBase'
)

# Unit tests depends on Nuget packages to be installed
# and the Debug build to be there.
Depends(unitTestTargets, nugetTarget)
Depends(unitTestTargets, buildTargets['DEBUG'])

###
# Aliases
###

Alias('nuget', nugetTarget)
Alias('template', templateTarget)
Alias('debug', buildTargets['DEBUG'])
Alias('release', buildTargets['RELEASE'])
Alias('install', buildTargets['INSTALL'])
Alias('unit_test', unitTestTargets)