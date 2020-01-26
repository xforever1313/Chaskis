public static void GenerateSha256( this ICakeContext context, FilePath source, FilePath output )
{
    FileHash hash = context.CalculateFileHash( source, HashAlgorithm.SHA256 );

    string hashStr = hash.ToHex();
    context.FileWriteText( output, hashStr );
    context.Information( "Hash for " + source.GetFilename() + ": " + hashStr );
}