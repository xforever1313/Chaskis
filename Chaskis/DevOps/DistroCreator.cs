//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core.IO;

namespace DevOps
{
    public class DistroCreatorConfig
    {
        // ---------------- Constructor ----------------

        // ---------------- Properties ----------------

        public string OutputLocation { get; set; }

        public string Target { get; set; } = "Debug";
    }

    public class DistroCreator
    {
        // ---------------- Fields ----------------

        private readonly ChaskisContext context;
        private readonly DistroCreatorConfig config;

        private readonly DirectoryPath output;
        private readonly FilePath wixFile;

        // ---------------- Constructor ----------------

        public DistroCreator(
            ChaskisContext context,
            DistroCreatorConfig config
        )
        {
            this.context = context;
            this.config = config;

            this.output = new DirectoryPath( this.config.OutputLocation );
            if( this.context.IsRunningOnWindows() )
            {
                this.wixFile = this.context.Paths.WixConfigFolder.CombineWithFilePath(
                    new FilePath( "Product.wxs" )
                );
            }
            else
            {
                this.wixFile = this.context.Paths.WixConfigFolder.CombineWithFilePath(
                    new FilePath( "Product.wxs.linux" )
                );
            }
        }

        // ---------------- Functions ----------------

        public void CreateDistro()
        {
            string target = this.config.Target;
            this.context.EnsureDirectoryExists( this.output );
            this.context.CleanDirectory( this.output );
            if( this.context.IsRunningOnWindows() == false )
            {
                this.context.SetDirectoryPermission( this.output, "755" );
            }

            FilePath cliInstallerPath = this.context.Paths.CliInstallerProjectFolder.CombineWithFilePath(
                $"bin/{target}/{this.context.ApplicationTarget}/Chaskis.CliInstaller"
            );

            string arguments =
                $"\"{this.context.Paths.SourceFolder}\" \"{this.output}\" \"{this.wixFile}\" \"{target}\" \"{this.context.ApplicationTarget}\" \"{this.context.PluginTarget}\"";

            string exeName;

            if( this.context.IsRunningOnWindows() )
            {
                exeName = cliInstallerPath.ToString() + ".exe";
            }
            else
            {
                exeName = cliInstallerPath.ToString();
            }

            this.context.Information( $"Starting CLI Installer at '{exeName}' with arguments: '{arguments}'" );

            ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
            ProcessSettings settings = new ProcessSettings
            {
                Arguments = argumentsBuilder
            };
            int exitCode = this.context.StartProcess( exeName, settings );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    "Error when creating distro.  Got error: " + exitCode
                );
            }
        }
    }
}
