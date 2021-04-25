//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chaskis.Core;
using SethCS.Basic;
using SethCS.Extensions;

namespace Chaskis.Plugins.NewVersionNotifier
{
    [ChaskisPlugin( PluginName )]
    public class NewVersionNotifier : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.2.0";

        private const string cacheFileName = ".lastversion.txt";

        internal const string PluginName = "new_version_notifier";

        private string pluginDir;

        private IInterPluginEventCreator chaskisEventCreator;
        private IInterPluginEventSender eventSender;
        private IReadOnlyIrcConfig ircConfig;
        private GenericLogger logger;

        private string cachedFilePath;
        private string cachedVersion;

        private NewVersionNotifierConfig config;

        private readonly List<IIrcHandler> ircHandlers;

        /// <summary>
        /// A hashset of channels that were already notified of the 
        /// version update.
        /// </summary>
        private readonly HashSet<string> channelsNotified;

        // ---------------- Constructor ----------------

        public NewVersionNotifier()
        {
            this.ircHandlers = new List<IIrcHandler>();
            this.channelsNotified = new HashSet<string>();
        }

        // ---------------- Properties ----------------

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/NewVersionNotifier";
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
                return "I send a message when I first connect if my version has changed.";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor initor )
        {
            this.chaskisEventCreator = initor.ChaskisEventCreator;
            this.eventSender = initor.ChaskisEventSender;
            this.ircConfig = initor.IrcConfig;
            this.logger = initor.Log;

            this.pluginDir = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "NewVersionNotifier"
            );

            string configPath = Path.Combine(
                this.pluginDir,
                "NewVersionNotifierConfig.xml"
            );

            this.config = XmlLoader.LoadConfigFromFile( configPath );

            this.cachedFilePath = Path.Combine(
                this.pluginDir,
                cacheFileName
            );

            if( File.Exists( this.cachedFilePath ) == false )
            {
                this.cachedVersion = string.Empty;
            }
            else
            {
                string[] lines = File.ReadAllLines( this.cachedFilePath );
                if( lines.Length == 0 )
                {
                    this.cachedVersion = string.Empty;
                }
                else
                {
                    this.cachedVersion = lines[0].Trim();
                }
            }

            InterPluginEventHandler eventHandler = this.chaskisEventCreator.CreatePluginEventHandler(
                "chaskis",
                this.HandleChaskisEvent
            );
            this.ircHandlers.Add( eventHandler );

            JoinHandlerConfig joinHandlerConfig = new JoinHandlerConfig
            {
                JoinAction = this.OnJoinChannel,
                RespondToSelf = true
            };
            JoinHandler joinHandler = new JoinHandler( joinHandlerConfig );
            this.ircHandlers.Add( joinHandler );
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                this.About,
                msgArgs.Channel
            );
        }

        public IList<IIrcHandler> GetHandlers()
        {
            return this.ircHandlers.AsReadOnly();
        }

        public void Dispose()
        {
            // Nothing to Dispose.
        }

        private void OnJoinChannel( JoinHandlerArgs args )
        {
            if( args.User.Equals( this.ircConfig.Nick, StringComparison.InvariantCultureIgnoreCase ) )
            {
                string channel = args.Channel;

                InterPluginEvent e = this.chaskisEventCreator.CreateTargetedEvent(
                    "chaskis",
                    new Dictionary<string, string>()
                    {
                        ["QUERY"] = "VERSION",
                        ["PLUGIN"] = "chaskis"
                    },
                    new Dictionary<string, string>()
                    {
                        ["CHANNEL"] = channel
                    }
                );

                this.eventSender.SendInterPluginEvent( e );
            }
        }

        private void HandleChaskisEvent( ChaskisEventHandlerLineActionArgs args )
        {
            if( args.PluginName.Equals( "CHASKIS", StringComparison.InvariantCultureIgnoreCase ) )
            {
                this.HandleChaskisPlugin( args );
            }
        }

        private async void HandleChaskisPlugin( ChaskisEventHandlerLineActionArgs args )
        {
            if( args.EventArgs.ContainsKey( "ERROR" ) )
            {
                return;
            }

            if( args.EventArgs.ContainsKey( "VERSION" ) )
            {
                string channel = args.PassThroughArgs["CHANNEL"];
                if( this.channelsNotified.Contains( channel ) )
                {
                    this.logger.WriteLine(
                        "Channel {0} already notified of version update, skipping message",
                        channel
                    );
                    return;
                }
                else if(
                    ( this.config.ChannelsToSendTo.IsEmpty() == false ) &&
                    ( this.config.ChannelsToSendTo.Contains( channel ) == false )
                )
                {
                    this.logger.WriteLine(
                        "Channel {0} not in the list to send the version update notification, skipping message",
                        channel
                    );
                    return;
                }

                string versString = args.EventArgs["VERSION"];
                if( versString.Equals( this.cachedVersion ) == false )
                {
                    string msg = this.config.Message.Replace( "{%version%}", versString );
                    args.IrcWriter.SendMessage( msg, channel );

                    // Event handlers all happen on one thread,
                    // which is why we don't need a lock here.
                    this.channelsNotified.Add( channel );
                    if( this.channelsNotified.Count == 1 )
                    {
                        await Task.Run(
                            () =>
                            {
                                // If our version is new, we need to update the cached file
                                // so the next time the application runs, we don't
                                // send an unneeded message.
                                File.WriteAllText( this.cachedFilePath, versString );
                                this.logger.WriteLine( "{0}'s {1} file has been updated", PluginName, cacheFileName );
                            }
                        );
                    }
                }
                else
                {
                    this.logger.WriteLine( "Bot not updated, skipping message" );
                }
            }
        }
    }
}
