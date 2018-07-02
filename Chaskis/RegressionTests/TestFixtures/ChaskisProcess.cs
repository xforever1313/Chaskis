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
        private GenericLogger testConsoleOutLog;

        private StringBuffer buffer;

        private string exeLocation;

        // ---------------- Constructor ----------------

        public ChaskisProcess()
        {
            this.consoleOutLog = Logger.GetLogFromContext( "chaskis" );
            this.consoleErrorLog = Logger.GetLogFromContext( "chaskis_error" );
            this.testConsoleOutLog = Logger.GetLogFromContext( "chaskis_status" );

            this.exeLocation = Path.Combine(
                EnvironmentManager.ChaskisDistDir,
                "Chaskis",
                "bin",
                "Chaskis.exe"
            );

            this.consoleOutLog.WriteLine( "Chaskis.exe Location: " + this.exeLocation );
            this.startInfo = new ProcessStartInfo(
                this.exeLocation,
                "--chaskisroot=" + EnvironmentManager.TestEnvironmentDir
            );

            this.startInfo.RedirectStandardInput = true;
            this.startInfo.RedirectStandardOutput = true;
            this.startInfo.RedirectStandardError = true;
            this.startInfo.UseShellExecute = false;

            this.buffer = new StringBuffer();
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

            return ( this.process != null );
        }

        private void Process_OutputDataReceived( object sender, DataReceivedEventArgs e )
        {
            if( string.IsNullOrEmpty( e.Data ) == false )
            {
                this.consoleOutLog.WriteLine( e.Data );
                this.buffer.EnqueueString( e.Data );
            }
        }

        private void Process_ErrorDataReceived( object sender, DataReceivedEventArgs e )
        {
            if( string.IsNullOrEmpty( e.Data ) == false )
            {
                this.consoleErrorLog.WriteLine( e.Data );
                this.buffer.EnqueueString( e.Data );
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

        public bool StopOrKillProcess()
        {
            return this.StopOrKillProcess( TestConstants.DefaultTimeout );
        }

        public bool StopOrKillProcess( int timeout )
        {
            if( this.process == null )
            {
                return true;
            }

            bool success = this.StopProcess( timeout );
            if( success == false )
            {
                this.testConsoleOutLog.WriteLine( "Could Not Stop Process, killing." );
                success &= this.KillProcess( timeout );
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

        /// <summary>
        /// Waits for a string that matches the given regex pattern to appear.
        /// </summary>
        /// <param name="regex">The regex to search for.</param>
        /// <returns>True if we found a match before the timeout, else false.</returns>
        public bool WaitForStringFromChaskis( string regex )
        {
            return this.WaitForStringFromChaskisWithTimeout( regex, TestConstants.DefaultTimeout );
        }

        /// <summary>
        /// Waits for a string that matches the given regex pattern to appear.
        /// </summary>
        /// <param name="regex">The regex to search for.</param>
        /// <param name="timeout">How long to wait before giving up.</param>
        /// <returns>True if we found a match before the timeout, else false.</returns>
        public bool WaitForStringFromChaskisWithTimeout( string regex, int timeout )
        {
            this.testConsoleOutLog.WriteLine( "Waiting for string '" + regex + "' from Chaskis Process..." );
            bool success = this.buffer.WaitForString( regex, timeout );
            this.testConsoleOutLog.WriteLine(
                "Waiting for string '" + regex + "' from Chaskis Process...{0}",
                success ? "Done!" : "Fail!"
            );

            return success;
        }
    }
}
