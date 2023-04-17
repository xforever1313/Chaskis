//
//          Copyright Seth Hendrick 2016-2021.
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
using DevOps.Template;
using SethCS.Extensions;

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
            this.TemplateConstants = new TemplateConstants( this );

#if DEBUG
            this.RunningRelease = false;
#else
            this.RunningRelease = true;
#endif
        }

        // ---------------- Properties ----------------

        public DirectoryPath RepoRoot { get; private set; }

        public ImportantPaths Paths { get; private set; }

        public TemplateConstants TemplateConstants { get; private set; }

        /// <summary>
        /// Dotnet target framework for applications (.exe).
        /// </summary>
        public string ApplicationTarget => "net6.0";
        
        /// <summary>
        /// Dotnet target framework for plugins or libraries (.dll).
        /// </summary>
        public string PluginTarget => "netstandard2.0";

        public bool IsJenkins =>
            ( "containeruser".EqualsIgnoreCase( System.Environment.UserName ) ) ||
            ( "jenknode".EqualsIgnoreCase( System.Environment.UserName ) );

        /// <summary>
        /// Was DevOps.exe compiled for release?
        /// </summary>
        public bool RunningRelease { get; set; }

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
            ProcessArgumentBuilder arguments = ProcessArgumentBuilder.FromString( $"{chmodValue} \"{directory}\"" );
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
