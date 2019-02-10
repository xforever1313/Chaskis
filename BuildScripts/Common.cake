void GenerateSha256( FilePath source, FilePath output )
{
    FileHash hash = CalculateFileHash( source, HashAlgorithm.SHA256 );

    string hashStr = hash.ToHex();
    FileWriteText( output, hashStr );
    Information( "Hash for " + source.GetFilename() + ": " + hashStr );
}