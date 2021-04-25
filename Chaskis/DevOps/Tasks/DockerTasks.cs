//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using DevOps.Template;

namespace DevOps.Tasks
{
    [TaskName( "wait_for_docker_to_start" )]
    public class WaitForDockerToStartTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            string arguments = $"images ls";
            ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
            ProcessSettings settings = new ProcessSettings
            {
                Arguments = argumentsBuilder
            };

            const int waitTimeSeconds = 10;
            const int maxTries = 12;
            bool success = false;
            for( int i = 1; ( i <= maxTries ) && ( success == false ); ++i )
            {
                context.Information( $"Waiting for Docker to start.  Attempt {i}/{maxTries}." );

                int exitCode = context.StartProcess( "docker", settings );
                success = ( exitCode == 0 );

                if( success == false )
                {
                    context.Information( $"Attempt {i} failed, trying again in {waitTimeSeconds} seconds." );
                    Thread.Sleep( TimeSpan.FromSeconds( waitTimeSeconds ) );
                }
            }

            if( success == false )
            {
                throw new TimeoutException(
                    "Docker did not start within time limit."
                );
            }

            context.Information( string.Empty );
            context.Information( "Docker is back!" );
        }
    }

    [TaskName( "build_windows_docker" )]
    public class BuildWindowsDocker : BaseDockerTask
    {
        public override void Run( ChaskisContext context )
        {
            DirectoryPath output = context.Paths.OutputPackages.Combine( new DirectoryPath( "windows" ) );
            output = output.Combine( new DirectoryPath( "docker" ) );

            DistroCreatorConfig config = new DistroCreatorConfig
            {
                OutputLocation = output.ToString(),
                Target = "Release"
            };

            DistroCreator creator = new DistroCreator( context, config );
            creator.CreateDistro();

            context.Information( "Running Docker..." );
            BuildDockerImage( context, windowsDockerImageName, ".\\Docker\\WindowsRuntime.Dockerfile" );
            TagDocker( context, windowsDockerImageName );
            context.Information( "Running Docker... Done!" );
        }
    }

    [TaskName( "build_ubuntu_docker" )]
    public class BuildUbuntusDocker : BaseDockerTask
    {
        public override void Run( ChaskisContext context )
        {
            BuildDockerImage( context, ubuntuDockerImageName, "./Docker/UbuntuRuntime.Dockerfile" );
            TagDocker( context, ubuntuDockerImageName );
        }
    }

    public abstract class BaseDockerTask : DefaultTask
    {
        // ---------------- Fields ----------------

        protected const string windowsDockerImageName = "xforever1313/chaskis.windows";
        protected const string ubuntuDockerImageName = "xforever1313/chaskis.ubuntu";

        // ---------------- Functions ----------------

        protected void BuildDockerImage( ChaskisContext context, string imageName, string dockerFile )
        {
            string arguments = $"build -t {imageName} -f {dockerFile} .";
            ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
            ProcessSettings settings = new ProcessSettings
            {
                Arguments = argumentsBuilder
            };
            int exitCode = context.StartProcess( "docker", settings );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    "Error when running docker to build.  Got error: " + exitCode
                );
            }
        }

        protected void TagDocker( ChaskisContext context, string imageName )
        {
            TemplateConstants templateConstants = new TemplateConstants( context );

            string arguments = $"tag {imageName}:latest {imageName}:{templateConstants.ChaskisVersion}";
            ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
            ProcessSettings settings = new ProcessSettings
            {
                Arguments = argumentsBuilder
            };
            int exitCode = context.StartProcess( "docker", settings );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    "Error when running docker to tag.  Got error: " + exitCode
                );
            }
        }
    }
}
