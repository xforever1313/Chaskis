using System.Text.RegularExpressions;

// ---------------- Arguments ----------------

const string defaultTarget = "default";
const string frameworkTarget = "net471";
string target = Argument( "target", defaultTarget );

// ---------------- Globals ----------------

#load "BuildScripts/ImportantPaths.cake"

ImportantPaths paths = new ImportantPaths( MakeAbsolute( new DirectoryPath( "." ) ) );

bool isWindows = ( Environment.OSVersion.Platform == PlatformID.Win32NT );

// ---------------- Includes ----------------

#addin "Cake.FileHelpers&version=3.1.0"

#load "BuildScripts/Common.cake"
#load "BuildScripts/MSBuild.cake"
#load "BuildScripts/UnitTest.cake"
#load "BuildScripts/Templatize.cake"

// ---------------- Targets ----------------

Task( "template" )
.Does(
    ( context ) =>
    {
        TemplateConstants templateConstants = new TemplateConstants(
            context,
            paths,
            frameworkTarget
        );

        FilesToTemplate files = new FilesToTemplate( paths );

        Templatizer templatizer = new Templatizer( templateConstants, files );
        templatizer.Template();
    }
)
.Description( "Runs the templator on all template files." );

Task( "debug" )
.Does(
    () =>
    {
        DoMsBuild( "Debug" );
    }
)
.Description( "Builds Chaskis with Debug turned on." );

Task( "release" )
.Does(
    () =>
    {
        DoMsBuild( "Release" );
    }
)
.Description( "Builds Chaskis with Release turned on." );

Task( "unit_test" )
.Does(
    ( context ) =>
    {
        UnitTestRunner runner = new UnitTestRunner( context, paths );
        runner.RunUnitTests();
    }
)
.Description( "Runs all the unit tests (does not run code coverage)." )
.IsDependentOn( "debug" );

Task( "msi" )
.Does(
    () =>
    {
        DirectoryPath outputPath = paths.OutputPackages.Combine( "windows" );
        EnsureDirectoryExists( outputPath );
        CleanDirectory( outputPath );

        DirectoryPath msiLocation = paths.InstallConfigFolder.Combine( "windows/bin/x64/Release" );

        FilePath msiFile = msiLocation.CombineWithFilePath( "ChaskisInstaller.msi" );
        FilePath checksumLocation = msiLocation.CombineWithFilePath( "ChaskisInstaller.msi.sha256" );

        DoMsBuild( "Install", PlatformTarget.x64 );
        GenerateSha256(
            msiFile,
            checksumLocation
        );

        CopyFileToDirectory(
            msiFile,
            outputPath
        );

        CopyFileToDirectory(
            checksumLocation,
            outputPath
        );
    }
)
.Description( "Builds the MSI on Windows." )
.WithCriteria( isWindows )
.IsDependentOn( "unit_test" );

Task( "code_coverage" )
.Does(
    ( context ) =>
    {
        UnitTestRunner runner = new UnitTestRunner( context, paths );
        runner.RunCodeCoverage();
    }
)
.Description( "Runs code coverage, Windows only." )
.WithCriteria( isWindows )
.IsDependentOn( "debug" );

Task( "nuget_pack" )
.Does(
    () =>
    {
        DirectoryPath outputPath = paths.OutputPackages.Combine( "nuget" );
        EnsureDirectoryExists( outputPath );
        CleanDirectory( outputPath );

        NuGetPackSettings settings = new NuGetPackSettings
        {
            OutputDirectory = outputPath,
            Properties = new Dictionary<string, string>
            {
                ["Configuration"] = "Release"
            }
        };

        NuGetPack(
            paths.ChaskisCoreFolder.CombineWithFilePath( "Chaskis.Core.csproj" ),
            settings
        );

        FilePath glob = outputPath.CombineWithFilePath( "*.nupkg" );
        foreach( FilePath file in GetFiles( glob.ToString() ) )
        {
            GenerateSha256(
                file,
                new FilePath( file.FullPath + ".sha256" )
            );
        }
    }
)
.Description( "Creates the Chaskis Core NuGet package. ")
.IsDependentOn( "release" );

Task( "choco_pack" )
.Does(
    () =>
    {
        DirectoryPath outputPath = paths.OutputPackages.Combine( "chocolatey" );
        EnsureDirectoryExists( outputPath );
        CleanDirectory( outputPath );

        DirectoryPath workingPath = paths.ChocolateyInstallConfigFolder.Combine( "package" );

        ChocolateyPackSettings settings = new ChocolateyPackSettings
        {
            OutputDirectory = outputPath,
            WorkingDirectory = workingPath
        };

        ChocolateyPack(
            workingPath.CombineWithFilePath( new FilePath( "chaskis.nuspec" ) ),
            settings
        );

        FilePath glob = outputPath.CombineWithFilePath( "*.nupkg" );
        foreach( FilePath file in GetFiles( glob.ToString() ) )
        {
            GenerateSha256(
                file,
                new FilePath( file.FullPath + ".sha256" )
            );
        }
    }
)
.WithCriteria( isWindows )
.Description( "Creates the Chocolatey Package (Windows Only)." );

Task( defaultTarget )
.IsDependentOn( "debug" )
.Description( "The default target; alias for 'debug'." );

RunTarget( target );