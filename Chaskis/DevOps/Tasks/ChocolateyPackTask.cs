//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.Chocolatey;
using Cake.Common.Tools.Chocolatey.Pack;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "choco_pack" )]
    [TaskDescription( "Creates the Chocolatey Package (Windows Only)." )]
    public class ChocolateyPackTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override bool ShouldRun( ChaskisContext context )
        {
            if( context.IsRunningOnWindows() == false )
            {
                context.Error( "Chocolatey can only be run on Windows" );
                return false;
            }
            else if( context.FileExists( context.Paths.MsiPath ) == false )
            {
                context.Error( "MSI does not exist.  Try building that first." );
                return false;
            }
            else if( context.FileExists( context.Paths.MsiChecksumFile ) == false )
            {
                context.Error( "MSI checksum required.  Try building the MSI first." );
                return false;
            }
            else
            {
                return true;
            }
        }

        public override void Run( ChaskisContext context )
        {
            DirectoryPath outputPath = context.Paths.OutputPackages.Combine( "chocolatey" );
            context.EnsureDirectoryExists( outputPath );
            context.CleanDirectory( outputPath );

            DirectoryPath workingPath = context.Paths.ChocolateyInstallConfigFolder.Combine( "obj" );
            context.EnsureDirectoryExists( workingPath );
            context.CleanDirectory( workingPath );

            ChocolateyPackSettings settings = GetPackSettings( context );
            settings.OutputDirectory = outputPath;
            settings.WorkingDirectory = workingPath;

            FilePath installPs1 = workingPath.CombineWithFilePath( new FilePath( "chocolateyinstall.ps1" ) );
            File.WriteAllText(
                installPs1.ToString(),
                GetInstallPs1FileContents( context )
            );

            FilePath uninstallps1 = workingPath.CombineWithFilePath( new FilePath( "chocolateyuninstall.ps1" ) );
            File.WriteAllText(
                uninstallps1.ToString(),
                GetUninstallPs1FileContents( context )
            );

            List<ChocolateyNuSpecContent> files = new List<ChocolateyNuSpecContent>();
            files.Add(
                new ChocolateyNuSpecContent
                {
                    Source = context.Paths.LicenseFile.ToString(),
                    Target = "License.txt"
                }
            );

            files.Add(
                new ChocolateyNuSpecContent
                {
                    Source = context.Paths.ChocolateyInstallConfigFolder.CombineWithFilePath( new FilePath( "VERIFICATION.txt" ) ).ToString(),
                    Target = "VERIFICATION.txt"
                }
            );

            files.Add(
                new ChocolateyNuSpecContent
                {
                    Source = context.Paths.MsiPath.ToString(),
                    Target = "tools"
                }
            );

            files.Add(
                new ChocolateyNuSpecContent
                {
                    Source = installPs1.ToString(),
                    Target = "tools"
                }
            );

            files.Add(
                new ChocolateyNuSpecContent
                {
                    Source = uninstallps1.ToString(),
                    Target = "tools"
                }
            );

            settings.Files = files;

            context.ChocolateyPack(
                settings
            );

            FilePath glob = outputPath.CombineWithFilePath( "*.nupkg" );
            foreach( FilePath file in context.GetFiles( glob.ToString() ) )
            {
                context.GenerateSha256(
                    file,
                    new FilePath( file.FullPath + ".sha256" )
                );
            }
        }

        private static ChocolateyPackSettings GetPackSettings( ChaskisContext context )
        {
            return new ChocolateyPackSettings
            {
                // Package Specific Section
                Id = "chaskis",
                Version = context.TemplateConstants.ChaskisVersion,
                PackageSourceUrl = new Uri( context.TemplateConstants.ProjectUrl ),
                Owners = new string[] { context.TemplateConstants.Author },

                // Software Specific Section
                Title = context.TemplateConstants.FullName + " (Install)",
                Authors = new string[] { context.TemplateConstants.Author },
                ProjectUrl = new Uri( context.TemplateConstants.ProjectUrl ),
                IconUrl = new Uri( context.TemplateConstants.IconUrl ),
                Copyright = context.TemplateConstants.CopyRight,
                LicenseUrl = new Uri( context.TemplateConstants.LicenseUrl ),
                RequireLicenseAcceptance = false,
                ProjectSourceUrl = new Uri( context.TemplateConstants.ProjectUrl ),
                DocsUrl = new Uri( context.TemplateConstants.WikiUrl ),
                BugTrackerUrl = new Uri( context.TemplateConstants.IssueTrackerUrl ),
                Tags = context.TemplateConstants.CliTags.Split( ' ', StringSplitOptions.RemoveEmptyEntries ),
                Summary = context.TemplateConstants.Summary,
                Description = context.TemplateConstants.Description,
                ReleaseNotes = new string[] { context.TemplateConstants.ReleaseNotes }
            };
        }

        private static string GetInstallPs1FileContents( ChaskisContext context )
        {
            // First, need to set the checksum of the MSI file.
            string checksum = File.ReadAllText( context.Paths.MsiChecksumFile.ToString() ).Trim();

            string installPs1 =
    $@"
$ErrorActionPreference = 'Stop';
$toolsDir   = ""$(Split-Path -parent $MyInvocation.MyCommand.Definition)""
$fileLocation      = Join-Path $toolsDir 'ChaskisInstaller.msi'

$packageArgs = @{{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'msi'
  file          = $fileLocation
  softwareName  = 'chaskis*'
  checksum    = '{checksum}'
  checksumType= 'sha256'

  # MSI
  silentArgs    = ""/qn /norestart /l*v `""$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""""
  validExitCodes= @(0, 3010, 1641)
}}

Install-ChocolateyPackage @packageArgs
";

            return installPs1;
        }

        private static string GetUninstallPs1FileContents( ChaskisContext context )
        {
            string uninstallPs1 =
    @"
$ErrorActionPreference = 'Stop';
$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  softwareName  = 'chaskis*'
  fileType      = 'MSI'
  silentArgs    = ""/qn /norestart""
  validExitCodes= @(0, 3010, 1605, 1614, 1641)
}

$uninstalled = $false
[array]$key = Get-UninstallRegistryKey -SoftwareName $packageArgs['softwareName']

if ($key.Count -eq 1) {
  $key | % { 
    $packageArgs['file'] = ""$($_.UninstallString)""
    
    if ($packageArgs['fileType'] -eq 'MSI') {
      $packageArgs['silentArgs'] = ""$($_.PSChildName) $($packageArgs['silentArgs'])""
      $packageArgs['file'] = ''
    } else {
    }

    Uninstall-ChocolateyPackage @packageArgs
  }
} elseif ($key.Count -eq 0) {
  Write-Warning ""$packageName has already been uninstalled by other means.""
} elseif ($key.Count -gt 1) {
  Write-Warning ""$($key.Count) matches found!""
  Write-Warning ""To prevent accidental data loss, no programs will be uninstalled.""
  Write-Warning ""Please alert package maintainer the following keys were matched:""
  $key | % {Write-Warning ""- $($_.DisplayName)""}
}
";
            return uninstallPs1;
        }
    }
}
