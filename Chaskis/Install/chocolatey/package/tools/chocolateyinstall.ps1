
$ErrorActionPreference = 'Stop';

$packageName= $env:ChocolateyPackageName
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = ''
$url64      = 'https://files.shendrick.net/projects/chaskis/releases/0.6.1/windows/ChaskisInstaller.msi'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'msi'
  url           = $url
  url64bit      = $url64

  softwareName  = 'chaskis*'

  checksum      = ''
  checksumType  = 'sha256'
  checksum64    = 'F89F1F263B652BEB7450EA5B10E81F0E0E9F45399C7060A326D3FDA9C5993FCF'
  checksumType64= 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs

