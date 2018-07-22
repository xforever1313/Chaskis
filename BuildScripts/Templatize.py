from SCons.Script import *
from SCons.Environment import *
from SCons.Builder import *

import io
import os
import re
import subprocess

Import('envBase')
tempEnv = envBase.Clone()

###
# Helper Functions and Variables
###

chaskis_cs = os.path.join(
    tempEnv['CHASKIS_EXE_DIR'],
    'Chaskis.cs'
)

ircbot_cs = os.path.join(
    tempEnv['CHASKIS_CORE_DIR'],
    'IrcBot.cs'
)

license_txt = os.path.join(
    tempEnv['REPO_ROOT'],
    'LICENSE_1_0.txt'
)

regression_test_plugin_cs = os.path.join(
    tempEnv['REGRESSION_TEST_DIR'],
    'RegressionTestPlugin',
    'RegressionTestPlugin.cs'
)

default_handlers_cs = os.path.join(
    tempEnv['CHASKIS_EXE_DIR'],
    'DefaultHandlers.cs'
)

core_assembly_info_cs = os.path.join(
    tempEnv['CHASKIS_CORE_DIR'],
    'Properties',
    'AssemblyInfo.cs'
)

debChecksumFile = os.path.join(
    tempEnv['SAVED_CHECKSUM_DIR'],
    'chaskis.deb.sha256'
)

msiChecksumFile = os.path.join(
    tempEnv['SAVED_CHECKSUM_DIR'],
    'ChaskisInstaller.msi.sha256'
)

def GetMainVersion():
    fileName = chaskis_cs
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()

    pattern = r'public\s+const\s+string\s+VersionStr\s+=\s+"(?P<version>\d+\.\d+\.\d+)";'

    match = re.search(pattern, contents)
    return match.group('version')

def GetCoreVersion():
    fileName = ircbot_cs
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()

    pattern = r'public\s+const\s+string\s+VersionString\s+=\s+"(?P<version>\d+\.\d+\.\d+)";'

    match = re.search(pattern, contents)
    return match.group('version')

def GetLicense():
    fileName = license_txt
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()
    
    return contents

def GetRegressionTestVersion():
    fileName = regression_test_plugin_cs
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()

    pattern = r'public\s+const\s+string\s+VersionStr\s+=\s+"(?P<version>\d+\.\d+\.\d+)";'

    match = re.search(pattern, contents)
    return match.group('version')

def GetRegressionTestPluginName():
    fileName = regression_test_plugin_cs
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()

    pattern = r'\[ChaskisPlugin\( "(?P<name>\w+)" \)\]'

    match = re.search(pattern, contents)
    return match.group('name')

def GetDefaultPluginName():
    fileName = default_handlers_cs
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()

    pattern = r'public\s+const\s+string\s+DefaultPluginName\s*=\s*"(?P<name>\w+)"'

    match = re.search(pattern, contents)
    return match.group('name')

def GetCopyRight():
    fileName = ircbot_cs
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()

    pattern = r'public\s+const\s+string\s+CopyRight\s+=\s+"(?P<copyright>.+)";'

    match = re.search(pattern, contents)
    return match.group('copyright')

def GetDescription():
    fileName = core_assembly_info_cs
    with io.open(fileName, 'r', encoding="utf8") as inFile:
        contents = inFile.read()

    pattern = r'(?ms)(\[assembly:\s+AssemblyDescription\(\s*@"(?P<description>.+)"\s*\)\]\s+//\s+End\s+Description)'

    match = re.search(pattern, contents)
    return match.group('description')

def GetDebChecksum():
    fileName = debChecksumFile
    with io.open(fileName, 'r', encoding='utf-8') as inFile:
        for line in inFile:
            checksum = line.strip()
            break
        return checksum

def GetMsiChecksum():
    fileName = msiChecksumFile
    with io.open(fileName, 'r', encoding='utf-8') as inFile:
        for line in inFile:
            checksum = line.strip()
            break
        return checksum

###
# Template List:
###

class Template:
    def __init__(self, source, target, defines):
        self.Target = target
        self.Source = source
        self.Defines = defines

templates = []

product_wxs = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'windows', 'Product.wxs.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'windows', 'Product.wxs'),
    ['WINDOWS']
)
templates += [product_wxs]

