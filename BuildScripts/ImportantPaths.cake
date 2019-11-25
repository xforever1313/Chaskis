/// <summary>
/// This class contains all of the important files
/// and directories that the build scripts will care about.
/// </summary>
public class ImportantPaths
{
    // ---------------- Constructor ----------------

    public ImportantPaths( DirectoryPath projectRoot )
    {
        this.ProjectRoot = projectRoot;

        this.BuildScriptFolder = this.ProjectRoot.Combine( new DirectoryPath( "BuildScripts" ) );
        this.CodeCoverageFolder = this.ProjectRoot.Combine( new DirectoryPath( "CodeCoverage" ) );
        this.DocumentationFolder = this.ProjectRoot.Combine( new DirectoryPath( "Documentation" ) );
        this.SavedChecksumsFolder = this.ProjectRoot.Combine( new DirectoryPath( "SavedChecksums" ) );
        this.SourceFolder = this.ProjectRoot.Combine( new DirectoryPath( "Chaskis" ) );
        this.UnitTestResultFolder = this.ProjectRoot.Combine( new DirectoryPath( "TestResult" ) );
        this.OutputPackages = this.ProjectRoot.Combine( new DirectoryPath( "DistPackages" ) );

        this.ChaskisCliFolder = this.SourceFolder.Combine( new DirectoryPath( "Chaskis" ) );
        this.ChaskisCoreFolder = this.SourceFolder.Combine( new DirectoryPath( "ChaskisCore" ) );
        this.RegressionTestFolder = this.SourceFolder.Combine( new DirectoryPath( "RegressionTests" ) );
        this.RegressionDistroFolder = this.RegressionTestFolder.Combine( new DirectoryPath( "dist" ) );
        this.RegressionTestRunnerDirectory = this.RegressionTestFolder.Combine( new DirectoryPath( "runner" ) );
        this.RegressionTestResultsFolder = this.RegressionTestFolder.Combine( new DirectoryPath( "TestResults" ) );
        this.RegressionTestResultsFile = this.RegressionTestResultsFolder.CombineWithFilePath( new FilePath( "TestResults.html" ) );
        this.FitNesseRoot = this.RegressionTestFolder.Combine( new DirectoryPath( "FitNesseRoot" ) );
        this.UnitTestFolder = this.SourceFolder.Combine( new DirectoryPath( "UnitTests" ) );

        this.InstallConfigFolder = this.SourceFolder.Combine( new DirectoryPath( "Install" ) );
        this.WindowsInstallConfigFolder = this.InstallConfigFolder.Combine( new DirectoryPath( "windows" ) );
        this.WixConfigFolder = this.WindowsInstallConfigFolder;
        this.LinuxInstallConfigFolder = this.InstallConfigFolder.Combine( new DirectoryPath( "linux" ) );
        this.ArchLinuxInstallConfigFolder = this.LinuxInstallConfigFolder.Combine( new DirectoryPath( "arch" ) );
        this.DebianLinuxInstallConfigFolder = this.LinuxInstallConfigFolder.Combine( new DirectoryPath( "debian" ) );
        this.FedoraLinuxInstallConfigFolder = this.LinuxInstallConfigFolder.Combine( new DirectoryPath( "fedora" ) );
        this.ChocolateyInstallConfigFolder = this.InstallConfigFolder.Combine( new DirectoryPath( "chocolatey" ) );
        this.CliInstallerProjectFolder = this.InstallConfigFolder.Combine( new DirectoryPath( "ChaskisCliInstaller" ) );

        this.SolutionPath = this.SourceFolder.CombineWithFilePath( new FilePath( "Chaskis.sln" ) );
        this.ChaskisVersionFile = this.ChaskisCliFolder.CombineWithFilePath( new FilePath( "Chaskis.cs" ) );
        this.ChaskisCoreVersionFile = this.ChaskisCoreFolder.CombineWithFilePath( new FilePath( "Chaskis.Core.csproj" ) );
        this.LicenseFile = this.ProjectRoot.CombineWithFilePath( new FilePath( "LICENSE_1_0.txt" ) );
        this.DefaultPluginFile = this.ChaskisCliFolder.CombineWithFilePath( new FilePath( "DefaultHandlers.cs" ) );
        this.RegressionTestPluginFile = this.RegressionTestFolder.CombineWithFilePath( new FilePath( "RegressionTestPlugin/RegressionTestPlugin.cs" ) );

        this.DebianChecksumFile = this.SavedChecksumsFolder.CombineWithFilePath( new FilePath( "chaskis.deb.sha256" ) );
        this.MsiChecksumFile = this.SavedChecksumsFolder.CombineWithFilePath( new FilePath( "ChaskisInstaller.msi.sha256" ) );
    }

    // ---------------- Properties ----------------

