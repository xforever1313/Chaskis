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

plugin_runtime = ARGUMENTS.get('plugin_runtime', DEFAULT_PLUGIN_RUNTIME)
exe_runtime = ARGUMENTS.get('exe_runtime', DEFAULT_EXE_RUNTIME)
code_coverage = ARGUMENTS.get('codecoverage', '0')

###
# Help
###

Help(
'''
Targets:
nuget - Installs nuget packages that are not installed during the restore process.
template - Runs the script the templates various files (Always Builds)
submodule - Runs "submodule update --init" on the submodules.
debug - Build debug target (ANY CPU)
release - Build release target (ANY CPU)
install - Build install target.  If on Windows, this compiles with WIX.  If on Linux, this builds the .deb.
run_test - Runs the unit tests.
regression_test - Runs the regression tests

Arguments:
plugin_runtime - The plugin runtime we are going to target.  Defaulted to ''' + DEFAULT_PLUGIN_RUNTIME + '''
exe_runtime - The exe runtime we are going to target.  Defaulted to ''' + DEFAULT_EXE_RUNTIME + '''
codecoverage - Set to '1' if you want to run code coverage with ReportGenerator.  WINDOWS ONLY!
'''
)

###
# Environments
###
envBase = Environment()
envBase['ENV']['PATH']=os.environ['PATH'] # Look in path for tools
envBase["REPO_ROOT"] = os.path.join(WORKING_DIRECTORY)
envBase["SLN_DIR"] = os.path.join(envBase["REPO_ROOT"], 'Chaskis')

###
# SConscripts
###

nugetTarget = SConscript(
    os.path.join(BUILD_SCRIPTS_DIR, "Nuget.py"),
    exports='envBase'
)

Alias('nuget', nugetTarget)