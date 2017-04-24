//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChaskisCore;
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

        public const string VersionStr = "0.1.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private IIrcConfig ircConfig;

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
        /// <param name="pluginPath">The absolute path to the plugin dll.</param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "QuoteBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.ircConfig = ircConfig;

            this.quoteBotConfig = XmlLoader.LoadConfig( configPath );
            this.parser = new QuoteBotParser( this.quoteBotConfig );
            this.db = new QuoteBotDatabase( Path.Combine( Path.GetDirectoryName( pluginPath ), "quotes.db" ) );

            MessageHandler addHandler = new MessageHandler(
                this.quoteBotConfig.AddCommand,
                this.AddHandler
            );
            this.handlers.Add( addHandler );

            MessageHandler deleteHandler = new MessageHandler(
                this.quoteBotConfig.DeleteCommand,
                this.DeleteHandler
            );
            this.handlers.Add( deleteHandler );

            MessageHandler randomHandler = new MessageHandler(
                this.quoteBotConfig.RandomCommand,
                this.RandomHandler
            );
            this.handlers.Add( randomHandler );

            MessageHandler getHandler = new MessageHandler(
                this.quoteBotConfig.GetCommand,
                this.GetHandler
            );
            this.handlers.Add( getHandler );
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            StringBuilder builder = new StringBuilder();

            builder.Append( "@" + response.RemoteUser + ": " );

            if( args.Length == 0 )
            {
                builder.Append( "Append 'add', 'delete', 'random', or 'get' to the help message you just sent to get more information about each command." );
            }
            else if( args[0] == "add" )
            {
                builder.Append( "Adds a quote to the database. Command regex: " + this.quoteBotConfig.AddCommand );
            }
            else if( args[0] == "delete" )
            {
                builder.Append( "Deletes a quote from the database. Must be a bot admin. Command regex: " + this.quoteBotConfig.DeleteCommand );
            }
            else if( args[0] == "random" )
            {
                builder.Append( "Posts a random quote from the database. Command regex: " + this.quoteBotConfig.RandomCommand );
            }
            else if( args[0] == "get" )
            {
                builder.Append( "Posts the given quote from the database. Command regex: " + this.quoteBotConfig.GetCommand );
            }

            writer.SendMessageToUser(
                builder.ToString(),
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
            this.db?.Dispose();
        }

        // -------- Handlers --------

        /// <summary>
        /// Handles the response when the user wants to add a quote.
        /// </summary>
        private async void AddHandler( IIrcWriter writer, IrcResponse response )
        {
            Quote quote;
            string error;
            if( this.parser.TryParseAddCommand( response.Message, out quote, out error ) )
            {
                try
                {
                    long id = await this.db.AddQuoteAsync( quote );
                    writer.SendMessageToUser(
                        string.Format( "Quote by {0} added.  Its ID is {1}", quote.Author, quote.Id.Value ),
                        response.Channel
                    );
                }
                catch( Exception err )
                {
                    writer.SendMessageToUser(
                        "Error when adding quote: " + err.Message.NormalizeWhiteSpace(),
                        response.Channel
                    );
                }
            }
            else
            {
                writer.SendMessageToUser(
                    "Error when adding quote: " + error.NormalizeWhiteSpace(),
                    response.Channel
                );
            }
        }

        /// <summary>
        /// Handles the response when the user wants to delete a quote.
        /// </summary>
        private async void DeleteHandler( IIrcWriter writer, IrcResponse response )
        {
            if( this.ircConfig.Admins.Contains( response.RemoteUser ) == false )
            {
                writer.SendMessageToUser(
                    "@" + response.RemoteUser + ": you are not a bot admin.  You can not delete quotes.",
                    response.Channel
                );
                return;
            }

            long id;
            string error;
            if( this.parser.TryParseDeleteCommand( response.Message, out id, out error ) )
            {
                try
                {
                    bool success = await this.db.DeleteQuoteAsync( id );
                    if( success )
                    {
                        writer.SendMessageToUser(
                            "Quote " + id + " deleted successfully.",
                            response.Channel
                        );
                    }
                    else
                    {
                        writer.SendMessageToUser(
                            "Can not delete quote " + id + ".  Are you sure it existed?",
                            response.Channel
                        );
                    }
                }
                catch( Exception err )
                {
                    writer.SendMessageToUser(
                        "Error when deleting quote: " + err.Message.NormalizeWhiteSpace(),
                        response.Channel
                    );
                }
            }
            else
            {
                writer.SendMessageToUser(
                    "Error when deleteing quote.  Are you sure it exists? " + error.NormalizeWhiteSpace(),
                    response.Channel
                );
            }
        }

        /// <summary>
        /// Handles the response when the user wants to get a random quote.
        /// </summary>
        private async void RandomHandler( IIrcWriter writer, IrcResponse response )
        {
            string error;
            if( this.parser.TryParseRandomCommand( response.Message, out error ) )
            {
                try
                {
                    Quote quote = await this.db.GetRandomQuoteAsync();
                    if( quote != null )
                    {
                        writer.SendMessageToUser(
                            string.Format( "Quote {0} by {1}: '{2}'", quote.Id.Value, quote.Author, quote.QuoteText ),
                            response.Channel
                        );
                    }
                    else
                    {
                        writer.SendMessageToUser(
                            "Can not get random quote.  Do we even have any?",
                            response.Channel
                        );
                    }
                }
                catch( Exception err )
                {
                    writer.SendMessageToUser(
                        "Error getting random quote: " + err.Message.NormalizeWhiteSpace(),
                        response.Channel
                    );
                }
            }
            else
            {
                writer.SendMessageToUser(
                    "Error getting random quote: " + error.NormalizeWhiteSpace(),
                    response.Channel
                );
            }
        }

        /// <summary>
        /// Handles the response when the user wants to get a quote.
        /// </summary>
        private async void GetHandler( IIrcWriter writer, IrcResponse response )
        {
            string error;
            long id;
            if( this.parser.TryParseGetCommand( response.Message, out id, out error ) )
            {
                try
                {
                    Quote quote = await this.db.GetQuoteAsync( id );
                    if( quote != null )
                    {
                        writer.SendMessageToUser(
                            string.Format( "Quote {0} by {1}: '{2}'", quote.Id.Value, quote.Author, quote.QuoteText ),
                            response.Channel
                        );
                    }
                    else
                    {
                        writer.SendMessageToUser(
                            "Can not get quote with id " + id + ". Are you sure it exists?",
                            response.Channel
                        );
                    }
                }
                // Error that happens if there are no quotes.
                catch( InvalidOperationException err )
                {
                    writer.SendMessageToUser(
                        "Error getting quote: " + err.Message.NormalizeWhiteSpace() + ". Do any quotes exist?",
                        response.Channel
                    );
                }
                catch( Exception err )
                {
                    writer.SendMessageToUser(
                        "Error getting quote: " + err.Message.NormalizeWhiteSpace(),
                        response.Channel
                    );
                }
            }
            else
            {
                writer.SendMessageToUser(
                    "Error getting quote: " + error.NormalizeWhiteSpace(),
                    response.Channel
                );
            }
        }
    }
}
