public static void GenerateSha256( this ICakeContext context, FilePath source, FilePath output )
{
    FileHash hash = context.CalculateFileHash( source, HashAlgorithm.SHA256 );

    string hashStr = hash.ToHex();
    context.FileWriteText( output, hashStr );
    context.Information( "Hash for " + source.GetFilename() + ": " + hashStr );
}

public static void SetDirectoryPermission( this ICakeContext context, DirectoryPath directory, string chmodValue )
{
    ProcessArgumentBuilder arguments = ProcessArgumentBuilder.FromString( $"{chmodValue} {directory}" );
    ProcessSettings settings = new ProcessSettings
    {
        Arguments = arguments
    };

    int exitCode = context.StartProcess( "chmod", settings );
    if( exitCode != 0 )
    {
        throw new ApplicationException(
            $"Could not set folder permission on '{directory}', got exit code: " + exitCode
        );
    }
}
