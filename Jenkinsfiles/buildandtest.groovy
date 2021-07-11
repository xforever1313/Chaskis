@Library( "X13JenkinsLib" )_

def archiveFolder = "archives";

def ParseTestResults( String filePattern )
{
    def results = xunit thresholds: [
        failed(failureNewThreshold: '0', failureThreshold: '0', unstableNewThreshold: '0', unstableThreshold: '0')
    ], tools: [
        MSTest(
            deleteOutputFiles: true,
            failIfNotNew: true,
            pattern: filePattern,
            skipNoTestFiles: true,
            stopProcessingIfError: true
        )
    ]
}

def RunCommand( String cmd )
{
    if( isUnix() )
    {
        sh cmd;
    }
    else
    {
        bat cmd;
    }
}

def CleanDirectory( String path )
{
    CallDevops( "--target=clean_directory --path=\"${path}\"" );
}

def CallCake( String arguments )
{
    if( isUnix() )
    {
        RunCommand( "./Cake/dotnet-cake ./checkout/build.cake ${arguments}" );
    }
    else
    {
        RunCommand( ".\\Cake\\dotnet-cake .\\checkout\\build.cake ${arguments}" );
    }
}

def CallDevops( String arguments )
{
    RunCommand( "dotnet ./checkout/Chaskis/DevOps/bin/Debug/netcoreapp3.1/DevOps.dll ${arguments}" );
}

def Prepare()
{
    RunCommand( 'dotnet tool update Cake.Tool --tool-path ./Cake' )
    CallCake( "--showdescription" )
}

def Build()
{
    CallCake( "--target=build" );
}

def RunUnitTests()
{
    CallDevops( "--target=unit_test" );
}

def BuildRelease()
{
    CallDevops( "--target=build_release" );
}

def RunRegressionTests()
{
    CallDevops( "--target=regression_test" );
}

def GetVersFile()
{
    return "version.txt";
}

def GetChaskisVersion()
{
    return readFile( GetVersFile() );
}

