//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Text;
using System.Threading;
using SethCS.Basic;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Plugins.IrcLogger
{
    /// <summary>
    /// This is the class that managers and writes to logs.
    /// </summary>
    public class LogManager : IDisposable
    {
        // -------- Fields --------

        /// <summary>
        /// The configuration to use.
        /// </summary>
        private readonly IrcLoggerConfig config;

        /// <summary>
        /// The current number of messages written to the log.
        /// </summary>
        private uint currentLineCount;

        /// <summary>
        /// The file stream to write to.
        /// </summary>
        private FileStream outFile;

        /// <summary>
        /// The stream writer for the out file.
        /// </summary>
        private StreamWriter outFileWriter;

        /// <summary>
        /// IO to the file system isn't cheap, let's background it.
        /// </summary>
        private readonly EventExecutor writerThread;

        private bool isDisposed;

        private readonly GenericLogger statusLog;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The config to use.</param>
        /// <param name="statusLog">The log to use for reporting status.</param>
        public LogManager( IrcLoggerConfig config, GenericLogger statusLog )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            this.config = config;
            this.currentLineCount = 0;
            this.CurrentFileName = string.Empty;
            this.LastFileName = string.Empty;

            if( Directory.Exists( config.LogFileLocation ) == false )
            {
                Directory.CreateDirectory( config.LogFileLocation );
            }

            this.statusLog = statusLog;

            this.writerThread = new EventExecutor( "IRC Logger" );

            this.writerThread.OnError += WriterThread_OnError;

            this.writerThread.Start();
            this.isDisposed = false;
        }

        private void WriterThread_OnError( Exception e )
        {
            this.statusLog.ErrorWriteLine(
                "Caught Exception while writing to file in IRC Logger: " + Environment.NewLine + e.ToString()
            );
        }

        ~LogManager()
        {
            try
            {
                this.Dispose( false );
            }
            catch( Exception )
            {
                // Don't let the GC thread die.
            }
        }

        // -------- Properties --------

        /// <summary>
        /// The current file name open.
        /// string.Empty if no file is open.
        /// </summary>
        public string CurrentFileName { get; private set; }

        /// <summary>
        /// The previous file logged to before the current one.
        /// string.Empty if there was no previous file open.
        /// </summary>
        public string LastFileName { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Starts writing a message in the background event queue.
        /// </summary>
        public void AsyncLogToFile( string message )
        {
            this.writerThread.AddEvent( () => this.LogToFileInternal( message ) );
        }

        /// <summary>
        /// Only recommended to use during Unit Testing.
        /// Writes the message to a file in the calling thread.
        /// </summary>
        public void SyncLogToFile( string message )
        {
            this.LogToFileInternal( message );
        }

        /// <summary>
        /// Logs the given string to the open file.
        /// </summary>
        /// <param name="message">The message to add.</param>
        private void LogToFileInternal( string message )
        {
            DateTime timeStamp = DateTime.UtcNow;

            // If this is the first line we are writting to, we need to create the file.
            if( string.IsNullOrEmpty( this.CurrentFileName ) )
            {
                this.CreateNewFile( timeStamp );
                this.outFileWriter.WriteLine( "############################################################" );
                this.outFileWriter.WriteLine( "New instance of Chaskis launched." );
                this.outFileWriter.WriteLine( "############################################################" );
            }

            StringBuilder builder = new StringBuilder();
            foreach( char ch in message )
            {
                if( char.IsControl( ch ) )
                {
                    builder.AppendFormat( "[0x{0}]", Convert.ToInt32( ch ).ToString( "X4" ) );
                }
                else
                {
                    builder.Append( ch );
                }
            }

            this.outFileWriter.WriteLine( timeStamp.ToTimeStampString() + "  " + builder.ToString() );

            ++this.currentLineCount;

            // If we reached the maximum log size, time to rotate, but
            // only if the user didn't tell us to rotate (set the message count to 0).
            if( this.config.MaxNumberMessagesPerLog > 0 )
            {
                if( this.currentLineCount >= this.config.MaxNumberMessagesPerLog )
                {
                    // Spin until we get a new timestamp.
                    while( DateTime.UtcNow.Equals( timeStamp ) )
                    {
                        Thread.Sleep( 15 );
                    };

                    timeStamp = DateTime.UtcNow;

                    this.outFileWriter.WriteLine( "############################################################" );
                    this.outFileWriter.WriteLine( "Maximum Size reached.  Continuing in " + GenerateFileName( timeStamp ) );
                    this.outFileWriter.WriteLine( "############################################################" );
                    this.outFileWriter.Flush();

                    this.outFileWriter.Dispose();
                    this.LastFileName = this.CurrentFileName;

                    CreateNewFile( timeStamp );

                    this.outFileWriter.WriteLine( "############################################################" );
                    this.outFileWriter.WriteLine( "Continuation of logs from " + this.LastFileName );
                    this.outFileWriter.WriteLine( "############################################################" );

                    // Remember to reset the line count unless you want a new file for
                    // everyline (oops).
                    this.currentLineCount = 0;
                }
            }

            // We should flush each time this function is called in case
            // of a crash.  We don't want missing log messages.  That wouldn't be cool.
            // We may have a small performance impact, but its better than losing data!
            //
            // However, it might not write to the file write away still!  According to a comment
            // on this stackoverflow: http://stackoverflow.com/questions/2417978/what-is-the-difference-between-streamwriter-flush-and-streamwriter-close
            // Flush writes to the OS's write-delay disk cache instead of directly to disk.
            // We can't even write to disk since there's a bug in .net according to that same stackoverflow answer.
            // Oh well, call flush and hope for the best!
            this.outFileWriter.Flush();
        }

        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="timeStamp">The timestamp from calling this.LogToFile()</param>
        private void CreateNewFile( DateTime timeStamp )
        {
            bool isNewFile = false;
            string newFileName = string.Empty;

            // Keep generating file names until we get one that does not exist.
            // its possible that our computer is small enough and our log file sizes are
            // small enough that it can screw with the file size.
            while( isNewFile == false )
            {
                this.CurrentFileName = GenerateFileName( timeStamp );
                newFileName = Path.Combine( this.config.LogFileLocation, this.CurrentFileName );
                if( File.Exists( newFileName ) == false )
                {
                    isNewFile = true;
                }
                else
                {
                    timeStamp = DateTime.UtcNow;
                }
            }

            this.outFile = new FileStream(
                newFileName,
                FileMode.Create,
                FileAccess.Write
            );
            this.outFileWriter = new StreamWriter( this.outFile );
        }

        /// <summary>
        /// Generates the file name to use.
        /// </summary>
        /// <param name="timeStamp">The timestamp from calling this.LogToFile()</param>
        /// <returns>The file name with the timestamp.</returns>
        private string GenerateFileName( DateTime timeStamp )
        {
            return string.Format(
                "{0}.{1}.log",
                this.config.LogName,
                timeStamp.ToString( "yyyy-MM-dd_HH-mm-ss-ffff" )
            );
        }

        /// <summary>
        /// Closes all log files.
        /// </summary>
        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected void Dispose( bool fromDispose )
        {
            if( this.isDisposed )
            {
                return;
            }

            try
            {
                if( fromDispose )
                {
                    ManualResetEvent doneEvent = new ManualResetEvent( false );
                    this.writerThread.AddEvent( () => doneEvent.Set() );

                    if( doneEvent.WaitOne( 15 * 1000 ) == false )
                    {
                        this.statusLog.ErrorWriteLine( "IRC Logger hung for 15 seconds during tear down, giving up." );
                    }

                    this.writerThread.Dispose();

                    // Closes the underlying stream as well.
                    this.outFileWriter?.Dispose();

                    this.writerThread.OnError -= this.WriterThread_OnError;
                }
                else
                {
                    // If we are from the finallizer, forget all the events,
                    // Just stop the event queue.
                    this.writerThread.Dispose();
                }
            }
            finally
            {
                this.isDisposed = true;
            }
        }
    }
}