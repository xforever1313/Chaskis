//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Diagnostics;
using System.IO;
using NetRunner.ExternalLibrary;
using SethCS.Basic;

namespace Chaskis.RegressionTests
{
    /// <summary>
    /// This represents the instance of Chaskis.
    /// </summary>
    public class ChaskisProcess : BaseTestContainer
    {
        // ---------------- Fields ----------------

        private Process process;

        private ProcessStartInfo startInfo;

        private GenericLogger consoleOutLog;
        private GenericLogger consoleErrorLog;

        private string exeLocation;

        // ---------------- Constructor ----------------

        public ChaskisProcess()
        {
            this.consoleOutLog = Logger.GetLogFromContext( "chaskis" );
            this.consoleErrorLog = Logger.GetLogFromContext( "chaskis_error" );

            this.exeLocation = Path.Combine(
                EnvironmentManager.ChaskisRoot,
                "Chaskis",
                "bin",
                "Debug",
                "Chaskis.exe"
            );

            this.consoleOutLog.WriteLine( "Chaskis.exe Location: " + this.exeLocation );
            this.startInfo = new ProcessStartInfo(
                this.exeLocation
                // TODO: Add Args to use Test Environment, not appdata.
            );

            this.startInfo.RedirectStandardInput = true;
            this.startInfo.RedirectStandardOutput = true;
            this.startInfo.RedirectStandardError = true;
            this.startInfo.UseShellExecute = false;
        }

        ~ChaskisProcess()
        {
            try
            {
                this.process?.Kill();
            }
            catch( Exception )
            {
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Starts the Chaskis Process using the Test Environment.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the process is already started.</exception>
        public bool StartProcess()
        {
            if( this.process != null )
            {
                throw new InvalidOperationException( "Process is already started!" );
            }

            this.process = new Process();
            this.process.OutputDataReceived += Process_OutputDataReceived;
            this.process.ErrorDataReceived += Process_ErrorDataReceived;
            this.process.StartInfo = this.startInfo;
            this.process.Start();
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();

            return true;
        }

        private void Process_OutputDataReceived( object sender, DataReceivedEventArgs e )
        {
            if( string.IsNullOrEmpty( e.Data ) == false )
            {
                this.consoleOutLog.WriteLine( e.Data );
            }
        }

        private void Process_ErrorDataReceived( object sender, DataReceivedEventArgs e )
        {
            if( string.IsNullOrEmpty( e.Data ) == false )
            {
                this.consoleErrorLog.WriteLine( e.Data );
            }
        }

        public bool StopProcess()
        {
            return this.StopProcess( TestConstants.DefaultTimeout );
        }

        /// <summary>
        /// Stops the process.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the process is not started.</exception>
        public bool StopProcess( int timeout )
        {
            if( this.process == null )
            {
                throw new InvalidOperationException( "There is no process to stop!" );
            }

            this.process.StandardInput.Write( Environment.NewLine );
            bool success = this.process.WaitForExit( timeout );
            if( success )
            {
                this.process.Close();
                this.process = null;
            }

            return success;
        }

        public bool KillProcess()
        {
            return this.KillProcess( TestConstants.DefaultTimeout );
        }

        /// <summary>
        /// Force Kills the process.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the process is not started.</exception>
        public bool KillProcess( int timeout = TestConstants.DefaultTimeout )
        {
            if( this.process == null )
            {
                throw new InvalidOperationException( "There is no process to kill!" );
            }

            this.process.Kill();

            bool success = this.process.WaitForExit( timeout );
            if( success )
            {
                this.process.Close();
                this.process = null;
            }

            return success;
        }
    }
}
