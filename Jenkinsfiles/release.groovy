// Pipeline to make a new release.
// This Pipeline requires the following be installed on a WINDOWS 10 PC (Install the following Chocolatey Packages):
// - docker-desktop
// - docker-cli
// - dotnetcore-sdk

// The following Windows features must be turned on:
// - Containers
// - Hyper-V
// - OpenSSH Server (Search for Optional Features in the Windows 10).  Can also install with Chocolatey via "choco install openssh -params "/SSHServerFeature /SSHAgentFeature""
//   This is so Jenkins can remote into the Windows machine to build.

// Jenkins Agent User Config:
// - The user Jenkins will SSH into the Windows machine as must be in the docker-users and adminstrator group.
//   If the user is not in the administrator group, dockercli.exe will not work in an SSH environment.  I have no idea why, it just won't.
// - Generate an SSH key with "ssh key-gen".  Make sure the public key is in the user's .ssh/authorized_keys file.
// - In c:\Program Data\ssh\sshd_config, comment out the following lines at the bottom (or make it work, not sure how though).
//   #Match Group administrators
//   #       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys
//
//   If this is uncommented, Jenkins will NOT be able to SSH in as your user; ssh wil reject it.  This is because the user Jenkins SSHes into
//   is in the admin group.
//   ... Docker for Windows is not a fun experience.

// Power Settings (Possibly not needed)
// Yes, we even need to change the Power Settings because Windows tries to be "helpful" but can put Docker into a bad state.
// This is to workaround a problem called out here: https://stackoverflow.com/questions/40668908/running-docker-for-windows-error-when-exposing-ports
// - Hit start and search for "Power".  Click on "Power & Sleep" settings.
// - On the right column, hit "Additional Power Settings".
// - On the Window that pops up, hit "Choose what the power buttons do"
// - Uncheck "Turn off fast startup"

// Before running
// - Ensure the Docker Engine is running by going to Start -> searching for "Services" and hitting enter.
//   You can have it start automatically by setting the Startup Type to "Automatic".
// - Also make sure the "Docker Desktop Service" is also running.

def distFolder = "dist"; // Staging to before we push to the website.
def buildDistFolder = ".\\Chaskis\\DistPackages"; // Where builds post their dists.
def archiveFolder = "archives";

def GetWebsiteCredsId()
{
    return "shendrick.net";
}

def GetAurCredsId()
{
    return "aur";
}

def UbuntuBuildImageName()
{
    return "chaskis.build.ubuntu";
}

def ArchBuildImageName()
{
    return "chaskis.build.archlinux";
}

def UseWindowsDockerForBuild()
{
    // Can't quite get the Windows build container working to generate .MSI's.  May need to wait for the next generation of WiX to be
    // built first.  The Windows PC has everything installed anyways, so meh.
    //
    // This needs to be a function since Jenkins is too stupid to read global variables.
    return false;
}

def WindowsSleep( Integer seconds )
{
    echo "Sleeping for ${seconds} seconds...";
    // This is apparently the only reliable way to sleep on a Windows PC >_>.
    bat "ping -n ${seconds} 127.0.0.1";
    echo "Sleeping for ${seconds} seconds... Done!";
}

///
/// Calls cake on the Jenkins Agent, with no docker container.
///
def CallCakeOnBuildMachine( String cmd )
{
    bat ".\\Cake\\dotnet-cake.exe .\\Chaskis\\build.cake ${cmd}"
}

///
/// Calls cake in the Windows docker container (or locally if there is no Docker container for Windows.)
///
def CallCakeOnWindows( String cmd )
{
    if( UseWindowsDockerForBuild() )
    {
        bat "docker run --mount type=bind,source=\"${pwd()}\\Chaskis\",target=c:\\Chaskis -i chaskis.build.windows c:\\Chaskis\\build.cake ${cmd}";

        // Sleep for 5 seconds to give containers a chance to cleanup.
        WindowsSleep( 5 );
    }
    else
    {
        CallCakeOnBuildMachine( cmd );
    }
}

