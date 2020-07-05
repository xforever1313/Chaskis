
$ErrorActionPreference = 'Stop';

$packageName= $env:ChocolateyPackageName
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = ''
$url64      = 'https://files.shendrick.net/projects/chaskis/releases/0.20.0/windows/ChaskisInstaller.msi'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'msi'
  url           = $url
  url64bit      = $url64

  softwareName  = 'chaskis*'

  checksum      = ''
  checksumType  = 'sha256'
  checksum64    = 'cdef446e1babe849bd26dcf9f265cafe61222518db3ab2d65b6ff3814772ad83'
  checksumType64= 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs