// ---------------- Arguments ----------------

const string defaultTarget = "default";
string target = Argument( "target", defaultTarget );

// ---------------- Globals ----------------

ImportantPaths paths = new ImportantPaths( MakeAbsolute( new DirectoryPath( "." ) ) );

bool isWindows = IsRunningOnWindows();

// If we are in a Jenkins Environment, we'll assume Jenkins builds all of the stuff in the correct order
// since its starting from an empty repo.
bool isJenkins = Environment.UserName == "ContainerUser";

#load "BuildScripts/Includes.cake"

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

var unitTestTask = Task( "unit_test" )
.Does(
    ( context ) =>
    {
        UnitTestRunner runner = new UnitTestRunner( context, paths );
        runner.RunTests();
    }
)
.Description( "Runs all the unit tests (does not run code coverage)." );
if( isJenkins == false )
{
    unitTestTask.IsDependentOn( "debug" );
}

var unitTestCoverageTask = Task( "unit_test_code_coverage" )
.Does(
    ( context ) =>
    {
        UnitTestRunner runner = new UnitTestRunner( context, paths );
        runner.RunCodeCoverage();
    }
)
.Description( "Runs code coverage, Windows only." )
.WithCriteria( isWindows );
if( isJenkins == false )
{
    unitTestCoverageTask.IsDependentOn( "debug" );
}

const string bootStrapTaskName = "bootstrap_regression_tests";

var bootStrapTask = Task( bootStrapTaskName )
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
.Description( "Creates the distro for the regression tests." );
if( isJenkins == false )
{
    bootStrapTask.IsDependentOn( "debug" );
}

Task( "regression_test" )
.Does(
    ( context ) =>
    {
        RegressionTestRunner runner = new RegressionTestRunner( context, paths );
        runner.RunTests();
    }
).Description(
    "Runs all regression tests."
).IsDependentOn( bootStrapTaskName );

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
//).IsDependentOn( bootStrapTaskName )
//.WithCriteria( isWindows );

// -------- Package Targets --------

var msiTask = Task( "msi" )
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
.WithCriteria( isWindows );
if( isJenkins == false )
{
    msiTask.IsDependentOn( "unit_test" );
}

var makeDistroTask = Task( "make_distro" )
.Does(
    ( context ) =>
    {
        string output = Argument( "output", string.Empty );
        if( string.IsNullOrWhiteSpace( output ) )
        {
            throw new ArgumentNullException( nameof( output ), "Output must be specified" );
        }

        DistroCreatorConfig config = new DistroCreatorConfig
        {
            OutputLocation = output,
            Target = "Release"
        };

        DistroCreator creator = new DistroCreator( context, paths, config );
        creator.CreateDistro();
    }
).Description( "Runs the Chaskis CLI installer and puts a disto in the specified location (using arguemnt 'output')" );
if( isJenkins == false )
{
    makeDistroTask.IsDependentOn( "Release" );
}

var nugetPackTask = Task( "nuget_pack" )
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
.Description( "Creates the Chaskis Core NuGet package. ");
if( isJenkins == false )
{
    nugetPackTask.IsDependentOn( "release" );
}

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

var debianPackTask = Task( "debian_pack" )
.Does(
    ( context ) =>
    {
        DebRunner runner = new DebRunner( context, paths );
        runner.BuildDeb();
    }
);
if( isJenkins == false )
{
    debianPackTask.IsDependentOn( "release" );
}

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
