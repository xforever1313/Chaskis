//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Specialized;
using System.Linq;
using Chaskis.Core;
using SethCS.Extensions;

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

        public HttpResponseInfo HandleResposne( string url, string method, NameValueCollection queryString )
        {
            ContentType contentType = ContentType.Xml;

            try
            {
                if( queryString.AllKeys.Contains( "format" ) )
                {
                    if( Enum.TryParse( queryString["format"], out ContentType parsedType ) == false )
                    {
                        return new HttpResponseInfo
                        {
                            ContentType = contentType,
                            Error = ErrorMessage.InvalidFormat,
                            Message = "Invalid format: " + queryString["format"],
                            ResponseStatus = HttpResponseStatus.ClientError
                        };
                    }
                    else
                    {
                        contentType = parsedType;
                    }
                }

                if( method.EqualsIgnoreCase( "POST" ) == false )
                {
                    return new HttpResponseInfo
                    {
                        ContentType = contentType,
                        Error = ErrorMessage.InvalidMethod,
                        Message = "Request must be a POST request, nothing else is currently supported.  Got: " + method,
                        ResponseStatus = HttpResponseStatus.ClientError
                    };
                }

                if( this.IsIrcConnected == false )
                {
                    // If after all of this, we are not connected, tell the client
                    // that the bot is not connected.
                    return new HttpResponseInfo
                    {
                        ContentType = contentType,
                        Error = ErrorMessage.NotConnectedToIrc,
                        Message = "IRC Bot not connected, can not send command.",
                        ResponseStatus = HttpResponseStatus.ServerError
                    };
                }

                HttpResponseInfo info = DoRequestAction( url, queryString, contentType );

                return info;
            }
            catch( Exception err )
            {
                return new HttpResponseInfo
                {
                    ContentType = contentType,
                    Error = ErrorMessage.Unknown,
                    Message = err.Message,
                    ResponseStatus = HttpResponseStatus.ServerError
                };
            }
        }

        private HttpResponseInfo DoRequestAction( string url, NameValueCollection queryString, ContentType format )
        {
            if ( url.EqualsIgnoreCase( "/privmsg" ) )
            {
                return this.HandlePrivmsgAction( queryString, format );
            }
            else if ( url.EqualsIgnoreCase( "/kick" ) )
            {
                return this.HandleKickAction( queryString, format );
            }
            else if ( url.EqualsIgnoreCase( "/broadcast" ) || url.EqualsIgnoreCase( "/bcast" ) )
            {
                return this.HandleBcastAction( queryString, format );
            }
            else if ( url.EqualsIgnoreCase( "/part" ) )
            {
                return this.HandlePartAction( queryString, format );
            }
            else if ( url.EqualsIgnoreCase( "/action" ) )
            {
                return this.HandlerActionAction( queryString, format );
            }
            else
            {
                return new HttpResponseInfo
                {
                    ContentType = format,
                    Error = ErrorMessage.InvalidUrl,
                    Message = "Invalid Action: " + url,
                    ResponseStatus = HttpResponseStatus.ClientError
                };
            }
        }

        private HttpResponseInfo HandlerActionAction( NameValueCollection queryString, ContentType format )
        {
            string channel = queryString["channel"];
            string message = queryString["message"];

            if ( string.IsNullOrWhiteSpace( channel ) || string.IsNullOrWhiteSpace( message ) )
            {
                return new HttpResponseInfo
                {
                    ContentType = format,
                    Error = ErrorMessage.PrivMsgMissingParameters,
                    Message = "PRIVMSG is missing either 'channel' or 'message' in the query string.",
                    ResponseStatus = HttpResponseStatus.ClientError
                };
            }

            this.writer.SendAction( message, channel );

            return new HttpResponseInfo
            {
                ContentType = format,
                Error = ErrorMessage.None,
                Message = string.Format( "Action '{0}' sent to '{1}'", message, channel ),
                ResponseStatus = HttpResponseStatus.Ok
            };
        }

        private HttpResponseInfo HandlePrivmsgAction( NameValueCollection queryString, ContentType format )
        {
            string channel = queryString["channel"];
            string message = queryString["message"];

            if( string.IsNullOrWhiteSpace( channel ) || string.IsNullOrWhiteSpace( message ) )
            {
                return new HttpResponseInfo
                {
                    ContentType = format,
                    Error = ErrorMessage.PrivMsgMissingParameters,
                    Message = "PRIVMSG is missing either 'channel' or 'message' in the query string.",
                    ResponseStatus = HttpResponseStatus.ClientError
                };
            }

            this.writer.SendMessage( message, channel );

            return new HttpResponseInfo
            {
                ContentType = format,
                Error = ErrorMessage.None,
                Message = string.Format( "Message '{0}' sent to '{1}'", message, channel ),
                ResponseStatus = HttpResponseStatus.Ok
            };
        }

        private HttpResponseInfo HandleKickAction( NameValueCollection queryString, ContentType format )
        {
            string user = queryString["user"];
            string channel = queryString["channel"];
            string reason = queryString["message"]; // Reason is optional.

            if( string.IsNullOrWhiteSpace( user ) || string.IsNullOrWhiteSpace( channel ) )
            {
                return new HttpResponseInfo
                {
                    ContentType = format,
                    Error = ErrorMessage.KickMsgMissingParameters,
                    Message = "KICK is missing 'user' or 'channel' in the query string.",
                    ResponseStatus = HttpResponseStatus.ClientError
                };
            }

            this.writer.SendKick( user, channel, reason );

            return new HttpResponseInfo
            {
                ContentType = format,
                Error = ErrorMessage.None,
                Message = string.Format( "'{0}' has been kicked from '{1}' for reason '{2}'", user, channel, reason ?? "None" ),
                ResponseStatus = HttpResponseStatus.Ok
            };
        }

        private HttpResponseInfo HandleBcastAction( NameValueCollection queryString, ContentType format )
        {
            string message = queryString["message"];

            if( string.IsNullOrWhiteSpace( message ) )
            {
                return new HttpResponseInfo
                {
                    ContentType = format,
                    Error = ErrorMessage.BcastMissingParameters,
                    Message = "BCAST is missing 'message' in the query string.",
                    ResponseStatus = HttpResponseStatus.ClientError
                };
            }

            this.writer.SendBroadcastMessage( message );

            return new HttpResponseInfo
            {
                ContentType = format,
                Error = ErrorMessage.None,
                Message = string.Format( "Message '{0}' sent to all channels.", message ),
                ResponseStatus = HttpResponseStatus.Ok
            };
        }

        private HttpResponseInfo HandlePartAction( NameValueCollection queryString, ContentType format )
        {
            string channel = queryString["channel"];
            string reason = queryString["message"]; // Reason is optional.

            if( string.IsNullOrWhiteSpace( channel ) )
            {
                return new HttpResponseInfo
                {
                    ContentType = format,
                    Error = ErrorMessage.PartMissingParameters,
                    Message = "PART is missing 'channel' in the query string.",
                    ResponseStatus = HttpResponseStatus.ClientError
                };
            }

            this.writer.SendPart( reason, channel );

            return new HttpResponseInfo
            {
                ContentType = format,
                Error = ErrorMessage.None,
                Message = string.Format( "Parted '{0}' for reason '{1}'", channel, reason ?? "None" ),
                ResponseStatus = HttpResponseStatus.Ok
            };
        }
    }
}
