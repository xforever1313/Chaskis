#load "Includes.cake"

public class PkgBuildRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext context;

    private readonly ImportantPaths paths;

    private readonly DirectoryPath workDir;

    private readonly FilePath pkgFile;

    private readonly FilePath srcInfoFile;

    // ---------------- Constructor ----------------

    public PkgBuildRunner( ICakeContext context, ImportantPaths paths )
    {
        this.context = context;
        this.paths = paths;

        this.workDir = this.paths.ArchLinuxInstallConfigFolder.Combine(
            new DirectoryPath( "obj" )
        );

        this.pkgFile = this.paths.ArchLinuxInstallConfigFolder.CombineWithFilePath(
            new FilePath( "PKGBUILD" )
        );

        this.srcInfoFile = this.workDir.CombineWithFilePath(
            new FilePath( ".SRCINFO" )
        );
    }

    // ---------------- Functions ----------------

    public void BuildArchPkg()
    {
        if( this.context.IsRunningOnWindows() )
        {
            throw new InvalidOperationException( "Can only build PKGBUILD package on Arch Linux." );
        }

        DirectoryPath outputFolder = this.paths.OutputPackages.Combine(
            new DirectoryPath( "arch_linux" )
        );

        // Create directories
        this.context.EnsureDirectoryExists( this.workDir );
        this.context.CleanDirectory( this.workDir );
        this.context.SetDirectoryPermission( this.workDir, "755" );
        this.context.EnsureDirectoryExists( outputFolder );
        this.context.CleanDirectory( outputFolder );

        // According the the makepkg documentation https://www.archlinux.org/pacman/makepkg.8.html
        // the build script must be in the same directory makepkg is called from.  We really
        // don't want all of that stuff to crowd the repo, so we'll stuff all the building into
        // the obj folder. However, this means we need to first copy the original PKGBUILD file
        // into the obj folder.
        this.CopyFile( this.pkgFile, this.workDir );
        this.BuildPkgFile();
        this.BuildSrcInfo();

        FilePath glob = new FilePath( "*.pkg.tar.*" );
        FilePathCollection files = this.context.GetFiles( this.workDir.CombineWithFilePath( glob ).ToString() );

        if( files.Count != 1 )
        {
            throw new ApplicationException( "Found 2 files to glob, something weird happened. Got: " + files.Count );
        }

        FilePath buildPackageFile = files.First();
        this.context.Information( "Arch Pkg: " + buildPackageFile.ToString() );

        FilePath sha256File = new FilePath( buildPackageFile.ToString() + ".sha256" );
        this.context.GenerateSha256( buildPackageFile, sha256File );

        this.CopyFile( buildPackageFile, outputFolder );
        this.CopyFile( sha256File, outputFolder );
        this.CopyFile( this.srcInfoFile, outputFolder );
        this.CopyFile( this.pkgFile, outputFolder );
    }

    private void BuildPkgFile()
    {
        this.context.Information( "Building PKG file..." );

        ProcessSettings settings = new ProcessSettings
        {
            WorkingDirectory = this.workDir
        };
        int exitCode = this.context.StartProcess( "makepkg", settings );
        if( exitCode != 0 )
        {
            throw new ApplicationException(
                "Could not package for Arch Linux, got exit code: " + exitCode
            );
        }

        this.context.Information( "Building PKG file... Done!" );
    }

    private void BuildSrcInfo()
    {
        this.context.Information( "Building .SRCINFO file..." );

        string arguments = "--printsrcinfo";
        ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );

        ProcessSettings settings = new ProcessSettings
        {
            Arguments = argumentsBuilder,
            WorkingDirectory = this.workDir,
            RedirectStandardOutput = true
        };

        IEnumerable<string> stdOut;
        int exitCode = this.context.StartProcess( "makepkg", settings, out stdOut );
        if( exitCode != 0 )
        {
            throw new ApplicationException(
                "Could not make SRCINFO, got exit code: " + exitCode
            );
        }

        System.IO.File.WriteAllLines( this.srcInfoFile.ToString(), stdOut );

        this.context.Information( "Building .SRCINFO file... Done!" );
    }

    private void CopyFile( FilePath source, DirectoryPath destination )
    {
        this.context.Information( $"Moving '{source}' to '${destination}'" );
        this.context.CopyFileToDirectory( source, destination );
    }
}