//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Plugins.HttpServer
{
    public enum HttpResponseStatus
    {
        /// <summary>
        /// Everything went okay on the server side.
        /// </summary>
        Ok,

        /// <summary>
        /// The client gave something wrong.
        /// </summary>
        ClientError,

        /// <summary>
        /// The server did something wrong.
        /// </summary>
        ServerError
    }

    public enum ContentType
    {
        /// <summary>
        /// We only return XML at the moment...
        /// </summary>
        Xml
    }
}
