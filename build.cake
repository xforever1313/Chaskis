// ---------------- Arguments ----------------

const string defaultTarget = "default";
string target = Argument( "target", defaultTarget );

// ---------------- Globals ----------------

#load "BuildScripts/Includes.cake"

ImportantPaths paths = new ImportantPaths( MakeAbsolute( new DirectoryPath( "." ) ) );

bool isWindows = IsRunningOnWindows();

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

// -------- Build Targets --------

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

// -------- Test Targets --------

Task( "unit_test" )
.Does(
    ( context ) =>
    {
        UnitTestRunner runner = new UnitTestRunner( context, paths );
        runner.RunTests();
    }
)
.Description( "Runs all the unit tests (does not run code coverage)." )
.IsDependentOn( "debug" );

Task( "unit_test_code_coverage" )
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

const string bootStrapTask = "bootstrap_regression_tests";

Task( bootStrapTask )
.Does(
    ( context ) =>
    {
        DistroCreatorConfig distroConfig = new DistroCreatorConfig
        {
            OutputLocation = paths.RegressionDistroFolder.ToString()
        };

        DistroCreator distroCreator = new DistroCreator(
            context,
            paths,
            distroConfig
        );
        distroCreator.CreateDistro();
    }
)
.Description( "Creates the distro for the regression tests." )
.IsDependentOn( "debug" );

Task( "regression_test" )
.Does(
    ( context ) =>
    {
        RegressionTestRunner runner = new RegressionTestRunner( context, paths );
        runner.RunTests();
    }
).Description(
    "Runs all regression tests."
).IsDependentOn( bootStrapTask );

// Need to figure out paths first...
//Task( "regression_test_code_coverage" )
//.Does(
//    ( context ) =>
//    {
//        RegressionTestRunner runner = new RegressionTestRunner( context, paths );
//        runner.RunCodeCoverage();
//    }
//).Description(
//    "Runs all regression tests with code coverage."
//).IsDependentOn( bootStrapTask )
//.WithCriteria( isWindows );

// -------- Package Targets --------

Task( "msi" )
.Does(
    ( context ) =>
    {
        DirectoryPath outputPath = paths.OutputPackages.Combine( "windows" );
        EnsureDirectoryExists( outputPath );
        CleanDirectory( outputPath );

        DirectoryPath msiLocation = paths.InstallConfigFolder.Combine( "windows/bin/x64/Release" );

        FilePath msiFile = msiLocation.CombineWithFilePath( "ChaskisInstaller.msi" );
        FilePath checksumLocation = msiLocation.CombineWithFilePath( "ChaskisInstaller.msi.sha256" );

        MSBuildSettings settings = new MSBuildSettings
        {
            Configuration = "Install",
            MaxCpuCount = 0,
            PlatformTarget = PlatformTarget.x64,
            Restore = true,
            WorkingDirectory = paths.SourceFolder
        };

        // For WiX, need to call MSBuild, not dotnet core build.  Wix doesn't work with dotnet core.
        // Its... probably fine??
        MSBuild( paths.SolutionPath, settings );

        GenerateSha256(
            context,
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

Task( "nuget_pack" )
.Does(
    ( context ) =>
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
                context,
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
    ( context ) =>
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
                context,
                file,
                new FilePath( file.FullPath + ".sha256" )
            );
        }
    }
)
.WithCriteria( isWindows )
.Description( "Creates the Chocolatey Package (Windows Only)." );

Task( "debian_pack" )
.Does(
    ( context ) =>
    {
        DebRunner runner = new DebRunner( context, paths );
        runner.BuildDeb();
    }
).IsDependentOn( "release" );

Task( defaultTarget )
.IsDependentOn( "debug" )
.Description( "The default target; alias for 'debug'." );

Task( "appveyor" )
.IsDependentOn( "unit_test" )
.IsDependentOn( "nuget_pack" )
.IsDependentOn( "choco_pack" )
.IsDependentOn( "msi" )
.Description( "Runs when building AppVeyor" );

RunTarget( target );