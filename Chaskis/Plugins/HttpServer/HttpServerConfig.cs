//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;

namespace Chaskis.Plugins.HttpServer
{
    public class HttpServerConfig : IEquatable<HttpServerConfig>
    {
        // ---------------- Constructor ----------------

        public HttpServerConfig()
        {
            this.Port = 10080;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The port to listen to HTTP Server requests on.
        /// Defaulted to 80.
        /// </summary>
        public ushort Port { get; set; }

        // ---------------- Functions ----------------

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine( "Http Server Config:" );
            builder.AppendLine( "\t-Port: " + this.Port );

            return builder.ToString();
        }

        public override bool Equals( object obj )
        {
            HttpServerConfig other = obj as HttpServerConfig;
            return this.Equals( other );
        }

        public bool Equals( HttpServerConfig other )
        {
            if( other == null )
            {
                return false;
            }

            return
                ( this.Port == other.Port );
        }

        public override int GetHashCode()
        {
            return
                this.Port.GetHashCode();
        }

        public void Validate()
        {
            // Nothing to validate... yet.
        }
    }
}
