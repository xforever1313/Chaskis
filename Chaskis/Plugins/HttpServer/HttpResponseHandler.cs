//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Specialized;
using Chaskis.Core;

namespace Chaskis.Plugins.HttpServer
{
    /// <summary>
    /// Determines what request was sent from the client, and then
    /// figures out what to do with it.
    /// </summary>
    public class HttpResponseHandler
    {
        // ---------------- Fields ----------------

        private bool isConnected;
        private readonly object isConnectedLock;

        private readonly IIrcWriter writer;

        // ---------------- Constructor ----------------

        public HttpResponseHandler( IIrcWriter writer )
        {
            this.isConnected = false;
            this.isConnectedLock = new object();

            this.writer = writer;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Is the IRC bot connected to the server?
        /// Set to false if the connection goes down.
        /// </summary>
        public bool IsIrcConnected
        {
            get
            {
                lock( this.isConnectedLock )
                {
                    return this.isConnected;
                }
            }
            set
            {
                lock( this.isConnectedLock )
                {
                    this.isConnected = value;
                }
            }
        }

        // ---------------- Functions ----------------

        public void HandleResposne( string url, string method, NameValueCollection queryString )
        {
        }
    }
}
