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

        this.ChaskisCliFolder = this.SourceFolder.Combine( new DirectoryPath( "Chaskis" ) );
        this.ChaskisCoreFolder = this.SourceFolder.Combine( new DirectoryPath( "ChaskisCore" ) );
        this.InstallConfigFolder = this.SourceFolder.Combine( new DirectoryPath( "Install" ) );
        this.RegressionTestFolder = this.SourceFolder.Combine( new DirectoryPath( "RegressionTests" ) );
        this.UnitTestFolder = this.SourceFolder.Combine( new DirectoryPath( "UnitTests" ) );

        this.SolutionPath = this.SourceFolder.CombineWithFilePath( new FilePath( "Chaskis.sln" ) );
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
    /// Folder where the various install information lives.
    /// </summary>
    public DirectoryPath InstallConfigFolder { get; private set; }

    /// <summary>
    /// Folder where the Regression Tests live.
    /// </summary>
    public DirectoryPath RegressionTestFolder { get; private set; }

    /// <summary>
    /// Folder where the Unit Tests live.
    /// </summary>
    public DirectoryPath UnitTestFolder { get; private set; }

    // -------- Important Files --------

    /// <summary>
    /// The path to the Solution.
    /// </summary>
    public FilePath SolutionPath { get; private set; }

    // ---------------- Functions ----------------
}
