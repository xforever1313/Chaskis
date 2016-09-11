
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericIrcBot;

namespace Chaskis.Plugins.UserListBot
{
    /// <summary>
    /// Allows users to query for the users in a channel.
    /// </summary>
    public class UserListBot : IPlugin
    {
        // -------- Fields --------

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The irc configuration.
        /// </summary>
        private IIrcConfig ircConfig;

        /// <summary>
        /// The config for this plugin.
        /// </summary>
        private UserListBotConfig userListConfig;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserListBot()
        {
            this.handlers = new List<IIrcHandler>();
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

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.  Allowed to throw Exceptions.
        /// 
        /// This function should be used to validates that the environment is good for the plugin.
        /// For example, it has all dependencies installed, config files are in the correct spot, etc.
        /// It should also load GetHandlers() with the handlers.
        /// </summary>
        /// <param name="pluginPath">
        /// The absolute path to the plugin, including the file name.  To just get
        /// the path to the plugin, call Path.GetDirectoryName on this argument.
        /// </param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "UserListBotConfig.xml"
            );

            if ( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.ircConfig = ircConfig;
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
        public void Teardown()
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
            writer.SendRawCmd( "NAMES " + this.ircConfig.Channel );
        }
    }
}