///
/// Calls cake in a Linux docker container.
///
def CallCakeOnUbuntu( String cmd )
{
    // Mount to the container user's home directory so there are no permission issues.
    bat "docker run --mount type=bind,source=\"${pwd()}\\Chaskis\",target=/home/ContainerUser/chaskis/ -i ${UbuntuBuildImageName()} /home/ContainerUser/chaskis/build.cake ${cmd}"

    // Sleep for 5 seconds to give containers a chance to cleanup.
    WindowsSleep( 5 );
}

///
/// Calls cake in the Arch Linux docker container.
///
def CallCakeOnArchLinux( String cmd )
{
    // Mount to the container user's home directory so there are no permission issues.
    bat "docker run --mount type=bind,source=\"${pwd()}\\Chaskis\",target=/home/containeruser/chaskis/ -i ${ArchBuildImageName()} /home/containeruser/chaskis/build.cake ${cmd}"

    // Sleep for 5 seconds to give containers a chance to cleanup.
    WindowsSleep( 5 );
}

def CleanWindowsDirectory( String path )
{
    bat "IF EXIST \"${path}\" rmdir /S /Q \"${path}\""
    bat "mkdir \"${path}\""
}

def RunProcessIgnoreError( String cmd )
{
    try
    {
        bat cmd
    }
    catch( Exception )
    {

    }
}

def WaitForDockerReboot()
{
    CallCakeOnBuildMachine( "--target=wait_for_docker_to_start" );
}

def GetVersFile()
{
    return "version.txt";
}

def GetChaskisVersion()
{
    return readFile( GetVersFile() );
}

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

///
/// Apparently when Jenkins copies the SSH key to the secrets folder, it
/// doesn't restrict the permissions enough >_>.
/// Taken from : https://superuser.com/questions/1296024/windows-ssh-permissions-for-private-key-are-too-open
///
def SetupPrivateKey( String keyPath )
{
    // Remove Inheritance
    bat "icacls \"${keyPath}\" /c /t /Inheritance:d";

    // Set ownership to the current user
    bat "icacls \"${keyPath}\" /c /t /Grant ${UserName}:F";

    // Remove all users except for the current user.
    bat "icacls \"${keyPath}\" /c /t /Remove Administrator \"Authenticated Users\" BUILTIN\\Administrators BUILTIN Everyone System Users";

    // Verify (for debugging in the log if needed)
    bat "icacls \"${keyPath}\""
}

def PostDirectoryToWebsite( String localDirectory )
{
    withCredentials(
        [sshUserPrivateKey(
            credentialsId: GetWebsiteCredsId(),
            usernameVariable: "SSHUSER",
            keyFileVariable: "WEBSITE_KEY" // <- Note: WEBSITE_KEY must be in all quotes below, or SCP won't work if the path has whitespace.
        )]
    )
    {
        def releaseLocation = "/home/${SSHUSER}/files.shendrick.net/projects/chaskis/releases/${GetChaskisVersion()}/";

        SetupPrivateKey( WEBSITE_KEY );

        String verbose = "-v"; // Make "-v" for verbose mode.
        String options = "-o BatchMode=yes -o StrictHostKeyChecking=no -i \"${WEBSITE_KEY}\"";

        // Upload everything to the release folder.
        bat "scp ${verbose} -r ${options} ${localDirectory} ${SSHUSER}@shendrick.net:${releaseLocation}";

        // Make all uploaded directories 755
        bat "ssh ${verbose} ${options} ${SSHUSER}@shendrick.net find ${releaseLocation} -type d -exec chmod 755 {} +";

        // Make all uploaded files 644
        bat "ssh ${verbose} ${options} ${SSHUSER}@shendrick.net find ${releaseLocation} -type f -exec chmod 644 {} +"
    }
}

