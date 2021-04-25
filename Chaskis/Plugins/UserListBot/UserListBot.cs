//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using Chaskis.Core;

namespace Chaskis.Plugins.UserListBot
{
    /// <summary>
    /// Allows users to query for the users in a channel.
    /// </summary>
    [ChaskisPlugin( "userlistbot" )]
    public class UserListBot : IPlugin
    {
        // -------- Fields --------

        internal const string VersionStr = "0.3.3";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The config for this plugin.
        /// </summary>
        private UserListBotConfig userListConfig;

        /// <summary>
        /// The user list.
        /// </summary>
        private readonly UserList userList;

        /// <summary>
        /// Whether or not a user queried to for the userlist.
        /// When this is false, we will not send anything
        /// Key is the channel the query came from,
        /// Value whether or not that channel queried or not.
        /// </summary>
        private readonly Dictionary<string, bool> isQueried;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserListBot()
        {
            this.handlers = new List<IIrcHandler>();
            this.userList = new UserList();
            this.isQueried = new Dictionary<string, bool>();
        }

        // -------- Properties --------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/UserListBot";
            }
        }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        /// <summary>
        /// About this plugin.
        /// </summary>
        public string About
        {
            get
            {
                return "I print a list of users currently in the IRC channel.";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.  Allowed to throw Exceptions.
        ///
        /// This function should be used to validates that the environment is good for the plugin.
        /// For example, it has all dependencies installed, config files are in the correct spot, etc.
        /// It should also load GetHandlers() with the handlers.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            string configPath = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "UserListBot",
                "UserListBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.userListConfig = XmlLoader.LoadConfig( configPath );

            // User query command:
            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.userListConfig.Command,
                    LineAction = this.HandleGetUsersCommand,
                    CoolDown = this.userListConfig.Cooldown
                };

                MessageHandler userQueryHandler = new MessageHandler(
                    msgConfig
                );

                this.handlers.Add( userQueryHandler );
            }
            {
                ReceiveHandlerConfig allHandlerConfig = new ReceiveHandlerConfig
                {
                    LineAction = this.HandleNamesResponse
                };
                ReceiveHandler nameResponseHandler = new ReceiveHandler( allHandlerConfig );
                this.handlers.Add( nameResponseHandler );
            }

            {
                ReceiveHandlerConfig allHandlerConfig = new ReceiveHandlerConfig
                {
                    LineAction = this.HandleEndOfNamesResponse
                };
                ReceiveHandler endOfNamesHandler = new ReceiveHandler( allHandlerConfig );
                this.handlers.Add( endOfNamesHandler );
            }
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            string message = string.Format(
                "Usage: '{0}'.  Note I have a cooldown time of {1} seconds.",
                this.userListConfig.Command,
                this.userListConfig.Cooldown.ToString()
            );

            msgArgs.Writer.SendMessage(
                message,
                msgArgs.Channel
            );
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        /// <summary>
        /// Tears down the plugin.
        /// </summary>
        public void Dispose()
        {
            // Nothing to teardown.
        }

        /// <summary>
        /// Handles the get users command, which queries the server for the user list.
        /// </summary>
        private void HandleGetUsersCommand( MessageHandlerArgs args )
        {
            args.Writer.SendRawCmd( "NAMES " + args.Channel );
            this.isQueried[args.Channel] = true;
        }

        /// <summary>
        /// Handles the names response from the server.
        /// Adds the names to the list.
        /// </summary>
        private void HandleNamesResponse( ReceiveHandlerArgs args )
        {
            this.userList.ParseNameResponse( args.Line );
        }

        /// <summary>
        /// Handles the end-of-names response from the server.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleEndOfNamesResponse( ReceiveHandlerArgs args )
        {
            Tuple<string, string> foundUsers = this.userList.CheckAndHandleEndMessage( args.Line );
            if( foundUsers != null )
            {
                if( this.isQueried.ContainsKey( foundUsers.Item1 ) && this.isQueried[foundUsers.Item1] )
                {
                    args.Writer.SendMessage(
                        string.Format( "Users in {0}: {1}", foundUsers.Item1, foundUsers.Item2 ),
                        foundUsers.Item1
                    );
                }
            }
        }
    }
}