product_wxs_linux = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'windows', 'Product.wxs.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'windows', 'Product.wxs.linux'),
    ['LINUX']
)
templates += [product_wxs_linux]

pkgbuild = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'linux', 'arch', 'PKGBUILD.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'linux', 'arch', 'PKGBUILD'),
    []
)
templates += [pkgbuild]

debian_control = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'linux', 'debian', 'control.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'linux', 'debian', 'control'),
    []
)
templates += [debian_control]

fedora_spec = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'linux', 'fedora', 'chaskis.spec.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'linux', 'fedora', 'chaskis.spec'),
    []
)
templates += [fedora_spec]

chocolatey_nuspec = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'chocolatey', 'template', 'chaskis.nuspec.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'chocolatey', 'package', 'chaskis.nuspec'),
    []
)
templates += [chocolatey_nuspec]

chocolatey_license = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'chocolatey', 'template', 'tools', 'LICENSE.txt.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'chocolatey', 'package', 'tools', 'LICENSE.txt'),
    []
)
templates += [chocolatey_license]

chocolatey_install = Template(
    os.path.join(tempEnv['INSTALL_DIR'], 'chocolatey', 'template', 'tools', 'chocolateyinstall.ps1.template'),
    os.path.join(tempEnv['INSTALL_DIR'], 'chocolatey', 'package', 'tools', 'chocolateyinstall.ps1'),
    []
)
templates += [chocolatey_install]

readme = Template(
    os.path.join(tempEnv['REPO_ROOT'], 'README.md.template'),
    os.path.join(tempEnv['REPO_ROOT'], 'README.md'),
    []
)
templates += [readme]

core_nuspec = Template(
    os.path.join(tempEnv['CHASKIS_CORE_DIR'], 'Chaskis.Core.nuspec.template'),
    os.path.join(tempEnv['CHASKIS_CORE_DIR'], 'Chaskis.Core.nuspec'),
    []
)
templates += [core_nuspec]

regression_test_main_page = Template(
    os.path.join(tempEnv['REGRESSION_TEST_DIR'], 'FitNesseRoot', 'ChaskisTests', 'content.txt.template'),
    os.path.join(tempEnv['REGRESSION_TEST_DIR'], 'FitNesseRoot', 'ChaskisTests', 'content.txt'),
    []
)
templates += [regression_test_main_page]

new_version_notifier_test = Template(
    os.path.join(tempEnv['REGRESSION_TEST_DIR'], 'Environments', 'NewVersionNotifierNoChangeEnvironment', 'Plugins', 'NewVersionNotifier', '.lastversion.txt.template'),
    os.path.join(tempEnv['REGRESSION_TEST_DIR'], 'Environments', 'NewVersionNotifierNoChangeEnvironment', 'Plugins', 'NewVersionNotifier', '.lastversion.txt'),
    []
)
templates += [new_version_notifier_test]