def SetSymlinksToLatest()
{
    withCredentials(
        [sshUserPrivateKey(
            credentialsId: GetWebsiteCredsId(),
            usernameVariable: "SSHUSER",
            keyFileVariable: "WEBSITE_KEY" // <- Note: WEBSITE_KEY must be in all quotes below, or SCP won't work if the path has whitespace.
        )]
    )
    {
        def releaseRoot = "/home/${SSHUSER}/files.shendrick.net/projects/chaskis/releases/";
        def releaseLocation = "${releaseRoot}${GetChaskisVersion()}/"

        SetupPrivateKey( WEBSITE_KEY );

        String verbose = "-v"; // Make "-v" for verbose mode.
        String options = "-o BatchMode=yes -o StrictHostKeyChecking=no -i \"${WEBSITE_KEY}\"";

        bat "ssh ${verbose} ${options} ${SSHUSER}@shendrick.net rm ${releaseRoot}latest";
        bat "ssh ${verbose} ${options} ${SSHUSER}@shendrick.net ln -s ${releaseLocation} ${releaseRoot}latest";
    }
}

def SetLatestOnWebsite()
{
    withCredentials(
        [sshUserPrivateKey(
            credentialsId: GetWebsiteCredsId(),
            usernameVariable: "SSHUSER",
            keyFileVariable: "WEBSITE_KEY" // <- Note: WEBSITE_KEY must be in all quotes below, or SCP won't work if the path has whitespace.
        )]
    )
    {
        def releaseLocation = "/home/${SSHUSER}/files.shendrick.net/projects/chaskis/releases/${GetChaskisVersion()}/";

        SetupPrivateKey( WEBSITE_KEY );

        String verbose = "-v"; // Make "-v" for verbose mode.
        String options = "-o BatchMode=yes -o StrictHostKeyChecking=no -i \"${WEBSITE_KEY}\"";

        bat "ssh ${verbose} ${options} ${SSHUSER}@shendrick.net rm /home/${SSHUSER}/files.shendrick.net/projects/chaskis/releases/latest";
        bat "ssh ${verbose} ${options} ${SSHUSER}@shendrick.net ln -s ${releaseLocation} /home/${SSHUSER}/files.shendrick.net/projects/chaskis/releases/latest";
    }
}

def GitCommit( String workingDir, String message, Boolean toggleIgnoreLineEndings )
{
    if( toggleIgnoreLineEndings )
    {
        bat "cd \"${workingDir}\" && git config core.autocrlf false";
    }
    bat "cd \"${workingDir}\" && git config user.email \"seth@shendrick.net\"";
    bat "cd \"${workingDir}\" && git config user.name \"Seth Hendrick\"";
    bat "cd \"${workingDir}\" && git commit -a -m \"${message}\"";
}

def GitPush( String repoLocation, String credId, String url )
{
    withCredentials(
        [sshUserPrivateKey(
            credentialsId: credId,
            usernameVariable: "SSHUSER",
            keyFileVariable: "GIT_KEY"
        )]
    )
    {
        SetupPrivateKey( GIT_KEY );
        bat "cd \"${repoLocation}\" && git config core.sshCommand \"ssh -o BatchMode=yes -o StrictHostKeyChecking=no -i \\\"${GIT_KEY}\\\"\"";
        bat "cd \"${repoLocation}\" && git push ${url} HEAD:master";
    }
}

