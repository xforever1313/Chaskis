#load "Includes.cake"

public class DebRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext context;

    private readonly ImportantPaths paths;

    // ---------------- Constructor ----------------

    public DebRunner( ICakeContext context, ImportantPaths paths )
    {
        this.context = context;
        this.paths = paths;
    }

    // ---------------- Functions ----------------

    public void BuildDeb()
    {
        if( this.context.IsRunningOnWindows() )
        {
            throw new InvalidOperationException( "Can only build DEB file on Linux." );
        }

        // We will be creating the .deb file in the Debian install directory.
        // Like with building C# Assemblies, we will do all of the work
        // in the obj folder, and the final outputs will be in the bin
        // folder.

        // First, create and obj and bin folders, and ensure they are clean.
        DirectoryPath objFolder = this.paths.DebianLinuxInstallConfigFolder.Combine(
            new DirectoryPath( "obj" )
        );
        DirectoryPath outputFolder = this.paths.DebianLinuxInstallConfigFolder.Combine(
            new DirectoryPath( "bin" )
        );

        this.context.EnsureDirectoryExists( objFolder );
        this.context.CleanDirectory( objFolder );
        SetDirectoryPermission( objFolder, "755" );
        this.context.EnsureDirectoryExists( outputFolder );
        this.context.CleanDirectory( outputFolder );

        // Directory structure:
        // obj
        //  L chaskis
        //       L DEBIAN (must be 755)
        //            L control (control file)
        //       L bin
        //           L chaskis  (chaskis startup script)
        //       L usr
        //          L lib
        //             L systemd
        //                   L user
        //                       L chaskis.service
        //             L chaskis install directory

        // Create the directories.
        DirectoryPath chaskisWorkDir = CombineAndCreateDirectory( objFolder, "chaskis" );
        DirectoryPath debianFolder = CombineAndCreateDirectory( chaskisWorkDir, "DEBIAN" );
        DirectoryPath binFolder = CombineAndCreateDirectory( chaskisWorkDir, "bin" );
        DirectoryPath usrFolder = CombineAndCreateDirectory( chaskisWorkDir, "usr" );
        DirectoryPath usrLibFolder = CombineAndCreateDirectory( usrFolder, "lib" );
        DirectoryPath systemdFolder = CombineAndCreateDirectory( usrLibFolder, "systemd" );
        DirectoryPath systemdUserFolder = CombineAndCreateDirectory( systemdFolder, "user" );       

        // Move the linux files around.
        FilePath controlFile = this.paths.DebianLinuxInstallConfigFolder.CombineWithFilePath(
            new FilePath( "control" )
        );
        CopyFile( controlFile, debianFolder );
        CopyFile( this.paths.LinuxBinFile, binFolder );
        CopyFile( this.paths.SystemdFile, systemdUserFolder );

        // Next, need to run the distro creator to put everything
        // in the usr/lib/Chaskis folder.
        DistroCreatorConfig distroConfig = new DistroCreatorConfig
        {
            IsWindows = this.context.IsRunningOnWindows(),
            OutputLocation = usrLibFolder.ToString(),
            Target = "Release"
        };

        DistroCreator distroRunner = new DistroCreator(
            this.context,
            this.paths,
            distroConfig
        );
        distroRunner.CreateDistro();

        // Lastly, need to package everything up.
        ProcessArgumentBuilder arguments = ProcessArgumentBuilder.FromString( "--root-owner-group --build chaskis" );
        ProcessSettings settings = new ProcessSettings
        {
            Arguments = arguments,
            WorkingDirectory = objFolder
        };
        int exitCode = this.context.StartProcess( "dpkg-deb", settings );
        if( exitCode != 0 )
        {
            throw new ApplicationException(
                "Could not package deb, got exit code: " + exitCode
            );
        }

        // Copy from the obj folder to the bin folder.
        CopyFile(
            objFolder.CombineWithFilePath( new FilePath( "chaskis.deb" ) ),
            outputFolder
        );

        // Run the checksum.
        GenerateSha256(
            this.context,
            outputFolder.CombineWithFilePath( new FilePath( "chaskis.deb" ) ),
            outputFolder.CombineWithFilePath( new FilePath( "chaskis.deb.sha256" ) )
        );
    }

    private DirectoryPath CombineAndCreateDirectory( DirectoryPath parentFolder, string childFolderName )
    {
        DirectoryPath combinedPath = parentFolder.Combine(
            new DirectoryPath( childFolderName )
        );
        this.context.EnsureDirectoryExists( combinedPath );

        // All directories should be 755 to be consistent
        // with the old installation method.
        SetDirectoryPermission( combinedPath, "755" );

        return combinedPath;
    }

    private void CopyFile( FilePath source, DirectoryPath destination )
    {
        this.context.Information( $"Moving '{source}' to '${destination}'" );
        this.context.CopyFileToDirectory( source, destination );
    }

    private void SetDirectoryPermission( DirectoryPath directory, string chmodValue )
    {
        this.context.SetDirectoryPermission( directory, chmodValue );
    }
}
