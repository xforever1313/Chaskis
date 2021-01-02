//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chaskis.Core;
using SethCS.Extensions;

namespace Chaskis.Plugins.QuoteBot
{
    /// <summary>
    /// This plugin saves quotes from IRC to a sqlite database on the server.
    /// Also allows users to view quotes.
    /// </summary>
    [ChaskisPlugin( "quotebot" )]
    public class QuoteBot : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.5.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private IReadOnlyIrcConfig ircConfig;

        private QuoteBotConfig quoteBotConfig;

        private QuoteBotParser parser;

        private QuoteBotDatabase db;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public QuoteBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/QuoteBot";
            }
        }

        /// <summary>
        /// This plugin's version.
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
                return "I save memorable quotes from the IRC channel for you!";
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            string quoteBotRoot = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "QuoteBot"
            );

            string configPath = Path.Combine(
                quoteBotRoot,
                "QuoteBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.ircConfig = initor.IrcConfig;

            this.quoteBotConfig = XmlLoader.LoadConfig( configPath );
            this.parser = new QuoteBotParser( this.quoteBotConfig );
            this.db = new QuoteBotDatabase( Path.Combine( quoteBotRoot, "quotes.ldb" ) );

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.quoteBotConfig.AddCommand,
                    LineAction = this.AddHandler
                };

                MessageHandler addHandler = new MessageHandler(
                    msgConfig
                );
                this.handlers.Add( addHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.quoteBotConfig.DeleteCommand,
                    LineAction = this.DeleteHandler
                };

                MessageHandler deleteHandler = new MessageHandler(
                    msgConfig
                );
                this.handlers.Add( deleteHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.quoteBotConfig.RandomCommand,
                    LineAction = this.RandomHandler
                };

                MessageHandler randomHandler = new MessageHandler(
                    msgConfig
                );
                this.handlers.Add( randomHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.quoteBotConfig.GetCommand,
                    LineAction = this.GetHandler
                };

                MessageHandler getHandler = new MessageHandler(
                    msgConfig
                );
                this.handlers.Add( getHandler );
            }
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            StringBuilder builder = new StringBuilder();

            builder.Append( "@" + msgArgs.User + ": " );

            if( helpArgs.Length == 0 )
            {
                builder.Append( "Append 'add', 'delete', 'random', or 'get' to the help message you just sent to get more information about each command." );
            }
            else if( helpArgs[0] == "add" )
            {
                builder.Append( "Adds a quote to the database. Command regex: " + this.quoteBotConfig.AddCommand );
            }
            else if( helpArgs[0] == "delete" )
            {
                builder.Append( "Deletes a quote from the database. Must be a bot admin. Command regex: " + this.quoteBotConfig.DeleteCommand );
            }
            else if( helpArgs[0] == "random" )
            {
                builder.Append( "Posts a random quote from the database. Command regex: " + this.quoteBotConfig.RandomCommand );
            }
            else if( helpArgs[0] == "get" )
            {
                builder.Append( "Posts the given quote from the database. Command regex: " + this.quoteBotConfig.GetCommand );
            }

            msgArgs.Writer.SendMessage(
                builder.ToString(),
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
            this.db?.Dispose();
        }

        // -------- Handlers --------

        /// <summary>
        /// Handles the response when the user wants to add a quote.
        /// </summary>
        private async void AddHandler( MessageHandlerArgs args )
        {
            if( this.parser.TryParseAddCommand( args.Message, args.User, out Quote quote, out string error ) )
            {
                try
                {
                    int id = await this.db.AddQuoteAsync( quote );
                    args.Writer.SendMessage(
                        string.Format( "Quote said by {0} added by {1}.  Its ID is {2}.", quote.Author, quote.Adder, id ),
                        args.Channel
                    );
                }
                catch( Exception err )
                {
                    args.Writer.SendMessage(
                        "Error when adding quote: " + err.Message.NormalizeWhiteSpace(),
                        args.Channel
                    );
                }
            }
            else
            {
                args.Writer.SendMessage(
                    "Error when adding quote: " + error.NormalizeWhiteSpace(),
                    args.Channel
                );
            }
        }

        /// <summary>
        /// Handles the response when the user wants to delete a quote.
        /// </summary>
        private async void DeleteHandler( MessageHandlerArgs args )
        {
            if( this.ircConfig.Admins.Contains( args.User ) == false )
            {
                args.Writer.SendMessage(
                    "@" + args.User + ": you are not a bot admin.  You can not delete quotes.",
                    args.Channel
                );
                return;
            }

            if( this.parser.TryParseDeleteCommand( args.Message, out int id, out string error ) )
            {
                try
                {
                    bool success = await this.db.DeleteQuoteAsync( id );
                    if( success )
                    {
                        args.Writer.SendMessage(
                            "Quote " + id + " deleted successfully.",
                            args.Channel
                        );
                    }
                    else
                    {
                        args.Writer.SendMessage(
                            "Can not delete quote " + id + ".  Are you sure it existed?",
                            args.Channel
                        );
                    }
                }
                catch( Exception err )
                {
                    args.Writer.SendMessage(
                        "Error when deleting quote: " + err.Message.NormalizeWhiteSpace(),
                        args.Channel
                    );
                }
            }
            else
            {
                args.Writer.SendMessage(
                    "Error when deleteing quote.  Are you sure it exists? " + error.NormalizeWhiteSpace(),
                    args.Channel
                );
            }
        }

        /// <summary>
        /// Handles the response when the user wants to get a random quote.
        /// </summary>
        private async void RandomHandler( MessageHandlerArgs args )
        {
            if( this.parser.TryParseRandomCommand( args.Message, out string error ) )
            {
                try
                {
                    Quote quote = await this.db.GetRandomQuoteAsync();
                    if( quote != null )
                    {
                        args.Writer.SendMessage(
                            quote.ToString(),
                            args.Channel
                        );
                    }
                    else
                    {
                        args.Writer.SendMessage(
                            "Can not get random quote.  Do we even have any?",
                            args.Channel
                        );
                    }
                }
                catch( Exception err )
                {
                    args.Writer.SendMessage(
                        "Error getting random quote: " + err.Message.NormalizeWhiteSpace(),
                        args.Channel
                    );
                }
            }
            else
            {
                args.Writer.SendMessage(
                    "Error getting random quote: " + error.NormalizeWhiteSpace(),
                    args.Channel
                );
            }
        }

        /// <summary>
        /// Handles the response when the user wants to get a quote.
        /// </summary>
        private async void GetHandler( MessageHandlerArgs args )
        {
            if( this.parser.TryParseGetCommand( args.Message, out int id, out string error ) )
            {
                try
                {
                    Quote quote = await this.db.GetQuoteAsync( id );
                    if( quote != null )
                    {
                        args.Writer.SendMessage(
                            quote.ToString(),
                            args.Channel
                        );
                    }
                    else
                    {
                        args.Writer.SendMessage(
                            "Can not get quote with id " + id + ". Are you sure it exists?",
                            args.Channel
                        );
                    }
                }
                catch( Exception err )
                {
                    args.Writer.SendMessage(
                        "Error when getting quote: " + err.Message.NormalizeWhiteSpace(),
                        args.Channel
                    );
                }
            }
            else
            {
                args.Writer.SendMessage(
                    "Error when getting quote: " + error.NormalizeWhiteSpace(),
                    args.Channel
                );
            }
        }
    }
}
