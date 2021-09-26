//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Threading.Tasks;
using SethCS.Basic;

namespace Chaskis.Core
{
    internal sealed class MacWriter : ChaskisActionChannel
    {
        // ---------------- Fields ----------------

        private readonly GenericLogger log;
        private readonly IIrcMac mac;
        private readonly TimeSpan rateLimit;

        // ---------------- Constructor ----------------

        public MacWriter( GenericLogger log, IIrcMac mac, TimeSpan rateLimit ) :
            base( nameof( MacWriter ) )
        {
            this.log = log;
            this.mac = mac;
            this.rateLimit = rateLimit;
        }

        // ---------------- Functions ----------------

        public new void Start()
        {
            base.Start();
        }

        public void SendMessage( string message )
        {
            this.BeginInvoke(
                () =>
                {
                    if( this.mac.IsConnected == false )
                    {
                        return;
                    }

                    this.mac.WriteLine( message );
                    Task.Delay( this.rateLimit );
                }
            );
        }

        protected override void OnBadEnqueue( Action action )
        {
            this.log.ErrorWriteLine( "Could not enqueue action onto channel" );
        }

        protected override void OnError( Exception e )
        {
            this.PrintError( e );
        }

        protected override void OnTaskCancelled( TaskCanceledException e )
        {
            this.PrintError( e );
        }

        protected override void OnThreadExit()
        {
            this.log.WriteLine( $"Exiting {this.Name}" );
        }

        protected override void OnFatalError( Exception e )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( $"FATAL Exception in {this.Name}:" );
            errorMessage.WriteLine( e.ToString() );
            errorMessage.WriteLine( "***************" );

            this.log.ErrorWriteLine( errorMessage.ToString() );
        }

        private void PrintError( Exception err )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( $"Caught Exception in {this.Name}:" );
            errorMessage.WriteLine( err.ToString() );
            errorMessage.WriteLine( "***************" );

            this.log.ErrorWriteLine( errorMessage.ToString() );
        }
    }
}
