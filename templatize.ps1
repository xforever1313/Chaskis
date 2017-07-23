# This file reads in varables from various locations, and outputs them in other various locations.
# Useful so there is only 1 and only 1 location for each of these variables.

###
# Grab version of Chaskis.exe.
###
function GetMainVersion()
{
    # Get-Content gets an array of strings... [String] puts them together as one big string.
    [String]$chaskisMainFile = Get-Content ".\Chaskis\Chaskis\Chaskis.cs"
    $success = $chaskisMainFile -match 'public\s+const\s+string\s+VersionStr\s+=\s+"(?<version>\d+\.\d+\.\d+)";'
    return $matches["version"]
}


###
# Grab version of Core.
###
function GetCoreVersion()
{
    # Get-Content gets an array of strings... [String] puts them together as one big string.
    [String]$chaskisCoreMainFile = Get-Content ".\Chaskis\ChaskisCore\IrcBot.cs"
    $success = $chaskisCoreMainFile -match 'public\s+const\s+string\s+VersionString\s+=\s+"(?<version>\d+\.\d+\.\d+)";'
    return $matches["version"]
}

###
# Gets copyright information.
###
function GetCopyRight()
{
    # Get-Content gets an array of strings... [String] puts them together as one big string.
    [String]$chaskisCoreMainFile = Get-Content ".\Chaskis\ChaskisCore\IrcBot.cs"
    $success = $chaskisCoreMainFile -match 'public\s+const\s+string\s+CopyRight\s+=\s+"(?<copyright>.+)";'
    return $matches["copyright"]
}

function GetDescription()
{
    $coreAssemblyFile = Get-Content ".\Chaskis\ChaskisCore\Properties\AssemblyInfo.cs" | Out-String
    $success = $coreAssemblyFile -match '(?ms)(\[assembly:\s+AssemblyDescription\(\s*@"(?<description>.+)"\s*\)\]\s+//\s+End\s+Description)'
    return $matches["description"]
}

###
# Variables
###

$FullName = "Chaskis IRC Bot"

$ChaskisMainVersion = GetMainVersion
$ChaskisCoreVersion = GetCoreVersion
$License =  Get-Content ".\LICENSE_1_0.txt" | Out-String

$Tags = "chaskis irc bot framework plugin seth hendrick xforever1313 mono linux windows"
$CoreTags = $Tags + " core"
$MainTags = $Tags + " service full installer admin"
$Author = "Seth Hendrick"
$AuthorEmail = "seth@shendrick.net"
$ProjectUrl = "https://github.com/xforever1313/Chaskis/"
$LicenseUrl = "https://github.com/xforever1313/Chaskis/blob/master/LICENSE_1_0.txt"
$WikiUrl = "https://github.com/xforever1313/Chaskis/wiki"
$IssueTrackerUrl="https://github.com/xforever1313/Chaskis/issues"
$CopyRight = GetCopyRight
$Description = GetDescription
$ReleaseNotes = "View release notes here: [https://github.com/xforever1313/Chaskis/releases](https://github.com/xforever1313/Chaskis/releases)"
$Summary = "A generic framework written in C# for making IRC Bots."

$FilesToTemplate =(
    (".\Chaskis\Install\windows\Product.wxs.template", ".\Chaskis\Install\windows\Product.wxs"),
    (".\Chaskis\Install\linux\PKGBUILD.template", ".\Chaskis\Install\linux\PKGBUILD"),
    (".\Chaskis\Install\chocolatey\template\chaskis.nuspec.template", ".\Chaskis\Install\chocolatey\package\chaskis.nuspec"),
    (".\Chaskis\Install\chocolatey\template\tools\LICENSE.txt.template", ".\Chaskis\Install\chocolatey\package\tools\LICENSE.txt"),
    (".\README.md.template", ".\README.md"),
    (".\Chaskis\ChaskisCore\ChaskisCore.nuspec.template", ".\Chaskis\ChaskisCore\ChaskisCore.nuspec"),
    (".\Chaskis\Install\chocolatey\template\tools\chocolateyinstall.ps1.template", ".\Chaskis\Install\chocolatey\package\tools\chocolateyinstall.ps1")
)

function TemplateFile($filePath, $Output)
{
    $inputContents = Get-Content $filePath | Out-String
    $inputContents = $inputContents.replace("{%FullName%}", $FullName)
    $inputContents = $inputContents.replace("{%ChaskisMainVersion%}", $ChaskisMainVersion)
    $inputContents = $inputContents.replace("{%ChaskisCoreVersion%}", $ChaskisCoreVersion)
    $inputContents = $inputContents.replace("{%License%}", $License)
    $inputContents = $inputContents.replace("{%CoreTags%}", $CoreTags)
    $inputContents = $inputContents.replace("{%MainTags%}", $MainTags)
    $inputContents = $inputContents.replace("{%Author%}", $Author)
    $inputContents = $inputContents.replace("{%AuthorEmail%}", $AuthorEmail)
    $inputContents = $inputContents.replace("{%ProjectUrl%}", $ProjectUrl)
    $inputContents = $inputContents.replace("{%LicenseUrl%}", $LicenseUrl)
    $inputContents = $inputContents.replace("{%WikiUrl%}", $WikiUrl)
    $inputContents = $inputContents.replace("{%IssueTrackerUrl%}", $IssueTrackerUrl)
    $inputContents = $inputContents.replace("{%CopyRight%}", $CopyRight)
    $inputContents = $inputContents.replace("{%Description%}", $Description)
    $inputContents = $inputContents.replace("{%ReleaseNotes%}", $ReleaseNotes)
    $inputContents = $inputContents.replace("{%Summary%}", $Summary)
    
    $inputContents | Set-Content $Output
}

foreach($file in $FilesToTemplate)
{
    TemplateFile $file[0] $file[1]
}