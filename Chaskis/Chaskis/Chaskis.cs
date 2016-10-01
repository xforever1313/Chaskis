//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChaskisCore;
using SethCS.Exceptions;

namespace Chaskis
{
    /// <summary>
    /// The main class.
    /// </summary>
    public class Chaskis : IDisposable
    {
        // -------- Fields --------

        /// <summary>
        /// The action to take when we want to report info, where the argument is the info string.
        /// </summary>
        private Action<string> infoLogFunction;

        /// <summary>
        /// The action to take on an error where the argument is the error string.
        /// </summary>
        private Action<string> errorLogFunction;

        /// <summary>
        /// The IRC Bot.
        /// </summary>
        private IrcBot ircBot;

        /// <summary>
        /// The IRC config to use.
        /// </summary>
        private IIrcConfig ircConfig;

        /// <summary>
        /// The plugins we are using.
        /// </summary>
        private IDictionary<string, IPlugin> plugins;

        /// <summary>
        /// IRC handlers we will be using.
        /// </summary>
        private List<IIrcHandler> handlers;

        /// <summary>
        /// Whether or not this class was fully initialized or not.
        /// </summary>
        private bool fullyLoaded;

        /// <summary>
        /// The response when a user asks for the plugin list.
        /// </summary>
        private string pluginListResponse;

        /// <summary>
        /// The command for getting the source of a plugin.
        /// </summary>
        private string sourceCommand;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoLogFunction">
        /// Action to take when we want to log information.
        /// Argument to action is the string to log.
        /// </param>
        /// <param name="errorLogFunction">
        /// Action to take when we want to log an error.
        /// Argument to action is the string to log.
        /// </param>
        public Chaskis( Action<string> infoLogFunction, Action<string> errorLogFunction )
        {
            ArgumentChecker.IsNotNull( errorLogFunction, nameof( errorLogFunction ) );
            ArgumentChecker.IsNotNull( infoLogFunction, nameof( infoLogFunction ) );
            this.infoLogFunction = infoLogFunction;
            this.errorLogFunction = errorLogFunction;
            this.plugins = null;
            this.handlers = new List<IIrcHandler>();
            this.fullyLoaded = false;
        }

        // -------- Functions --------

        /// <summary>
        /// Loads the IRC config from the given XML path.
        /// </summary>
        /// <param name="xmlFilePath">The path to the IRC config XML.</param>
        public void InitState1_LoadIrcConfig( string xmlFilePath )
        {
            this.ircConfig = XmlLoader.ParseIrcConfig( xmlFilePath );
            this.sourceCommand = "[!@]" + this.ircConfig.Nick + @":?\s+source\s+(?<pluginName>\w+)";
        }

        /// <summary>
        /// Loads the Plugins from the given XML path.
        /// The IRC config MUST be loaded first.
        /// </summary>
        /// <param name="xmlFilePath">The path to the plugin config XML.</param>
        /// <returns>True if load was successful, else false.</returns>
        public bool InitStage2_LoadPlugins( string xmlFilePath )
        {
            IList<AssemblyConfig> pluginList = XmlLoader.ParsePluginConfig( xmlFilePath );
            return InitStage2_LoadPlugins( pluginList );
        }

        /// <summary>
        /// Loads plugins via a plugin list.
        /// </summary>
        /// <param name="pluginList">The list of plugins to load.</param>
        /// <returns>True if load was successful, else false.</returns>
        public bool InitStage2_LoadPlugins( IList<AssemblyConfig> pluginList )
        {
            if( this.ircConfig == null )
            {
                throw new InvalidOperationException(
                    nameof( this.ircConfig ) + " is null.  Ensure " + nameof( this.InitState1_LoadIrcConfig ) + " was call prior to this function."
                );
            }

            PluginManager manager = new PluginManager();

            if( manager.LoadPlugins( pluginList, this.ircConfig, this.infoLogFunction, this.errorLogFunction ) )
            {
                this.plugins = manager.Plugins;
                foreach( IPlugin plugin in this.plugins.Values )
                {
                    this.handlers.AddRange( plugin.GetHandlers() );
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the default irc handlers into the program.
        /// Plugins must have been loaded first.
        /// </summary>
        public void InitStage3_DefaultHandlers()
        {
            if( this.plugins == null )
            {
                throw new InvalidOperationException(
                    nameof( this.plugins ) + " is null.  Ensure " + nameof( this.InitStage2_LoadPlugins ) + " was call prior to this function."
                );
            }

            // Plugin List:
            {
                this.pluginListResponse = "List of plugins I am running: ";
                foreach( string pluginName in this.plugins.Keys )
                {
                    this.pluginListResponse += pluginName.ToLower() + " ";
                }

                MessageHandler pluginListHandler = new MessageHandler(
                    "[!@]" + this.ircConfig.Nick + @":?\s+plugins",
                    HandlePluginListCommand,
                    30
                );

                this.handlers.Add( pluginListHandler );
            }

            // Plugin Source command:
            {
                MessageHandler sourceHandler = new MessageHandler(
                    this.sourceCommand,
                    this.HandleSourceCommand,
                    3
                );

                this.handlers.Add( sourceHandler );
            }

            // Must always check for pings.
            this.handlers.Add( new PingHandler() );
        }

        /// <summary>
        /// Starts the IRC connection.
        /// </summary>
        public void InitStage4_OpenConnection()
        {
            if( this.ircConfig == null )
            {
                throw new InvalidOperationException(
                    nameof( this.ircConfig ) + " is null.  Ensure " + nameof( this.InitState1_LoadIrcConfig ) + " was call prior to this function."
                );
            }
            else if( this.handlers.Count == 0 )
            {
                throw new InvalidOperationException(
                    nameof( this.handlers ) + " is empty.  Ensure " +
                    nameof( this.InitStage3_DefaultHandlers ) + " and/or " +
                    nameof( this.InitStage2_LoadPlugins ) + " was call prior to this function."
                );
            }

            this.ircBot = new IrcBot( this.ircConfig, this.handlers, this.infoLogFunction, this.errorLogFunction );
            this.ircBot.Start();
            this.fullyLoaded = true;
        }

        /// <summary>
        /// Tearsdown this class.
        /// </summary>
        public void Dispose()
        {
            if( this.fullyLoaded )
            {
                foreach( IPlugin plugin in plugins.Values )
                {
                    try
                    {
                        plugin.Teardown();
                    }
                    catch( Exception err )
                    {
                        this.errorLogFunction( "Error when tearing down plugin:" + Environment.NewLine + err.ToString() );
                    }
                }
                this.ircBot.Dispose();
            }
        }

        // -------- Default Handlers --------

        /// <summary>
        /// Handles the "list plugin" command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandlePluginListCommand( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessageToUser( this.pluginListResponse, response.Channel );
        }

        /// <summary>
        /// Handles the source command, which returns the source code of the plugin.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleSourceCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = Regex.Match( response.Message, this.sourceCommand, RegexOptions.IgnoreCase );
            if( match.Success )
            {
                string pluginName = match.Groups["pluginName"].Value.ToLower();
                if( this.plugins.ContainsKey( pluginName ) )
                {
                    string msg = "Source of the plugin " + pluginName + ": " + this.plugins[pluginName].SourceCodeLocation;
                    writer.SendMessageToUser( msg, response.Channel );
                }
                else if( pluginName == "chaskis" )
                {
                    string msg = "My source code is located: https://github.com/xforever1313/Chaskis";
                    writer.SendMessageToUser( msg, response.Channel );
                }
                else
                {
                    writer.SendMessageToUser( pluginName + " is not a plugin I have loaded...", response.Channel );
                }
            }
        }
    }
}