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
using System.Text;
using System.Text.RegularExpressions;
using Chaskis.Core;
using SethCS.Basic;

namespace Chaskis.Plugins.CapsWatcher
{
    /// <summary>
    /// Caps watcher yells at uses who use all caps.
    /// </summary>
    [ChaskisPlugin( "capswatcher" )]
    public class CapsWatcher : IPlugin
    {
        // -------- Fields --------

        internal const string VersionStr = "0.4.1";

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

        //
        // ([^\w\s]*) - Special Symbols are allowed to be captured.
        // (:-?[\S]) - Emoticons such as :D :P :p :-p can be captured.
        // (\s[\S]-?:) - Emoticons such as D: can be captured.
        // (\s*) - Capture whitespace.
        // [A-Z]{2,} - Capture all caps... must have a minimum of two caps in a row somewhere.  Numbers can come before and after if needed.
        // ([A-Z0-9\s]*) - Capture caps/numbers before and after the two capital letters.
        private static readonly Regex capsRegex = new Regex(
            @"^(((([^\w\s]?)|(:-?[\S])*|(\s[\S]-?:)*|([A-Z0-9\s]*)))*[A-Z]{2,}((([^\w\s]*)|(:-?[\S])*|(\s[\S]-?:)*|([A-Z0-9\s]*)))*|(\s*))+$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        private Regex ignoreRegex;

        private GenericLogger log;

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
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            string configPath = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "CapsWatcher",
                "CapsWatcherConfig.xml"
            );

            this.log = initor.Log;

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.config = XmlLoader.LoadCapsWatcherConfig( configPath );
            this.ignoreRegex = new Regex( CollectionToRegex( config.Ignores ), RegexOptions.Compiled | RegexOptions.ExplicitCapture );
            this.config.Ignores.Clear(); // No need to eat-up RAM, we won't need this again.

            MessageHandlerConfig msgConfig = new MessageHandlerConfig
            {
                LineRegex = ".+",
                LineAction = this.HandleMessage
            };

            MessageHandler handler = new MessageHandler(
                msgConfig
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles what happens if a user requests help for the plugin
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                "Type in all caps to see what happens. Go ahead, I DARE YOU! Messages must contain at least 3 characters and 1 letter for a response.",
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
            // Nothing to Dispose
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
        public static bool CheckForCaps( string message, Regex ignoreList )
        {
            if( message == null )
            {
                return false;
            }

            // Filter out any ignores.
            message = ignoreList.Replace( message, string.Empty );

            // First check, make sure we're not empty.
            if( string.IsNullOrEmpty( message ) || string.IsNullOrWhiteSpace( message ) )
            {
                return false;
            }

            return capsRegex.IsMatch( message );
        }

        public static string CollectionToRegex( IList<string> ignoreList )
        {
            if( ignoreList.Count == 0 )
            {
                return string.Empty;
            }

            // Need to order the list so the longest strings come first.
            // What if we have a regex like this?
            // (\s*US\s*)|(\s*USA\s*)
            // and an input of USA USA?
            // Our replace result will be 'A', thus triggering the bot.
            // If we switch USA and US, it is not a problem.
            IOrderedEnumerable<string> sortedList = ignoreList.OrderByDescending( s => s.Length );

            StringBuilder builder = new StringBuilder();
            builder.Append( @"(\s*" );
            foreach( string ignore in sortedList )
            {
                // Only ignore if the thing is surrounded by whitespace, not other characters (
                // don't want to ignore ignores within other words).
                builder.Append( ignore + @"\s*)|(\s*" );
            }

            builder.Remove( builder.Length - 5, 5 );
            return builder.ToString();
        }

        /// <summary>
        /// Handles a message from the channel.
        /// </summary>
        private void HandleMessage( MessageHandlerArgs args )
        {
            if( CheckForCaps( args.Message, this.ignoreRegex ) )
            {
                string msgToSend = Parsing.LiquefyStringWithIrcConfig( SelectMessage(), args.User );
                args.Writer.SendMessage( msgToSend, args.Channel );
            }
            else
            {
                this.log.WriteLine(
                    Convert.ToInt32( LogVerbosityLevel.HighVerbosity ),
                    "Caps check failed for message '" + args.Match + "'"
                );
            }
        }
    }
}