#load "Includes.cake"

public class PkgBuildRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext context;

    private readonly ImportantPaths paths;

    private readonly DirectoryPath workDir;

    private readonly FilePath pkgFile;

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
    }

    private void BuildPkgFile()
    {
        ProcessSettings settings = new ProcessSettings
        {
            WorkingDirectory = this.workDir
        };
        int exitCode = this.context.StartProcess( "makepkg", settings );
        if( exitCode != 0 )
        {
            throw new ApplicationException(
                "Could not package deb, got exit code: " + exitCode
            );
        }
    }

    private void CopyFile( FilePath source, DirectoryPath destination )
    {
        this.context.Information( $"Moving '{source}' to '${destination}'" );
        this.context.CopyFileToDirectory( source, destination );
    }
}