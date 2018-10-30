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

    public enum ErrorMessage
    {
        /// <summary>
        /// Unknown error.
        /// </summary>
        Unknown,

        /// <summary>
        /// No errors to report.
        /// </summary>
        None,

        /// <summary>
        /// Invalid format string specified.
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// Invalid method specified.
        /// </summary>
        InvalidMethod,

        /// <summary>
        /// Invalid URL specified.
        /// </summary>
        InvalidUrl,

        /// <summary>
        /// Not connected to IRC.
        /// </summary>
        NotConnectedToIrc,

        /// <summary>
        /// Private Message is missing parameters.
        /// </summary>
        PrivMsgMissingParameters,

        /// <summary>
        /// Kick message is missing parameters
        /// </summary>
        KickMsgMissingParameters,

        /// <summary>
        /// BCast message is missing parameters.
        /// </summary>
        BcastMissingParameters,

        /// <summary>
        /// Part message is missing parameters.
        /// </summary>
        PartMissingParameters
    }

    public enum ContentType
    {
        /// <summary>
        /// We only return XML at the moment...
        /// </summary>
        Xml
    }
}
