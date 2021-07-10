//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.Core.IO;

/// <summary>
/// This class contains all of the important files
/// and directories that the build scripts will care about.
/// </summary>
public class ImportantPaths
{
    // ---------------- Constructor ----------------

    public ImportantPaths( DirectoryPath repoRoot )
    {
        this.ProjectRoot = repoRoot;
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
    public DirectoryPath BuildScriptFolder => this.ProjectRoot.Combine( new DirectoryPath( "BuildScripts" ) );

    /// <summary>
    /// Directory where project-level documentation lives.
    /// </summary>
    public DirectoryPath DocumentationFolder => this.ProjectRoot.Combine( new DirectoryPath( "Documentation" ) );

    /// <summary>
    /// Directory where saved checksums live.
    /// </summary>
    public DirectoryPath SavedChecksumsFolder => this.ProjectRoot.Combine( new DirectoryPath( "SavedChecksums" ) );

    /// <summary>
    /// The path that contains the source code.
    /// </summary>
    public DirectoryPath SourceFolder => this.ProjectRoot.Combine( new DirectoryPath( "Chaskis" ) );

    /// <summary>
    /// Where the unit test results get saved.
    /// </summary>
    public DirectoryPath TestResultFolder => this.ProjectRoot.Combine( new DirectoryPath( "TestResults" ) );

    /// <summary>
    /// Where to dump packages for distribution.
    /// </summary>
    public DirectoryPath OutputPackages => this.ProjectRoot.Combine( new DirectoryPath( "DistPackages" ) );

    // -------- Project Folders --------

    /// <summary>
    /// Folder where the Chaskis CLI project is.
    /// </summary>
    public DirectoryPath ChaskisCliFolder => this.SourceFolder.Combine( new DirectoryPath( "Chaskis" ) );

    /// <summary>
    /// Folder where the Chaskis.Core project is.
    /// </summary>
    public DirectoryPath ChaskisCoreFolder => this.SourceFolder.Combine( new DirectoryPath( "ChaskisCore" ) );

    /// <summary>
    /// Folder where the Regression Tests live.
    /// </summary>
    public DirectoryPath RegressionTestFolder => this.SourceFolder.Combine( new DirectoryPath( "RegressionTests" ) );

    /// <summary>
    /// Folder where the regression test project is.
    /// </summary>
    public DirectoryPath RegressionTestProjectFolder => this.RegressionTestFolder.Combine( new DirectoryPath( "TestFixtures" ) );

    /// <summary>
    /// Where to make a distro for regression tests.
    /// </summary>
    public DirectoryPath RegressionDistroFolder => this.RegressionTestFolder.Combine( new DirectoryPath( "dist" ) );

    /// <summary>
    /// Folder where the Unit Tests live.
    /// </summary>
    public DirectoryPath UnitTestFolder => this.SourceFolder.Combine( new DirectoryPath( "UnitTests" ) );

    // -------- Install Config Folders --------

    /// <summary>
    /// Folder where the various install information lives.
    /// </summary>
    public DirectoryPath InstallConfigFolder => this.SourceFolder.Combine( new DirectoryPath( "Install" ) );

    /// <summary>
    /// Folder where the Windows Install Configs live.
    /// </summary>
    public DirectoryPath WindowsInstallConfigFolder => this.InstallConfigFolder.Combine( new DirectoryPath( "windows" ) );

    /// <summary>
    /// Folder where the MSI configuration lives (powered by WIX).
    /// </summary>
    public DirectoryPath WixConfigFolder => this.WindowsInstallConfigFolder;

    /// <summary>
    /// Path to the WIX windows directory.
    /// </summary>
    public FilePath WindowsWixFile => this.WixConfigFolder.CombineWithFilePath( new FilePath( "Product.wxs" ) );

    /// <summary>
    /// Path to the directory that will contain the MSI file.
    /// </summary>
    public DirectoryPath MsiDirectory => this.WindowsInstallConfigFolder.Combine( new DirectoryPath( "bin/x64/Release/" ) );

    /// <summary>
    /// Path to the MSI file.
    /// </summary>
    public FilePath MsiPath => this.MsiDirectory.CombineWithFilePath( new FilePath( "ChaskisInstaller.msi" ) );

    /// <summary>
    /// Path to the checksum file of the MSI.
    /// </summary>
    public FilePath MsiChecksumFile => this.MsiDirectory.CombineWithFilePath( new FilePath( "ChaskisInstaller.msi.sha256" ) );

    /// <summary>
    /// Folder where the Linux Install Configs live.
    /// </summary>
    public DirectoryPath LinuxInstallConfigFolder => this.InstallConfigFolder.Combine( new DirectoryPath( "linux" ) );

    /// <summmary>
    /// Path to the script that goes in the linux bin folder
    /// during install.
    /// </summary>
    public FilePath LinuxBinFile => this.LinuxInstallConfigFolder.CombineWithFilePath( new FilePath( "bin/chaskis" ) );

    /// <summary>
    /// Path to the systemd file.
    /// </summary>
    public FilePath SystemdFile => this.LinuxInstallConfigFolder.CombineWithFilePath( new FilePath( "systemd/chaskis.service" ) );

    /// <summary>
    /// Folder where the Arch Linux Install Configs live.
    /// </summary>
    public DirectoryPath ArchLinuxInstallConfigFolder => this.LinuxInstallConfigFolder.Combine( new DirectoryPath( "arch" ) );

    /// <summary>
    /// Folder where the Debian/Ubuntu Linux Install Configs live.
    /// </summary>
    public DirectoryPath DebianLinuxInstallConfigFolder => this.LinuxInstallConfigFolder.Combine( new DirectoryPath( "debian" ) );

    /// <summary>
    /// Location of the directory that contains the deb file.
    /// </summary>
    public DirectoryPath DebDirectory => this.OutputPackages.Combine( new DirectoryPath( "debian" ) );

    /// <summary>
    /// Path to the .deb file.
    /// </summary>
    public FilePath DebPath => this.DebDirectory.CombineWithFilePath( new FilePath( "chaskis.deb" ) );

    /// <summary>
    /// Path to the checksum file of the .deb file.
    /// </summary>
    public FilePath DebChecksumFile => this.DebDirectory.CombineWithFilePath( new FilePath( "chaskis.deb.sha256" ) );

    /// <summary>
    /// Folder where the Fedora Linux Install Configs live.
    /// </summary>
    public DirectoryPath FedoraLinuxInstallConfigFolder => this.LinuxInstallConfigFolder.Combine( new DirectoryPath( "fedora" ) );

    /// <summary>
    /// Where the fedora spec file gets outputted.
    /// </summary>
    public FilePath SpecFileOutputPath => this.OutputPackages.CombineWithFilePath( new FilePath( "fedora/chaskis.spec" ) );

    /// <summary>
    /// Folder where the chocolatey config lives.
    /// </summary>
    public DirectoryPath ChocolateyInstallConfigFolder => this.InstallConfigFolder.Combine( new DirectoryPath( "chocolatey" ) );

    /// <summary>
    /// Folder where the CLI installer lives.
    /// </summary>
    public DirectoryPath CliInstallerProjectFolder => this.InstallConfigFolder.Combine( new DirectoryPath( "ChaskisCliInstaller" ) );

    // -------- Important Files --------

    /// <summary>
    /// The path to the Solution.
    /// </summary>
    public FilePath SolutionPath => this.SourceFolder.CombineWithFilePath( new FilePath( "Chaskis.sln" ) );

    /// <summary>
    /// File where the Chaskis version can be found.
    /// </summary>
    public FilePath ChaskisVersionFile => this.ChaskisCliFolder.CombineWithFilePath( new FilePath( "Chaskis.cs" ) );

    /// <summary>
    /// File where the Chaskis Core version can be found.
    /// </summary>
    public FilePath ChaskisCoreVersionFile => this.ChaskisCoreFolder.CombineWithFilePath( new FilePath( "Chaskis.Core.csproj" ) );

    /// <summary>
    /// The License file for this project.
    /// </summary>
    public FilePath LicenseFile => this.ProjectRoot.CombineWithFilePath( new FilePath( "LICENSE_1_0.txt" ) );

    /// <summary>
    /// The file for the default Chaskis plugin's main file.
    /// </summary>
    public FilePath DefaultPluginFile => this.ChaskisCliFolder.CombineWithFilePath( new FilePath( "DefaultHandlers.cs" ) );

    /// <summary>
    /// The file for the regression test plugin's main file.
    /// </summary>
    public FilePath RegressionTestPluginFile => this.RegressionTestFolder.CombineWithFilePath( new FilePath( "RegressionTestPlugin/RegressionTestPlugin.cs" ) );

    /// <summary>
    /// The file that contains the .deb saved checksum.
    /// </summary>
    public FilePath SavedDebianChecksumFile => this.SavedChecksumsFolder.CombineWithFilePath( new FilePath( "chaskis.deb.sha256" ) );

    /// <summary>
    /// The file that contains the .MSI saved checksum.
    /// </summary>
    public FilePath SavedMsiChecksumFile => this.SavedChecksumsFolder.CombineWithFilePath( new FilePath( "ChaskisInstaller.msi.sha256" ) );
}
