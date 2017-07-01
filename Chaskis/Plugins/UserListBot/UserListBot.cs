//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using ChaskisCore;

namespace Chaskis.Plugins.UserListBot
{
    /// <summary>
    /// Allows users to query for the users in a channel.
    /// </summary>
    [ChaskisPlugin( "userlistbot" )]
    public class UserListBot : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "1.0.0";

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
        private UserList userList;

        /// <summary>
        /// Whether or not a user queried to for the userlist.
        /// When this is false, we will not send anything
        /// Key is the channel the query came from,
        /// Value whether or not that channel queried or not.
        /// </summary>
        private Dictionary<string, bool> isQueried;

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
                MessageHandler userQueryHandler = new MessageHandler(
                    this.userListConfig.Command,
                    this.HandleGetUsersCommand,
                    this.userListConfig.Cooldown
                );

                this.handlers.Add( userQueryHandler );
            }
            {
                AllHandler nameResponseHandler = new AllHandler( HandleNamesResponse );
                this.handlers.Add( nameResponseHandler );
            }

            {
                AllHandler endOfNamesHandler = new AllHandler( HandleEndOfNamesResponse );
                this.handlers.Add( endOfNamesHandler );
            }
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            string message = string.Format(
                "Usage: '{0}'.  Note I have a cooldown time of {1} seconds.",
                this.userListConfig.Command,
                this.userListConfig.Cooldown.ToString()
            );

            writer.SendMessage(
                message,
                response.Channel
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
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleGetUsersCommand( IIrcWriter writer, IrcResponse response )
        {
            writer.SendRawCmd( "NAMES " + response.Channel );
            this.isQueried[response.Channel] = true;
        }

        /// <summary>
        /// Handles the names response from the server.
        /// Adds the names to the list.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleNamesResponse( IIrcWriter writer, IrcResponse response )
        {
            this.userList.ParseNameResponse( response.Message );
        }

        /// <summary>
        /// Handles the end-of-names response from the server.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleEndOfNamesResponse( IIrcWriter writer, IrcResponse response )
        {
            Tuple<string, string> userList = this.userList.CheckAndHandleEndMessage( response.Message );
            if( userList != null )
            {
                if( this.isQueried.ContainsKey( userList.Item1 ) && this.isQueried[userList.Item1] )
                {
                    writer.SendMessage(
                        string.Format( "Users in {0}: {1}", userList.Item1, userList.Item2 ),
                        userList.Item1
                    );
                }
            }
        }
    }
}