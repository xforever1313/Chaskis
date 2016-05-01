
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using SethCS.Exceptions;

namespace GenericIrcBot
{
    /// <summary>
    /// Interface for an IrcConfig object.
    /// </summary>
    public interface IIrcConfig
    {
        /// <summary>
        /// The server to connect to.
        /// </summary>
        string Server { get; }

        /// <summary>
        /// The room to connect to (Include the # character in front if needed).
        /// </summary>
        string Channel { get; }

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
        /// The password to use to register the nick name.
        /// Leave empty to not authenticate.
        /// </summary>
        string Password { get; }

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
            this.Channel = string.Empty;
            this.Port = 6667; // Default IRC port.
            this.UserName = "SomeIrcBot";
            this.Nick = "SomeIrcBot";
            this.RealName = "Some IRC Bot";
            this.Password = string.Empty;
        }

        // -------- Properties --------

        /// <summary>
        /// The server to connect to.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// The room to connect to (Include the # character in front if needed).
        /// </summary>
        public string Channel { get; set; }

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
        /// The password to use to register the nick name.
        /// Leave empty to not authenticate.
        /// </summary>
        public string Password { get; set; }

        // --------- Functions --------

        /// <summary>
        /// Returns a copy of this object.
        /// </summary>
        public IIrcConfig Clone()
        {
            return ( IrcConfig ) this.MemberwiseClone();
        }

        /// <summary>
        /// Sees if the given object is equal to this instance.
        /// That is, all properties match.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the given object is equal to this one, else false.</returns>
        public override bool Equals( object obj )
        {
            IIrcConfig other = obj as IIrcConfig;

            if ( other == null )
            {
                return false;
            }

            return
                ( this.Server == other.Server ) &&
                ( this.Channel == other.Channel ) &&
                ( this.Port == other.Port ) &&
                ( this.UserName == other.UserName ) &&
                ( this.Nick == other.Nick ) &&
                ( this.RealName == other.RealName ) &&
                ( this.Password == other.Password );
        }

        /// <summary>
        /// Just returns the base object's hash code.
        /// </summary>
        /// <returns>The base object's hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Validates the IRC config to ensure no properties are
        /// bad, such as values being null/empty or negative.
        /// Only password can be empty, all others need some value in them.
        /// </summary>
        /// <exception cref="ApplicationException">If this object doesn't validate.</exception>
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
        private IIrcConfig wrappedConfig;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The config to wrap.</param>
        public ReadOnlyIrcConfig( IIrcConfig config )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );
            this.wrappedConfig = config;
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
        public string Channel
        {
            get
            {
                return this.wrappedConfig.Channel;
            }
            set
            {
                ThrowException( nameof( this.Channel ) );
            }
        }


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

        /// <summary>
        /// The password to use to register the nick name.
        /// Leave empty to not authenticate.
        /// </summary>
        public string Password
        {
            get
            {
                return this.wrappedConfig.Password;
            }
            set
            {
                ThrowException( nameof( this.Password ) );
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
            IIrcConfig other = obj as IIrcConfig;

            return
                ( this.Server == other.Server ) &&
                ( this.Channel == other.Channel ) &&
                ( this.Port == other.Port ) &&
                ( this.UserName == other.UserName ) &&
                ( this.Nick == other.Nick ) &&
                ( this.RealName == other.RealName ) &&
                ( this.Password == other.Password );
        }

        /// <summary>
        /// Just returns the base object's hash code.
        /// </summary>
        /// <returns>The base object's hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Validates the IRC config to ensure no properties are
        /// bad, such as values being null/empty or negative.
        /// Only password can be empty, all others need some value in them.
        /// </summary>
        /// <exception cref="ApplicationException">If this object doesn't validate.</exception>
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
            string errorString = "The following errors are wrong with the IrcConfig:" + Environment.NewLine;

            if ( string.IsNullOrEmpty( config.Server ) )
            {
                errorString += "Server can not be null or empty" + Environment.NewLine;
                success = false;
            }
            if ( string.IsNullOrEmpty( config.Channel ) )
            {
                errorString += "Channel can not be null or empty" + Environment.NewLine;
                success = false;
            }
            if ( config.Port < 0 )
            {
                errorString += "Port can not be null or empty" + Environment.NewLine;
                success = false;
            }
            if ( string.IsNullOrEmpty( config.UserName ) )
            {
                errorString += "UserName can not be null or empty" + Environment.NewLine;
                success = false;
            }
            if ( string.IsNullOrEmpty( config.Nick ) )
            {
                errorString += "Nick can not be null or empty" + Environment.NewLine;
                success = false;
            }
            if ( string.IsNullOrEmpty( config.RealName ) )
            {
                errorString += "RealName can not be null or empty" + Environment.NewLine;
                success = false;
            }

            // Password can be empty, its optional on servers.
            if ( success == false )
            {
                throw new ApplicationException( errorString );
            }
        }
    }
}
