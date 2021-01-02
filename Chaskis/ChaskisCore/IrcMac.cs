//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using SethCS.Basic;

namespace Chaskis.Core
{
    public interface IIrcMac : IDisposable
    {
        // ---------------- Properties ----------------

        bool IsConnected { get; }

        // ---------------- Functions ----------------

        void Connect();

        void Disconnect();

        void WriteLine( string str );

        void WriteLine( string str, params object[] objs );

        string ReadLine();
    }

    /// <summary>
    /// This is the Media Access Control (MAC) layer of an IRC Connection.
    /// This is responsible for controlling how Chaskis.Core gains access to the socket and
    /// transmit or receive data.
    /// </summary>
    public class IrcMac : IIrcMac
    {
        // ---------------- Fields ----------------

        private readonly IReadOnlyIrcConfig config;

        private readonly GenericLogger log;

        private TcpClient connection;
        private SslStream sslStream;
        private StreamWriter ircWriter;
        private StreamReader ircReader;

        private bool isDisposed;

        // ---------------- Constructor ----------------

        public IrcMac( IReadOnlyIrcConfig config, GenericLogger log )
        {
            this.config = config;
            this.log = log;
            this.isDisposed = false;
        }

        // ---------------- Properties ----------------

        public bool IsConnected
        {
            get
            {
                return
                    ( this.connection != null ) &&
                    this.connection.Connected;
            }
        }

        // ---------------- Functions ----------------

        public void Connect()
        {
            this.DisposeCheck();
            if( this.connection != null )
            {
                throw new InvalidOperationException( "Already connected! can not connect again!" );
            }

            this.connection = new TcpClient( this.config.Server, this.config.Port );
            Stream stream;
            if( this.config.UseSsl )
            {
                this.log.WriteLine( "Using SSL Connection." );
                this.sslStream = new SslStream( this.connection.GetStream() );
                this.sslStream.AuthenticateAsClient( this.config.Server );
                stream = this.sslStream;
            }
            else
            {
                this.log.WriteLine( "WARNING! Using plain text connection." );
                stream = this.connection.GetStream();
            }

            this.ircReader = new StreamReader( stream );
            this.ircWriter = new StreamWriter( stream );
        }

        /// <summary>
        /// Closes the connection.  You are able to call <see cref="Connect"/>
        /// again to reopen the connection.
        /// 
        /// No-op if we are already disconnected.
        /// </summary>
        public void Disconnect()
        {
            this.DisposeCheck();
            this.connection?.Close();

            this.connection?.Dispose();
            this.sslStream?.Dispose();
            this.ircReader?.Dispose();
            this.ircWriter?.Dispose();

            this.connection = null;
            this.sslStream = null;
            this.ircReader = null;
            this.ircWriter = null;
        }

        /// <summary>
        /// Once called, you must create a new instance of this object.  Calling
        /// <see cref="Connect"/> or any other function will throw an <see cref="ObjectDisposedException"/>
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.Disconnect();
            }
            catch( Exception e )
            {
                this.log.ErrorWriteLine( "Error when disconnecting:" + Environment.NewLine + e.ToString() );
            }
            finally
            {
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Writes the given sting to the IRC server.
        /// Not thread safe.
        /// </summary>
        public void WriteLine( string str )
        {
            this.DisposeCheck();

            this.ircWriter.WriteLine( str );
            this.ircWriter.Flush();
        }

        public void WriteLine( string str, params object[] objs )
        {
            this.DisposeCheck();

            this.ircWriter.WriteLine( str, objs );
            this.ircWriter.Flush();
        }

        public string ReadLine()
        {
            this.DisposeCheck();

            return this.ircReader.ReadLine();
        }

        private void DisposeCheck()
        {
            if( this.isDisposed )
            {
                throw new ObjectDisposedException( nameof( IrcMac ) );
            }
        }
    }
}