pipeline
{
    agent none
    environment
    {
        DOTNET_CLI_TELEMETRY_OPTOUT = 'true'
        DOTNET_NOLOGO = 'true'
    }
    parameters
    {
        booleanParam( name: "BuildWindows", defaultValue: true, description: "Should we build for Windows?" );
        booleanParam( name: "BuildLinux", defaultValue: true, description: "Should we build for Linux?" );
        booleanParam( name: "BuildArchLinux", defaultValue: true, description: "Should we build for Arch Linux?" );
        booleanParam( name: "BuildDocker", defaultValue: true, description: "Should we build Docker Containers?" );
        booleanParam( name: "RunUnitTests", defaultValue: true, description: "Should unit tests be run?" );
        booleanParam( name: "RunRegressionTests", defaultValue: true, description: "Should regression tests be run?" );
    }
    options
    {
        skipDefaultCheckout( true );
    }
    stages
    {
        stage( 'Build & Test' )
        {
            parallel
            {
                stage( 'Windows' )
                {
                    agent
                    {
                        label "windows && x64";
                    }
                    stages
                    {
                        // Jenkins doesn't seem to like running Docker in
                        // and agent... it just seems to just hang.
                        // https://issues.jenkins.io/browse/JENKINS-59893
                        // Also, WiX doesn't seem to work in Docker for Windows either.
                        // Therefore, just run on the Windows agent directly.
                        stage( 'clean' )
                        {
                            steps
                            {
                                cleanWs();
                            }
                        }
                        stage( 'checkout' )
                        {
                            steps
                            {
                                // TODO: put this back in once configured in Jenkins
                                // checkout scm;
                                checkout([$class: 'GitSCM', branches: [[name: '*/master']], extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'checkout'], [$class: 'CleanCheckout'], [$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false]], userRemoteConfigs: [[url: 'https://github.com/xforever1313/Chaskis.git']]])
                            }
                        }
                        stage( 'prepare' )
                        {
                            steps
                            {
                                Prepare();
                            }
                        }
                        stage( 'build' )
                        {
                            steps
                            {
                                Build();
                                CallDevops( "--target=dump_version --output=\"${pwd()}\\${GetVersFile()}\"" );
                                CleanDirectory( pwd() + "\\${archiveFolder}" );
                            }
                        }
                        stage( 'unit_test' )
                        {
                            steps
                            {
                                RunUnitTests();
                            }
                            when
                            {
                                expression
                                {
                                    return params.RunUnitTests;
                                }
                            }
                            post
                            {
                                always
                                {
                                    ParseTestResults( "checkout/TestResults/UnitTests/*.xml" );
                                }
                            }
                        }
                        stage( 'Build Msi' )
                        {
                            steps
                            {
                                CallDevops( "--target=build_msi" );
                            }
                        }
                        stage( 'Regression Test' )
                        {
                            steps
                            {
                                RunRegressionTests();
                            }
                            when
                            {
                                expression
                                {
                                    return params.RunRegressionTests;
                                }
                            }
                            post
                            {
                                always
                                {
                                    ParseTestResults( "checkout\\TestResults\\RegressionTests\\*.xml" );
                                    bat "MOVE .\\checkout\\TestResults\\RegressionTests\\Logs .\\${archiveFolder}\\windows_regression_logs";
                                    archiveArtifacts "${archiveFolder}\\windows_regression_logs\\*.log";
                                }
                            }
                        }
                        stage( 'NuGet Pack' )
                        {
                            steps
                            {
                                CallDevops( "--target=nuget_pack" );
                            }
                        }
                        stage( 'Chocolatey Pack' )
                        {
                            steps
                            {
                                CallDevops( "--target=choco_pack" );
                            }
                        }
                        stage( 'Docker Build' )
                        {
                            steps
                            {
                                CallDevops( "--target=build_windows_docker" );
                            }
                            when
                            {
                                expression
                                {
                                    return params.BuildDocker;
                                }
                            }
                        }
                    }
                    when
                    {
                        expression
                        {
                            return params.BuildWindows;
                        }
                    }
                    post
                    {
                        always
                        {
                            archiveArtifacts "checkout\\DistPackages\\**\\*"
                        }
                    }
                }

                // ---------------- Linux ----------------

                stage( 'Linux' )
                {
                    agent
                    {
                        label "ubuntu && docker && x64";
                    }
                    stages
                    {
                        stage( 'clean' )
                        {
                            steps
                            {
                                cleanWs();
                            }
                        }
                        stage( 'checkout' )
                        {
                            steps
                            {
                                // TODO: put this back in once configured in Jenkins
                                // checkout scm;
                                checkout([$class: 'GitSCM', branches: [[name: '*/master']], extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'checkout'], [$class: 'CleanCheckout'], [$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false]], userRemoteConfigs: [[url: 'https://github.com/xforever1313/Chaskis.git']]])
                            }
                        }
                        stage( 'In Dotnet Docker' )
                        {
                            agent
                            {
                                docker
                                {
                                    image 'mcr.microsoft.com/dotnet/sdk:3.1'
                                    args "-e HOME='${env.WORKSPACE}'"
                                    reuseNode true
                                }
                            }
                            stages
                            {
                                stage( 'prepare' )
                                {
                                    steps
                                    {
                                        Prepare();
                                    }
                                }
                                stage( 'build' )
                                {
                                    steps
                                    {
                                        Build();
                                        CallDevops( "--target=dump_version --output=\"${pwd()}/${GetVersFile()}\"" );
                                        CleanDirectory( pwd() + "/${archiveFolder}" );
                                        stash includes: GetVersFile(), name: 'version'
                                    }
                                }
                                stage( 'unit_test' )
                                {
                                    steps
                                    {
                                        RunUnitTests();
                                    }
                                    when
                                    {
                                        expression
                                        {
                                            return params.RunUnitTests;
                                        }
                                    }
                                    post
                                    {
                                        always
                                        {
                                            ParseTestResults( "checkout/TestResults/UnitTests/*.xml" );
                                        }
                                    }
                                }
                                stage( 'Build Release' )
                                {
                                    steps
                                    {
                                        BuildRelease();
                                    }
                                }
                                stage( 'Regression Test' )
                                {
                                    steps
                                    {
                                        RunRegressionTests();
                                    }
                                    when
                                    {
                                        expression
                                        {
                                            return params.RunRegressionTests;
                                        }
                                    }
                                    post
                                    {
                                        always
                                        {
                                            ParseTestResults( "checkout/TestResults/RegressionTests/*.xml" );
                                            sh "cp -r ./checkout/TestResults/RegressionTests/Logs/ ./${archiveFolder}/linux_regression_logs";
                                            archiveArtifacts "${archiveFolder}/linux_regression_logs/*.log";
                                        }
                                    }
                                }
                                stage( 'Debian Pack' )
                                {
                                    steps
                                    {
                                        CallDevops( "--target=debian_pack" );
                                        stash includes: "checkout/DistPackages/debian/*.deb", name: 'deb'
                                    }
                                }
                                stage( 'Fedora Spec File' )
                                {
                                    steps
                                    {
                                        CallDevops( "--target=specfile" );
                                    }
                                }
                            }
                        }
                        stage( 'In Arch Docker' )
                        {
                            agent
                            {
                                dockerfile
                                {
                                    filename 'ArchBuild.Dockerfile'
                                    dir 'checkout/Docker'
                                    label 'chaskis-arch-buildenv'
                                    args "-e HOME='${env.WORKSPACE}'"
                                    additionalBuildArgs "--no-cache"
                                    reuseNode true
                                }
                            }
                            stages
                            {
                                stage( 'arch pkgbuild' )
                                {
                                    steps
                                    {
                                        CallDevops( "--target=pkgbuild" );
                                    }
                                }
                            }
                            when
                            {
                                expression
                                {
                                    return params.BuildArchLinux;
                                }
                            }
                        }
                        stage( 'Docker Build' )
                        {
                            steps
                            {
                                sh "docker build -t xforever1313/chaskis.ubuntu -f checkout/Docker/UbuntuRuntime.Dockerfile checkout";
                                sh "docker tag xforever1313/chaskis.ubuntu:latest xforever1313/chaskis.ubuntu:${GetChaskisVersion()}";
                            }
                            when
                            {
                                expression
                                {
                                    return params.BuildDocker;
                                }
                            }
                        }
                    }
                    when
                    {
                        expression
                        {
                            return params.BuildLinux;
                        }
                    }
                    post
                    {
                        always
                        {
                            archiveArtifacts "checkout/DistPackages/**/*"
                        }
                    }
                }
            } // End parallel
        } // End Build + Test Stage
        stage( "Build Raspbian Docker" )
        {
            agent
            {
                label "pi && docker && linux"
            }
            stages
            {
                stage( 'Build Docker' )
                {
                    steps
                    {
                        unstash "deb"
                        unstash "version"

                        sh "ls -l"
                        sh "ls -l /checkout"
                        sh "ls -l /checkout/DistPackages"
                    }
                }
            }
            when
            {
                expression
                {
                    return params.BuildLinux && params.BuildDocker;
                }
            }
        }
    }
}