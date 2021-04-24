//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.Security;
using Cake.Core;
using Cake.Core.IO;
using Cake.FileHelpers;
using Cake.Frosting;

namespace DevOps
{
    public sealed class ChaskisContext : FrostingContext
    {
        // ---------------- Fields ----------------

        // ---------------- Constructor ----------------

        public ChaskisContext( ICakeContext context ) :
            base( context )
        {
            this.RepoRoot = context.Environment.WorkingDirectory;
            this.Paths = new ImportantPaths( this.RepoRoot );
        }

        // ---------------- Properties ----------------

        public DirectoryPath RepoRoot { get; private set; }

        public ImportantPaths Paths { get; private set; }

        // ---------------- Functions ----------------

        public void GenerateSha256( FilePath source, FilePath output )
        {
            FileHash hash = this.CalculateFileHash( source, HashAlgorithm.SHA256 );

            string hashStr = hash.ToHex();
            this.FileWriteText( output, hashStr );
            this.Information( "Hash for " + source.GetFilename() + ": " + hashStr );
        }

        public void SetDirectoryPermission( DirectoryPath directory, string chmodValue )
        {
            ProcessArgumentBuilder arguments = ProcessArgumentBuilder.FromString( $"{chmodValue} {directory}" );
            ProcessSettings settings = new ProcessSettings
            {
                Arguments = arguments
            };

            int exitCode = this.StartProcess( "chmod", settings );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    $"Could not set folder permission on '{directory}', got exit code: " + exitCode
                );
            }
        }
    }
}
