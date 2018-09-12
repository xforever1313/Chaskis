//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Plugins.HttpServer
{
    public class HttpServerConfig
    {
        // ---------------- Constructor ----------------

        public HttpServerConfig()
        {
            this.Port = 80;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The port to listen to HTTP Server requests on.
        /// Defaulted to 80.
        /// </summary>
        public ushort Port { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            // Nothing to validate... yet.
        }
    }
}