pipeline
{
    agent
    {
        label "windows";
    }
    parameters
    {
        booleanParam( name: "BuildWindows", defaultValue: true, description: "Should we build for Windows?" );
        booleanParam( name: "BuildLinux", defaultValue: true, description: "Should we build for Linux?" );
        booleanParam( name: "RunUnitTests", defaultValue: true, description: "Should unit tests be run?" );
        booleanParam( name: "RunRegressionTests", defaultValue: true, description: "Should regression tests be run?" );
        booleanParam( name: "Deploy", defaultValue: true, description: "Should we deploy?" );
        booleanParam( name: "UploadToWebsite", defaultValue: true, description: "Should files be uploaded to the website?" );
        booleanParam( name: "CleanUp", defaultValue: true, description: "Should we clean up the workspace first?" );
        booleanParam( name: "Push", defaultValue: true, description: "Should we push everything to third party services?" );
    }
    stages
    {
        stage( 'checkout' )
        {
            steps
            {
                checkout poll: false, scm: [
                    $class: 'GitSCM', 
                    branches: [[name: '*/master']],
                    doGenerateSubmoduleConfigurations: false,
                    extensions: [
                        [$class: 'CleanBeforeCheckout'],
                        [$class: 'RelativeTargetDirectory', relativeTargetDir: 'Chaskis'],
                        [$class: 'CloneOption', depth: 0, noTags: true, reference: '', shallow: true],
                        [$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false]
                    ],
                    submoduleCfg: [],
                    userRemoteConfigs: [[url: 'https://github.com/xforever1313/Chaskis.git']]
                ]
            }
            when
            {
                expression
                {
                    return params.CleanUp;
                }
            }
        }
        stage( 'setup' )
        {
            steps
            {
                bat "dotnet tool update Cake.Tool --tool-path .\\Cake"
                CallCakeOnBuildMachine( "--showdescription" );
                CleanWindowsDirectory( pwd() + "\\${distFolder}" );
                CleanWindowsDirectory( pwd() + "\\${archiveFolder}" );
                CallCakeOnBuildMachine( "--target=dump_version --output=\"${pwd()}\\${GetVersFile()}\"" );
            }
            when
            {
                expression
                {
                    return params.CleanUp;
                }
            }
        }
        stage( 'Windows' )
        {
            stages
            {
                stage( 'make_build_container' )
                {
                    steps
                    {
                        // Make the Windows build docker container.
                        bat 'C:\\"Program Files"\\Docker\\Docker\\DockerCli.exe -Version';
                        bat 'C:\\"Program Files"\\Docker\\Docker\\DockerCli.exe -SwitchWindowsEngine';
                        WaitForDockerReboot(); // Wait to switch.
                        script
                        {
                            if( UseWindowsDockerForBuild() )
                            {
                                bat "docker build -t chaskis.build.windows -f .\\Chaskis\\Docker\\WindowsBuild.Dockerfile .\\Chaskis";
                            }
                        }
                    }
                }
                stage( 'build_debug' )
                {
                    steps
                    {
                        // Build Debug
                        CallCakeOnWindows( "--target=debug" );
                    }
                }
                stage( 'unit_test' )
                {
                    steps
                    {
                        CallCakeOnWindows( "--target=unit_test" );
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
                            ParseTestResults( "Chaskis\\TestResults\\UnitTests\\*.xml" );
                        }
                    }
                }
                stage( 'regression_test' )
                {
                    steps
                    {
                        CallCakeOnWindows( "--target=regression_test" );
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
                            ParseTestResults( "Chaskis\\TestResults\\RegressionTests\\*.xml" );
                            bat "MOVE .\\Chaskis\\TestResults\\RegressionTests\\Logs .\\${archiveFolder}\\windows_regression_logs";
                            archiveArtifacts "${archiveFolder}\\windows_regression_logs\\*.log";
                        }
                    }
                }
                stage( 'build_msi' )
                {
                    steps
                    {
                        CallCakeOnWindows( "--target=msi" );
                    }
                }
                stage( 'nuget_pack' )
                {
                    steps
                    {
                        CallCakeOnWindows( "--target=nuget_pack" );
                    }
                }
                stage( 'choco_pack' )
                {
                    steps
                    {
                        // Chocolatey needs the checksum to be updated.
                        // This file will be reverted during the Linux stage, but restored later during the deploy stage.
                        bat "COPY /Y ${buildDistFolder}\\windows\\ChaskisInstaller.msi.sha256 .\\Chaskis\\SavedChecksums\\ChaskisInstaller.msi.sha256"
                        CallCakeOnBuildMachine( "--target=template" );
                        CallCakeOnWindows( "--target=choco_pack" );
                    }
                }
                stage( 'make_runtime_container' )
                {
                    steps
                    {
                        bat ".\\Cake\\dotnet-cake.exe .\\Chaskis\\build.cake --target=build_windows_docker"
                    }
                }
                stage( 'Archive' )
                {
                    steps
                    {
                        // No need for Docker to stick around, delete.
                        bat "rmdir /S /Q ${buildDistFolder}\\windows\\docker"

                        // Move binaries over so they can be posted online.
                        bat "MOVE ${buildDistFolder}\\windows .\\${distFolder}\\windows"
                        bat "MOVE ${buildDistFolder}\\chocolatey .\\${distFolder}\\chocolatey"
                        bat "MOVE ${buildDistFolder}\\nuget .\\${distFolder}\\nuget"
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
        }// End Windows
        stage( "Linux" )
        {
            stages
            {
                stage( 'setup' )
                {
                    steps
                    {
                        // Wait a few seconds since sometimes we get a failure
                        // where we can't delete >_>.
                        WindowsSleep( 5 );
                        bat "cd Chaskis && git clean -dfx"
                    }
                }
                stage( 'Make Linux Build Container' )
                {
                    steps
                    {
                        // Make the Linux build docker container.
                        bat 'C:\\"Program Files"\\Docker\\Docker\\DockerCli.exe -SwitchLinuxEngine';
                        WaitForDockerReboot(); // Give some time to switch.
                        bat "docker build -t ${UbuntuBuildImageName()} -f .\\Chaskis\\Docker\\UbuntuBuild.Dockerfile .\\Chaskis";
                    }
                }
                stage( 'build_debug' )
                {
                    steps
                    {
                        // Build Debug
                        CallCakeOnUbuntu( "--target=debug" );
                    }
                }
                stage( 'unit_test' )
                {
                    steps
                    {
                        CallCakeOnUbuntu( "--target=unit_test" );
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
                            ParseTestResults( "Chaskis\\TestResults\\UnitTests\\*.xml" );
                        }
                    }
                }
                stage( 'regression_test' )
                {
                    steps
                    {
                        CallCakeOnUbuntu( "--target=regression_test" );
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
                            ParseTestResults( "Chaskis\\TestResults\\RegressionTests\\*.xml" );
                            bat "MOVE .\\Chaskis\\TestResults\\RegressionTests\\Logs .\\${archiveFolder}\\linux_regression_logs";
                            archiveArtifacts "${archiveFolder}\\linux_regression_logs\\*.log";
                        }
                    }
                }
                stage( 'build_release' )
                {
                    steps
                    {
                        CallCakeOnUbuntu( "--target=release" );
                    }
                }
                stage( 'make_deb' )
                {
                    steps
                    {
                        CallCakeOnUbuntu( "--target=debian_pack --deb_build_dir=/home/ContainerUser/deb" );
                    }
                }
                stage( 'make_runtime_container' )
                {
                    steps
                    {
                        bat ".\\Cake\\dotnet-cake.exe .\\Chaskis\\build.cake --target=build_ubuntu_docker";
                    }
                }
                stage( 'Archive' )
                {
                    steps
                    {
                        // Move binaries over so they can be posted online.
                        bat "MOVE ${buildDistFolder}\\debian .\\${distFolder}\\debian"
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
        } // End Linux
        stage( 'deploy' )
        {
            stages
            {
                stage( 'save_checksums' )
                {
                    steps
                    {
                        // When deploying, we need to make sure all checksums are up-to-date.
                        bat "COPY /Y ${distFolder}\\windows\\ChaskisInstaller.msi.sha256 .\\Chaskis\\SavedChecksums\\ChaskisInstaller.msi.sha256";
                        bat "COPY /Y ${distFolder}\\debian\\chaskis.deb.sha256 .\\Chaskis\\SavedChecksums\\chaskis.deb.sha256";
                        CallCakeOnBuildMachine( "--target=template" );
                    }
                }
                stage( 'upload_to_website' )
                {
                    steps
                    {
                        // PKGBUILD for Arch Linux requires files be deployed to the website first.
                        PostDirectoryToWebsite( distFolder );
                        SetLatestOnWebsite();
                    }
                    when
                    {
                        expression
                        {
                            return params.UploadToWebsite;
                        }
                    }
                }
                stage( 'Arch Linux Deploy' )
                {
                    steps
                    {
                        // Build the Arch Linux Docker image so we can make the PKGBUILD file.
                        // Because arch sources change *all the time*, need to build from scratch.
                        bat "docker build -t ${ArchBuildImageName()} -f .\\Chaskis\\Docker\\ArchBuild.Dockerfile .\\Chaskis";

                        // Run Cake in the docker image to make the PKGBUILD file.
                        CallCakeOnArchLinux( "--target=pkgbuild" ); 

                        // Move binaries over so they can be posted online.
                        bat "MOVE ${buildDistFolder}\\arch_linux .\\${distFolder}\\arch_linux";

                        script
                        {
                            if( params.UploadToWebsite )
                            {
                                PostDirectoryToWebsite( ".\\${distFolder}\\arch_linux" );
                            }
                        }

                        // Clone the AUR git repo.
                        checkout poll: false, scm: [
                            $class: 'GitSCM', 
                            branches: [[name: '*/master']],
                            doGenerateSubmoduleConfigurations: false,
                            extensions: [
                                [$class: 'CleanBeforeCheckout'],
                                [$class: 'RelativeTargetDirectory', relativeTargetDir: 'chaskis_aur'],
                                [$class: 'CloneOption', depth: 0, noTags: true, reference: '', shallow: true],
                                [$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false]
                            ],
                            submoduleCfg: [],
                            userRemoteConfigs: [[url: 'https://aur.archlinux.org/chaskis.git']]
                        ]

                        // Copy over the SRCINFO and PKGBUILD files.
                        bat "COPY /Y ${distFolder}\\arch_linux\\.SRCINFO .\\chaskis_aur\\.SRCINFO";
                        bat "COPY /Y ${distFolder}\\arch_linux\\PKGBUILD .\\chaskis_aur\\PKGBUILD";

                        // Commit && Push.
                        GitCommit( "chaskis_aur", "Deployed Version ${GetChaskisVersion()}", true );
                    }
                }
                stage( 'Commit' )
                {
                    steps
                    {
                        GitCommit( "Chaskis", "Deployed Version ${GetChaskisVersion()}", false );
                    }
                }
                stage( 'Release Notes' )
                {
                    steps
                    {
                        CallCakeOnBuildMachine( "--target=make_release_notes" );
                        archiveArtifacts "Chaskis\\ReleaseNotes.md";
                    }
                }
            }
            when
            {
                expression
                {
                    return params.Deploy;
                }
            }
        } // End Deploy
        stage( "Push" )
        {
            stages
            {
                stage( "push_github" )
                {
                    steps
                    {
                        GitPush( "Chaskis", GetWebsiteCredsId(), "git@github.com:xforever1313/Chaskis.git" );
                    }
                }
                stage( "push_aur" )
                {
                    steps
                    {
                        GitPush( "chaskis_aur", GetAurCredsId(), "ssh://aur@aur.archlinux.org/chaskis.git" );
                    }
                }
                stage( 'Deploy NuGet' )
                {
                    steps
                    {
                        withCredentials([string(credentialsId: 'chaskiscore_nuget_deploy', variable: 'nuget_api_key')])
                        {
                            bat "nuget push ${distFolder}\\nuget\\*.nupkg ${nuget_api_key}";
                        }
                    }
                }
                stage( 'Deploy Chocolatey' )
                {
                    steps
                    {
                        withCredentials([string(credentialsId: 'choco_api_key', variable: 'choco_api_key')])
                        {
                            bat "choco push ${distFolder}\\chocolatey\\*.nupkg -k ${choco_api_key} --source https://push.chocolatey.org/";
                        }
                    }
                }
                stage( "Set Symlinks")
                {
                    SetSymlinksToLatest();
                }
            }
            when
            {
                expression
                {
                    return params.Push;
                }
            }
        }
    }
}