###
# Builders
###
def Templatize(target, source, env):

    # Constants.  Put these inside of the builder function
    # such that we don't read files everytime the SConstruct runs.
    FullName = "Chaskis IRC Bot"

    ChaskisMainVersion = GetMainVersion()
    ChaskisCoreVersion = GetCoreVersion()
    License = GetLicense()
    RegressionTestPluginVersion = GetRegressionTestVersion()
    RegressionTestPluginName = GetRegressionTestPluginName()
    DefaultPluginName = GetDefaultPluginName()

    Tags = "chaskis irc bot framework plugin seth hendrick xforever1313 mono linux windows"
    CoreTags = Tags + " core"
    MainTags = Tags + " service full installer admin"
    Author = "Seth Hendrick"
    AuthorEmail = "seth@shendrick.net"
    ProjectUrl = "https://github.com/xforever1313/Chaskis/"
    LicenseUrl = "https://github.com/xforever1313/Chaskis/blob/master/LICENSE_1_0.txt"
    WikiUrl = "https://github.com/xforever1313/Chaskis/wiki"
    IssueTrackerUrl="https://github.com/xforever1313/Chaskis/issues"
    CopyRight = GetCopyRight()
    Description = GetDescription()
    ReleaseNotes = "View release notes here: [https://github.com/xforever1313/Chaskis/releases](https://github.com/xforever1313/Chaskis/releases)"
    Summary = "A generic framework written in C# for making IRC Bots."
    IconUrl = "https://files.shendrick.net/projects/chaskis/assets/icon.png"
    RunTime = tempEnv["EXE_RUNTIME"]
    debCheckSum = GetDebChecksum()
    msiCheckSum = GetMsiChecksum()

    for template in templates:
        with io.open(template.Source, 'r', encoding="utf8") as inFile:
            try:
                if (len(template.Defines) == 0):
                    # If we have no defines, just grab the whole file.
                    contents = inFile.read()
                else:
                    contents = ""
                    # Otherwise, we need to be smart.   If something is defined,
                    # include the text between the #IF and the #ENDIF.  Otherwise, move on.
                    # This is bascially a terrible version of the C PreProcessor since WIX doesn't
                    # like extra attributes.
                    # This crappy version doesn't currently support nested if statements.
                    ifRegexes = []
                    for define in template.Defines:
                        ifRegexes += [re.compile(r'\s*#[iI][fF]\s+' + define)]

                    badRegex = re.compile(r'\s*#[iI][fF]\s+\w+')

                    endRegex = re.compile(r'\s*#[eE][nN][dD][iI][fF]')

                    addLine = True
                    for line in inFile:
                        foundIfRegex = False
                        for r in ifRegexes:
                            if (bool(r.search(line))):
                                foundIfRegex = True
                                break

                        if (foundIfRegex):
                            addLine = True
                            continue

                        elif (bool(badRegex.search(line))):
                            addLine = False
                            continue

                        elif (bool(endRegex.search(line))):
                            addLine = True
                            continue
                        
                        elif(addLine):
                            contents += line
            except:
                print ("Error when reading from " + template.Source)
                raise

        contents = re.sub(r'{%FullName%}', FullName, contents)
        contents = re.sub(r'{%ChaskisMainVersion%}', ChaskisMainVersion, contents)
        contents = re.sub(r'{%ChaskisCoreVersion%}', ChaskisCoreVersion, contents)
        contents = re.sub(r'{%License%}', License, contents)
        contents = re.sub(r'{%RegressionTestPluginVersion%}', RegressionTestPluginVersion, contents)
        contents = re.sub(r'{%RegressionTestPluginName%}', RegressionTestPluginName, contents)
        contents = re.sub(r'{%DefaultPluginName%}', DefaultPluginName, contents)
        contents = re.sub(r'{%CoreTags%}', CoreTags, contents)
        contents = re.sub(r'{%MainTags%}', MainTags, contents)
        contents = re.sub(r'{%Author%}', Author, contents)
        contents = re.sub(r'{%AuthorEmail%}', AuthorEmail, contents)
        contents = re.sub(r'{%ProjectUrl%}', ProjectUrl, contents)
        contents = re.sub(r'{%LicenseUrl%}', LicenseUrl, contents)
        contents = re.sub(r'{%WikiUrl%}', WikiUrl, contents)
        contents = re.sub(r'{%IssueTrackerUrl%}', IssueTrackerUrl, contents)
        contents = re.sub(r'{%CopyRight%}', CopyRight, contents)
        contents = re.sub(r'{%Description%}', Description, contents)
        contents = re.sub(r'{%ReleaseNotes%}', ReleaseNotes, contents)
        contents = re.sub(r'{%Summary%}', Summary, contents)
        contents = re.sub(r'{%IconUrl%}', IconUrl, contents)
        contents = re.sub(r'{%RunTime%}', RunTime, contents)
        contents = re.sub(r'{%DebCheckSum%}', debCheckSum, contents)
        contents = re.sub(r'{%MsiCheckSum%}', msiCheckSum, contents)

        with io.open(template.Target, 'w', encoding="utf8") as outFile:
            try:
                outFile.write(contents)
            except:
                print ("Error when writing to " + template.Target)
                raise
        
tempEnv.Append(BUILDERS={'Template' : Builder(action=Templatize)})

###
# Target Creation
###

# Sources where we get information from.
infoSources = [
    chaskis_cs,
    ircbot_cs,
    license_txt,
    regression_test_plugin_cs,
    default_handlers_cs,
    core_assembly_info_cs,
    debChecksumFile,
    msiChecksumFile
]

targets = []
sources = []
for template in templates:
    targets += [template.Target]
    sources += [template.Source]

sources += infoSources

templateTarget = tempEnv.Template(
    target = targets,
    source = sources
)

for target in targets:
    tempEnv.NoClean(target)

Return('templateTarget')