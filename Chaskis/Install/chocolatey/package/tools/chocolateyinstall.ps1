
$ErrorActionPreference = 'Stop';

$packageName= $env:ChocolateyPackageName
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = ''
$url64      = 'https://files.shendrick.net/projects/chaskis/releases/0.3.0/ChaskisInstaller.msi'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'msi'
  url           = $url
  url64bit      = $url64

  softwareName  = 'chaskis*'

  checksum      = ''
  checksumType  = 'sha256'
  checksum64    = '932009E195FECAB98FC39C81178D9CD3E6E4C0380CF38D2F6DF3B22B08F5E9B4'
  checksumType64= 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs

