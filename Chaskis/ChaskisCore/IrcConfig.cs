//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SethCS.Exceptions;

namespace ChaskisCore
{
    /// <summary>
    /// Interface for an IrcConfig object.
    /// </summary>
    public interface IIrcConfig : IEquatable<IIrcConfig>
    {
        /// <summary>
        /// The server to connect to.
        /// </summary>
        string Server { get; }

        /// <summary>
        /// The room to connect to (Include the # character in front if needed).
        /// </summary>
        IList<string> Channels { get; }

        /// <summary>
        /// The port to connect to.
        /// </summary>
        short Port { get; }

        /// <summary>
        /// The bot's user name.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// The bot's nick name
        /// </summary>
        string Nick { get; }

        /// <summary>
        /// The bot's "Real Name"
        /// </summary>
        string RealName { get; }

        /// <summary>
        /// The password to use to enter the server.
        /// Leave empty to not authenticate to the server.
        /// </summary>
        string ServerPassword { get; }

        /// <summary>
        /// The password to use to register the nick name.
        /// Leave empty to not authenticate.
        /// </summary>
        string NickServPassword { get; }

        /// <summary>
        /// The quit message when the bot is leaving.
        /// Must be less than 160 characters and contain no new lines.
        /// </summary>
        string QuitMessage { get; }

        /// <summary>
        /// Dictionary of bots that act as bridges to other clients (e.g. telegram).
        /// Key is the bridge's user name.  Regexes are allowed in case of rejoinings (e.g. bridgeBot1, bridgeBot2, bridgeBot).
        /// Value a regex.  Must include (?<bridgeUser>) and (<bridgeMessage>) regex groups.
        /// somewhere in it so we can tell who the user name from the bridged service and what the
        /// message was.
        ///
        /// For example, if your bridge bot will output message in the form of:
        /// bridgeUser: This is the user's message from the bridge
        /// then your regex should be:
        /// (?<bridgeUser>\w+:)\s+(?<bridgeMessage>.+)
        ///
        /// If you have no bridges to watch in your channel, leave this blank.
        /// </summary>
        IDictionary<string, string> BridgeBots { get; }

        /// <summary>
        /// Admins who control the bot.
        /// Plugins can use this to see who has elevated permissions and can
        /// do things such as delete entries, etc.
        /// </summary>
        IList<string> Admins { get; }

        /// <summary>
        /// How long between sending messages to the IRC channel in milliseconds.
        /// Each server and/or channel usually has a flood limit that the bot needs
        /// to observe or risk being kicked/banned.
        /// </summary>
        int RateLimit { get; }

        // -------- Functions ---------

        /// <summary>
        /// Clone this instance.
        /// </summary>
        IIrcConfig Clone();

        /// <summary>
        /// Sees if the given object is equal to this instance.
        /// That is, all properties match.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the given object is equal to this one, else false.</returns>
        bool Equals( object other );

        /// <summary>
        /// Validates the IRC config to ensure no properties are
        /// bad, such as values being null/empty or negative.
        /// Only password can be empty, all others need some value in them.
        /// </summary>
        void Validate();
    }

    /// <summary>
    /// Mutable IrcConfig object.
    /// </summary>
    public class IrcConfig : IIrcConfig
    {
        // -------- Constructor --------

        /// <summary>
        /// Contructor, fills with default settings.
        /// </summary>
        public IrcConfig()
        {
            this.Server = string.Empty;
            this.Channels = new List<string>();
            this.Port = 6667; // Default IRC port.
            this.UserName = "SomeIrcBot";
            this.Nick = "SomeIrcBot";
            this.RealName = "Some IRC Bot";
            this.ServerPassword = string.Empty;
            this.NickServPassword = string.Empty;
            this.BridgeBots = new Dictionary<string, string>();
            this.Admins = new List<string>();
            this.QuitMessage = string.Empty;
            this.RateLimit = 0;
        }

        // -------- Properties --------

        /// <summary>
        /// The server to connect to.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// The room to connect to (Include the # character in front if needed).
        /// </summary>
        public IList<string> Channels { get; private set; }

        /// <summary>
        /// The port to connect to.
        /// </summary>
        public short Port { get; set; }

        /// <summary>
        /// The bot's user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The bot's nick name
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// The bot's "Real Name"
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// The password to use to enter the server.
        /// Leave empty to not authenticate to the server.
        /// </summary>
        public string ServerPassword { get; set; }

        /// <summary>
        /// The password to use to register the nick name.
        /// Leave empty to not authenticate.
        /// </summary>
        public string NickServPassword { get; set; }

        /// <summary>
        /// The quit message when the bot is leaving.
        /// Must be less than 160 characters and contain no new lines.
        /// </summary>
        public string QuitMessage { get; set; }

        /// <summary>
        /// Mutable Dictionary of bots that act as bridges to other clients (e.g. telegram).
        /// Key is the bridge's user name
        /// Value a regex.  Must include (?<bridgeUser>) and (?<bridgeMessage>) regex groups.
        /// </summary>
        public IDictionary<string, string> BridgeBots { get; private set; }

        /// <summary>
        /// Admins who control the bot.
        /// Plugins can use this to see who has elevated permissions and can
        /// do things such as delete entries, etc.
        /// </summary>
        public IList<string> Admins { get; private set; }

        /// <summary>
        /// How long between sending messages to the IRC channel in milliseconds.
        /// Each server and/or channel usually has a flood limit that the bot needs
        /// to observe or risk being kicked/banned.
        /// </summary>
        public int RateLimit { get; set; }

        // --------- Functions --------

        /// <summary>
        /// Returns a copy of this object.
        /// </summary>
        public IIrcConfig Clone()
        {
            IrcConfig clone = (IrcConfig)this.MemberwiseClone();

            clone.Channels = new List<string>( clone.Channels );
            clone.BridgeBots = new Dictionary<string, string>( clone.BridgeBots );
            clone.Admins = new List<string>( clone.Admins );

            return clone;
        }

        /// <summary>
        /// Sees if the given object is equal to this instance.
        /// That is, all properties match.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the given object is equal to this one, else false.</returns>
        public override bool Equals( object obj )
        {
            return IrcConfigHelpers.Equals( this, obj );
        }

        public bool Equals( IIrcConfig other )
        {
            return IrcConfigHelpers.Equals( this, other );
        }

        /// <summary>
        /// Just returns the base object's hash code.
        /// </summary>
        /// <returns>The base object's hash code.</returns>
        public override int GetHashCode()
        {
            return IrcConfigHelpers.GetHashCode( this );
        }

        /// <summary>
        /// Validates the IRC config to ensure no properties are
        /// bad, such as values being null/empty or negative.
        /// Only password can be empty, all others need some value in them.
        /// </summary>
        /// <exception cref="ValidationException">If this object doesn't validate.</exception>
        public void Validate()
        {
            IrcConfigHelpers.Validate( this );
        }
    }

    /// <summary>
    /// Wraps an IIrcConfig object such that its readonly.
    /// Note: All setters throw ReadOnlyException.
    /// </summary>
    public class ReadOnlyIrcConfig : IIrcConfig
    {
        // -------- Fields --------

        /// <summary>
        /// The wrapped config.
        /// </summary>
        private readonly IIrcConfig wrappedConfig;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The config to wrap.</param>
        public ReadOnlyIrcConfig( IIrcConfig config )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            this.wrappedConfig = config;

            this.Channels = new List<string>( config.Channels ).AsReadOnly();
            this.BridgeBots = new ReadOnlyDictionary<string, string>( config.BridgeBots );
            this.Admins = new List<string>( config.Admins ).AsReadOnly();
        }

        // -------- Properties --------

        /// <summary>
        /// The server to connect to.
        /// </summary>
        public string Server
        {
            get
            {
                return this.wrappedConfig.Server;
            }
            set
            {
                ThrowException( nameof( this.Server ) );
            }
        }

        /// <summary>
        /// The room to connect to (Include the # character in front if needed).
        /// </summary>
        public IList<string> Channels { get; private set; }

        /// <summary>
        /// The port to connect to.
        /// </summary>
        public short Port
        {
            get
            {
                return this.wrappedConfig.Port;
            }
            set
            {
                ThrowException( nameof( this.Port ) );
            }
        }

        /// <summary>
        /// The bot's user name.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.wrappedConfig.UserName;
            }
            set
            {
                ThrowException( nameof( this.UserName ) );
            }
        }

        /// <summary>
        /// The bot's nick name
        /// </summary>
        public string Nick
        {
            get
            {
                return this.wrappedConfig.Nick;
            }
            set
            {
                ThrowException( nameof( this.Nick ) );
            }
        }

        /// <summary>
        /// The bot's "Real Name"
        /// </summary>
        public string RealName
        {
            get
            {
                return this.wrappedConfig.RealName;
            }
            set
            {
                ThrowException( nameof( this.RealName ) );
            }
        }

        public string ServerPassword
        {
            get
            {
                return this.wrappedConfig.ServerPassword;
            }
            set
            {
                ThrowException( nameof( this.ServerPassword ) );
            }
        }

        /// <summary>
        /// The password to use to register the nick name.
        /// Leave empty to not authenticate.
        /// </summary>
        public string NickServPassword
        {
            get
            {
                return this.wrappedConfig.NickServPassword;
            }
            set
            {
                ThrowException( nameof( this.NickServPassword ) );
            }
        }

        /// <summary>
        /// The quit message when the bot is leaving.
        /// Must be less than 160 characters and contain no new lines.
        /// </summary>
        public string QuitMessage
        {
            get
            {
                return this.wrappedConfig.QuitMessage;
            }
            set
            {
                ThrowException( nameof( this.QuitMessage ) );
            }
        }

        /// <summary>
        /// Read-only Dictionary of bots that act as bridges to other clients (e.g. telegram).
        /// Key is the bridge's user name
        /// Value a regex.  Must include (?<bridgeUser>) and (?<bridgeMessage>) regex groups.
        /// </summary>
        public IDictionary<string, string> BridgeBots { get; private set; }

        /// <summary>
        /// Admins who control the bot.
        /// Plugins can use this to see who has elevated permissions and can
        /// do things such as delete entries, etc.
        /// 
        /// Read-only list.
        /// </summary>
        public IList<string> Admins { get; private set; }

        /// <summary>
        /// How long between sending messages to the IRC channel in milliseconds.
        /// Each server and/or channel usually has a flood limit that the bot needs
        /// to observe or risk being kicked/banned.
        /// </summary>
        public int RateLimit
        {
            get
            {
                return this.wrappedConfig.RateLimit;
            }
            set
            {
                ThrowException( nameof( this.RateLimit ) );
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Returns a copy of this object.
        /// </summary>
        public IIrcConfig Clone()
        {
            return new ReadOnlyIrcConfig( this.wrappedConfig.Clone() );
        }

        /// <summary>
        /// Sees if the given object is equal to this instance.
        /// That is, all properties match.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the given object is equal to this one, else false.</returns>
        public override bool Equals( object obj )
        {
            return IrcConfigHelpers.Equals( this, obj );
        }

        public bool Equals( IIrcConfig other )
        {
            return IrcConfigHelpers.Equals( this, other );
        }

        /// <summary>
        /// Just returns the base object's hash code.
        /// </summary>
        /// <returns>The base object's hash code.</returns>
        public override int GetHashCode()
        {
            return IrcConfigHelpers.GetHashCode( this );
        }

        /// <summary>
        /// Validates the IRC config to ensure no properties are
        /// bad, such as values being null/empty or negative.
        /// Only password can be empty, all others need some value in them.
        /// </summary>
        /// <exception cref="ValidationException">If this object doesn't validate.</exception>
        public void Validate()
        {
            IrcConfigHelpers.Validate( this );
        }

        /// <summary>
        /// Throws the read-only exception.
        /// </summary>
        /// <param name="property">The property that was called.</param>
        private static void ThrowException( string property )
        {
            throw new ReadOnlyException( "Can't modify " + property + ", this config is readonly" );
        }
    }

    /// <summary>
    /// Helpers that can be used for all Irc Config object types.
    /// </summary>
    internal static class IrcConfigHelpers
    {
        /// <summary>
        /// Validates the IRC config to ensure no properties are
        /// bad, such as values being null/empty or negative.
        /// Only password can be empty, all others need some value in them.
        /// </summary>
        /// <param name="config">Config to validate.</param>
        internal static void Validate( IIrcConfig config )
        {
            bool success = true;
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( "The following errors are wrong with the IrcConfig:" );

            if( string.IsNullOrEmpty( config.Server ) )
            {
                builder.AppendLine( "-\tServer can not be null or empty" );
                success = false;
            }

            if( config.Channels == null )
            {
                builder.AppendLine( "-\tChannel can not be null or empty" );
                success = false;
            }
            else if( config.Channels.Count == 0 )
            {
                builder.AppendLine( "-\tMust contain at least one channel." );
                success = false;
            }
            else if( config.Channels.Any( c => string.IsNullOrEmpty( c ) || string.IsNullOrWhiteSpace( c ) ) )
            {
                builder.AppendLine( "\t-Channels can not be null, empty, or whitespace." );
                success = false;
            }

            if( config.Port < 0 )
            {
                builder.AppendLine( "-\tPort can not be null or empty" );
                success = false;
            }
            if( string.IsNullOrEmpty( config.UserName ) )
            {
                builder.AppendLine( "-\tUserName can not be null or empty" );
                success = false;
            }
            if( string.IsNullOrEmpty( config.Nick ) )
            {
                builder.AppendLine( "-\tNick can not be null or empty" );
                success = false;
            }
            if( string.IsNullOrEmpty( config.RealName ) )
            {
                builder.AppendLine( "-\tRealName can not be null or empty" );
                success = false;
            }
            if( config.QuitMessage == null )
            {
                builder.AppendLine( "-\tQuit Message can not be null" );
                success = false;
            }
            // Per this website, quit messages can not contain new lines:
            // http://www.user-com.undernet.org/documents/quitmsg.php
            else if( config.QuitMessage.Contains( Environment.NewLine ) )
            {
                builder.AppendLine( "-\tQuit Message can not contain new lines" );
                success = false;
            }
            // Per this website, quit messages can not contain more than 160 characters.
            // http://www.user-com.undernet.org/documents/quitmsg.php
            else if( config.QuitMessage.Length > 160 )
            {
                builder.AppendLine( "-\tQuit Message can not contain more than 160 characters" );
                success = false;
            }
            // Bridge bots MAY be empty, but can not be null.
            if( config.BridgeBots == null )
            {
                builder.AppendLine( "-\tBridgeBots can not be null" );
                success = false;
            }
            else
            {
                foreach( KeyValuePair<string, string> bridgeBot in config.BridgeBots )
                {
                    if( string.IsNullOrEmpty( bridgeBot.Key ) )
                    {
                        builder.AppendLine( "-\tBrideBots can not have empty or null Key" );
                        success = false;
                    }
                    if( string.IsNullOrEmpty( bridgeBot.Value ) )
                    {
                        builder.AppendLine( "-\tBrideBots " + bridgeBot.Key + " can not have empty or null Value" );
                        success = false;
                    }
                    else
                    {
                        if( bridgeBot.Value.Contains( @"?<bridgeUser>" ) == false )
                        {
                            builder.AppendLine( "-\tBrideBots " + bridgeBot.Key + " must have regex group 'bridgeUser' in it" );
                            success = false;
                        }
                        if( bridgeBot.Value.Contains( @"?<bridgeMessage>" ) == false )
                        {
                            builder.AppendLine( "-\tBrideBots " + bridgeBot.Key + " must have regex group 'bridgeMessage' in it" );
                            success = false;
                        }
                    }
                }
            }

            if( config.Admins == null )
            {
                builder.AppendLine( "-\tAdmins can not be null." );
                success = false;
            }
            else
            {
                foreach( string admin in config.Admins )
                {
                    if( string.IsNullOrEmpty( admin ) || string.IsNullOrWhiteSpace( admin ) )
                    {
                        builder.AppendLine( "-\tAdmin can not be null, empty, or whitespace." );
                        success = false;
                    }
                }
            }

            if( config.RateLimit < 0 )
            {
                builder.AppendLine( "-\tRate Limit can not be negative." );
                success = false;
            }

            // Password can be empty, its optional on servers.

            if( success == false )
            {
                throw new ValidationException( builder.ToString() );
            }
        }

        /// <summary>
        /// Gets the hash code of the IRC config object.
        /// </summary>
        /// <param name="config">The config to get the hash code of.</param>
        /// <returns>The hash code of the IRC config object.</returns>
        internal static int GetHashCode( IIrcConfig config )
        {
            return
                config.Server.GetHashCode() +
                config.Channels.GetHashCode() +
                config.Port.GetHashCode() +
                config.UserName.GetHashCode() +
                config.Nick.GetHashCode() +
                config.RealName.GetHashCode() +
                config.ServerPassword.GetHashCode() +
                config.NickServPassword.GetHashCode() +
                config.QuitMessage.GetHashCode() +
                config.BridgeBots.GetHashCode() +
                config.Admins.GetHashCode() +
                config.RateLimit.GetHashCode();
        }

        /// <summary>
        /// Checks to see if the given IIrcConfig object is the same
        /// as the given object.
        /// </summary>
        /// <param name="config1">The IIrcConfig object to check.</param>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the given object is equal to this one, else false.</returns>
        internal static bool Equals( IIrcConfig config1, object config2 )
        {
            IIrcConfig other = config2 as IIrcConfig;

            if( other == null )
            {
                return false;
            }

            return Equals( config1, other );
        }

        internal static bool Equals( IIrcConfig config1, IIrcConfig other )
        {
            bool isEqual =
                ( config1.Server == other.Server ) &&
                ( config1.Channels.Count == other.Channels.Count ) &&
                ( config1.Port == other.Port ) &&
                ( config1.UserName == other.UserName ) &&
                ( config1.Nick == other.Nick ) &&
                ( config1.RealName == other.RealName ) &&
                ( config1.ServerPassword == other.ServerPassword ) &&
                ( config1.NickServPassword == other.NickServPassword ) &&
                ( config1.QuitMessage == other.QuitMessage ) &&
                ( config1.BridgeBots.Count == other.BridgeBots.Count ) &&
                ( config1.Admins.Count == other.Admins.Count ) &&
                ( config1.RateLimit == other.RateLimit );

            if( isEqual )
            {
                foreach( string channel in config1.Channels )
                {
                    if( other.Channels.Contains( channel ) == false )
                    {
                        isEqual = false;
                        break;
                    }
                }
            }

            if( isEqual )
            {
                foreach( KeyValuePair<string, string> bridgeBot in config1.BridgeBots )
                {
                    if( other.BridgeBots.ContainsKey( bridgeBot.Key ) )
                    {
                        if( bridgeBot.Value != other.BridgeBots[bridgeBot.Key] )
                        {
                            isEqual = false;
                            break;
                        }
                    }
                    else
                    {
                        isEqual = false;
                        break;
                    }
                }
            }

            if( isEqual )
            {
                foreach( string admin in config1.Admins )
                {
                    if( other.Admins.Contains( admin ) == false )
                    {
                        isEqual = false;
                        break;
                    }
                }
            }

            return isEqual;
        }
    }
}