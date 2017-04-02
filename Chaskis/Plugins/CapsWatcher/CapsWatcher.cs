//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChaskisCore;

namespace Chaskis.Plugins.CapsWatcher
{
    /// <summary>
    /// Caps watcher yells at uses who use all caps.
    /// </summary>
    [ChaskisPlugin( "capswatcher" )]
    public class CapsWatcher : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "0.1.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// Config to this class.
        /// </summary>
        private CapsWatcherConfig config;

        /// <summary>
        /// RNG.
        /// </summary>
        private readonly Random random;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public CapsWatcher()
        {
            this.handlers = new List<IIrcHandler>();
            this.random = new Random();
        }

        // -------- Properties --------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/CapsWatcher";
            }
        }

        /// <summary>
        /// This plugin's current Version in the form of a string.
        /// </summary>
        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        /// <summary>
        /// What this plugin does.
        /// </summary>
        public string About
        {
            get
            {
                return "This plugin will yell at users who POST IN ALL CAPS!";
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
                "CapsWatcherConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.config = XmlLoader.LoadCapsWatcherConfig( configPath );

            MessageHandler handler = new MessageHandler(
                ".+",
                this.HandleMessage
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles what happens if a user requests help for the plugin/
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessageToUser(
                "Type in all caps to see what happens. Go ahead, I DARE YOU! Messages must contain at least 3 characters and 1 letter for a response.",
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
        public void Teardown()
        {
        }

        /// <summary>
        /// Selects a random message from the config to send
        /// to the user.
        /// </summary>
        /// <returns>The message to send to the user.</returns>
        public string SelectMessage()
        {
            int index = random.Next( 0, this.config.Messages.Count );
            return this.config.Messages[index];
        }

        /// <summary>
        /// Checks to see if the given message is all caps or not.
        /// Message must be at least 3 characters, must contain
        /// one letter, and all letters must be in all caps.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if the message is in all caps, else false.</returns>
        public static bool CheckForCaps( string message )
        {
            // First check, make sure the message is at least 3 characters.
            if( string.IsNullOrEmpty( message ) || string.IsNullOrWhiteSpace( message ) )
            {
                return false;
            }
            else if( message.Length < 3 )
            {
                return false;
            }

            // Find letter.  Return if can't find it.
            else if( message.FirstOrDefault( ch => char.IsLetter( ch ) ) == '\0' )
            {
                return false;
            }

            // If our message matches what it would be upper case, then
            // we are good to go.
            return message.Equals( message.ToUpper() );
        }

        /// <summary>
        /// Handles a message from the channel.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleMessage( IIrcWriter writer, IrcResponse response )
        {
            if( CheckForCaps( response.Message ) )
            {
                string msgToSend = SelectMessage().Replace( "{%user%}", response.RemoteUser );
                writer.SendCommandToChannel( msgToSend );
            }
        }
    }
}