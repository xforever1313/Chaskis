//
//          Copyright Seth Hendrick 2018-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using Chaskis.Core;
using SethCS.Basic;

namespace Chaskis.Plugins.HttpServer
{
    [ChaskisPlugin( PluginName )]
    public class HttpServerPlugin : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.2.0";

        internal const string PluginName = "httpserver";

        private HttpServer server;
        private HttpResponseHandler httpResponseHandler;
        private HttpServerConfig config;
        private GenericLogger log;

        private readonly List<IIrcHandler> handlers;

        // ---------------- Constructor ----------------

        public HttpServerPlugin()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // ---------------- Properties ----------------

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/HttpServer";
            }
        }

        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        public string About
        {
            get
            {
                return "I allow admins to control me remotely via HTTP.";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor pluginInit )
        {
            string configPath = Path.Combine(
                pluginInit.ChaskisConfigPluginRoot,
                "HttpServer",
                "HttpServerConfig.xml"
            );

            this.config = XmlLoader.LoadConfig( configPath );
            this.log = pluginInit.Log;

            DisconnectingEventConfig disconnectingConfig = new DisconnectingEventConfig
            {
                LineAction = this.OnDisconnecting
            };
            this.handlers.Add(
                new DisconnectingEventHandler( disconnectingConfig )
            );

            ConnectedEventConfig connectedEventConfig = new ConnectedEventConfig
            {
                LineAction = this.OnConnect
            };
            this.handlers.Add(
                new ConnectedEventHandler( connectedEventConfig )
            );
        }

        public void Dispose()
        {
            if( this.server != null )
            {
                this.server.OnStatus -= this.Status;
                this.server.OnError -= this.ErrorStatus;
                this.server.Dispose();
            }
        }

        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                "I have no commands.",
                msgArgs.Channel
            );
        }

        private void OnConnect( ConnectedEventArgs args )
        {
            this.httpResponseHandler = new HttpResponseHandler( args.Writer )
            {
                IsIrcConnected = true
            };
            this.server = new HttpServer( config, this.httpResponseHandler );
            this.server.OnStatus += this.Status;
            this.server.OnError += this.ErrorStatus;
            this.server.Start();
            this.Status( "HTTP Server Started" );
        }

        private void OnDisconnecting( DisconnectingEventArgs args )
        {
            if( this.httpResponseHandler != null )
            {
                this.httpResponseHandler.IsIrcConnected = false;
            }
        }

        private void Status( string str )
        {
            this.log.WriteLine( str );
        }

        private void ErrorStatus( string str )
        {
            this.log.ErrorWriteLine( str );
        }
    }
}
