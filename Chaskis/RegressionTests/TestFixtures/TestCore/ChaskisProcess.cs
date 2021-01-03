//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    /// <summary>
    /// This represents the instance of Chaskis.
    /// </summary>
    public class ChaskisProcess : IDisposable
    {
        // ---------------- Fields ----------------

        private Process process;

        private bool isDisposed;

        private readonly ProcessStartInfo startInfo;

        private readonly GenericLogger consoleOutLog;
        private readonly GenericLogger consoleErrorLog;
        private readonly GenericLogger testConsoleOutLog;

        private readonly StringBuffer buffer;

        private readonly string exeLocation;
        private readonly string dllLocation;
        private readonly string exeFile;

        // ---------------- Constructor ----------------

        public ChaskisProcess( string distDir, string environmentDir )
        {
            this.consoleOutLog = Logger.GetLogFromContext( "chaskis" );
            this.consoleErrorLog = Logger.GetLogFromContext( "chaskis_error" );
            this.testConsoleOutLog = Logger.GetLogFromContext( "chaskis_status" );

            this.exeLocation = Path.Combine(
                distDir,
                "bin"
            );

            this.dllLocation = Path.Combine(
                exeLocation,
                "Chaskis.dll"
            );

            string exeString;
            if( Environment.OSVersion.Platform == PlatformID.Win32NT )
            {
                exeString = ".exe";
            }
            else
            {
                exeString = string.Empty;
            }

            this.exeFile = Path.Combine(
                exeLocation,
                "Chaskis" + exeString
            );

            this.consoleOutLog.WriteLine( "Chaskis.exe Location: " + this.exeFile );
            this.startInfo = new ProcessStartInfo(
                this.exeFile,
                $"--chaskisroot=\"{environmentDir}\""
            )
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            this.buffer = new StringBuffer();

            this.isDisposed = false;
        }

        ~ChaskisProcess()
        {
            try
            {
                this.Dispose( false );
            }
            catch( Exception )
            {
            }
        }

        // ---------------- Functions ----------------

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected void Dispose( bool fromDispose )
        {
            if( this.isDisposed == false )
            {
                // Release unmanaged code here.
                this.process?.Kill();

                if( fromDispose )
                {
                    // Release managed code here.
                    this.process?.Dispose();
                }

                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Starts the Chaskis Process using the Test Environment.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the process is already started.</exception>
        public void StartProcess()
        {
            this.buffer.FlushQueue();

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

        /// <summary>
        /// Stops the process.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the process is not started.</exception>
        public bool StopProcess( int timeout = TestConstants.DefaultTimeout )
        {
            if( this.process == null )
            {
                throw new InvalidOperationException( "There is no process to stop!" );
            }

            this.process.StandardInput.WriteLine( "q" );
            return this.Wait( timeout );
        }

        /// <summary>
        /// Waits for the process to stop. This really should only be called
        /// if we are expecting the process to exit on its own.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Wait( int timeout = TestConstants.DefaultTimeout )
        {
            if( this.process == null )
            {
                throw new InvalidOperationException( "There is no process to wait for!" );
            }

            bool success = this.process.WaitForExit( timeout );
            if( success )
            {
                this.process.Close();
                this.process = null;
            }

            return success;
        }

        public bool StopOrKillProcess( int timeout = TestConstants.DefaultTimeout )
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
        /// <param name="timeout">How long to wait before giving up.</param>
        /// <returns>True if we found a match before the timeout, else false.</returns>
        public bool WaitForStringFromChaskis( string regex, int timeout = TestConstants.DefaultTimeout )
        {
            this.testConsoleOutLog.WriteLine( "Waiting for string '" + regex + "' from Chaskis Process..." );
            bool success = this.buffer.WaitForString( regex, timeout );
            this.testConsoleOutLog.WriteLine(
                "Waiting for string '" + regex + "' from Chaskis Process...{0}",
                success ? "Done!" : "Fail!"
            );

            return success;
        }

        /// <summary>
        /// Creates a <see cref="StringWatcher"/> to watch for strings
        /// from the Chaskis Process.  Remember to call <see cref="StringWatcher.Dispose"/>
        /// when done.
        /// </summary>
        public StringWatcher CreateStringWatcher( string regex )
        {
            return new StringWatcher( this.buffer, regex, this.testConsoleOutLog );
        }
    }
}
