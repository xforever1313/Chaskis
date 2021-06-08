@Library( "X13JenkinsLib" )_

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

def CallCake( String arguments )
{
    RunCommand( "./Cake/dotnet-cake ./checkout/build.cake ${arguments}" );
}

def BuildAndTest( Map params )
{
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
                checkout scm;
            }
        }
        stage( 'In Docker' )
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
                        RunCommand( 'dotnet tool update Cake.Tool --tool-path ./Cake' )
                        CallCake( "--showdescription" )
                    }
                }
                stage( 'build' )
                {
                    steps
                    {
                        CallCake( "--target=debug" );
                    }
                }
                stage( 'unit_test' )
                {
                    steps
                    {
                        CallCake( "--target=unit_test" );
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
            }
        }
    }
}

pipeline
{
    agent
    {
        label none
    }
    environment
    {
        DOTNET_CLI_TELEMETRY_OPTOUT = 'true'
        DOTNET_NOLOGO = 'true'
    }
    parameters
    {
        booleanParam( name: "BuildWindows", defaultValue: true, description: "Should we build for Windows?" );
        booleanParam( name: "BuildLinux", defaultValue: true, description: "Should we build for Linux?" );
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
                        label "windows && docker && x64";
                    }
                    BuildAndTest( params );
                }
                when
                {
                    expression
                    {
                        return params.BuildWindows;
                    }
                }

                stage( 'Linux' )
                {
                    agent
                    {
                        label "ubuntu && docker && x64";
                    }
                    BuildAndTest( params );
                }
                when
                {
                    expression
                    {
                        return params.BuildLinux;
                    }
                }
            }
        }
    }
}