    // -------- Root Folders --------

    /// <summary>
    /// The absolute path to the root of the Repo.
    /// </summary>
    public DirectoryPath ProjectRoot { get; private set; }

    /// <summary>
    /// Directory where the various build scripts live.
    /// </summary>
    public DirectoryPath BuildScriptFolder { get; private set; }

    /// <summary>
    /// Directory where code coverage data gets put.
    /// </summary>
    public DirectoryPath CodeCoverageFolder { get; private set; }

    /// <summary>
    /// Directory where project-level documentation lives.
    /// </summary>
    public DirectoryPath DocumentationFolder { get; private set; }

    /// <summary>
    /// Directory where saved checksums live.
    /// </summary>
    public DirectoryPath SavedChecksumsFolder { get; private set; }

    /// <summary>
    /// The path that contains the source code.
    /// </summary>
    public DirectoryPath SourceFolder { get; private set; }

    /// <summary>
    /// Where the unit test results get saved.
    /// </summary>
    public DirectoryPath UnitTestResultFolder { get; private set; }

    /// <summary>
    /// Where to dump packages for distribution.
    /// </summary>
    public DirectoryPath OutputPackages { get; private set; }

    // -------- Project Folders --------

    /// <summary>
    /// Folder where the Chaskis CLI project is.
    /// </summary>
    public DirectoryPath ChaskisCliFolder { get; private set; }

    /// <summary>
    /// Folder where the Chaskis.Core project is.
    /// </summary>
    public DirectoryPath ChaskisCoreFolder { get; private set; }

    /// <summary>
    /// Folder where the Regression Tests live.
    /// </summary>
    public DirectoryPath RegressionTestFolder { get; private set; }

    /// <summary>
    /// Where to make a distro for regression tests.
    /// </summary>
    public DirectoryPath RegressionDistroFolder { get; private set; }

    /// <summary>
    /// Path to where FitNesse expects the NetRunner executable.
    /// </summary>
    public DirectoryPath RegressionTestRunnerDirectory { get; private set; }

    /// <summary>
    /// Folder to dump the regression test results.
    /// </summary>
    public DirectoryPath RegressionTestResultsFolder{ get; private set; }

    /// <summary>
    /// File to dump the regression test results.
    /// </summary>
    public FilePath RegressionTestResultsFile { get; private set; }

    /// <summary>
    /// The FitNesse Root.
    /// </summary>
    public DirectoryPath FitNesseRoot { get; private set; }

    /// <summary>
    /// Folder where the Unit Tests live.
    /// </summary>
    public DirectoryPath UnitTestFolder { get; private set; }

    // -------- Install Config Folders --------

    /// <summary>
    /// Folder where the various install information lives.
    /// </summary>
    public DirectoryPath InstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the Windows Install Configs live.
    /// </summary>
    public DirectoryPath WindowsInstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the MSI configuration lives (powered by WIX).
    /// </summary>
    public DirectoryPath WixConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the Linux Install Configs live.
    /// </summary>
    public DirectoryPath LinuxInstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the Arch Linux Install Configs live.
    /// </summary>
    public DirectoryPath ArchLinuxInstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the Debian/Ubuntu Linux Install Configs live.
    /// </summary>
    public DirectoryPath DebianLinuxInstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the Fedora Linux Install Configs live.
    /// </summary>
    public DirectoryPath FedoraLinuxInstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the chocolatey config lives.
    /// </summary>
    public DirectoryPath ChocolateyInstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the CLI installer lives.
    /// </summary>
    public DirectoryPath CliInstallerProjectFolder { get; private set; }

    // -------- Important Files --------

    /// <summary>
    /// The path to the Solution.
    /// </summary>
    public FilePath SolutionPath { get; private set; }

    /// <summary>
    /// File where the Chaskis version can be found.
    /// </summary>
    public FilePath ChaskisVersionFile { get; private set; }

    /// <summary>
    /// File where the Chaskis Core version can be found.
    /// </summary>
    public FilePath ChaskisCoreVersionFile { get; private set; }

    /// <summary>
    /// The License file for this project.
    /// </summary>
    public FilePath LicenseFile { get; private set; }

    /// <summary>
    /// The file for the default Chaskis plugin's main file.
    /// </summary>
    public FilePath DefaultPluginFile { get; private set; }

    /// <summary>
    /// The file for the regression test plugin's main file.
    /// </summary>
    public FilePath RegressionTestPluginFile { get; private set; }

    /// <summary>
    /// The file that contains the .deb saved checksum.
    /// </summary>
    public FilePath DebianChecksumFile { get; private set; }

    /// <summary>
    /// The file that contains the .MSI saved checksum.
    /// </summary>
    public FilePath MsiChecksumFile { get; private set; }
}
