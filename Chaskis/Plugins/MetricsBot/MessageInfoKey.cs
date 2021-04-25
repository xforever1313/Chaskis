//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;
using SethCS.Extensions;

namespace Chaskis.Plugins.MetricsBot
{
    internal sealed class MessageInfoKey : IEquatable<MessageInfoKey>
    {
        // ----------------- Constructor -----------------

        public MessageInfoKey()
        {
        }

        public MessageInfoKey(
            Protocol protocol,
            string server,
            string channel,
            string ircUser,
            MessageType messageType
        )
        {
            this.Protocol = protocol;
            this.Server = server;
            this.Channel = channel;
            this.IrcUser = ircUser;
            this.MessageType = messageType;
        }

        // ----------------- Properties -----------------

        public Protocol Protocol { get; private set; }

        public string Server { get; private set; }

        public string Channel { get; private set; }

        public string IrcUser { get; private set; }

        public MessageType MessageType { get; private set; }

        // ----------------- Functions -----------------

        public override bool Equals( object obj )
        {
            return base.Equals( obj );
        }

        public bool Equals( MessageInfoKey other )
        {
            if( other == null )
            {
                return false;
            }

            return
                ( this.Protocol == other.Protocol ) &&
                this.Server.EqualsIgnoreCase( other.Server ) &&
                this.Channel.EqualsIgnoreCase( other.Channel ) &&
                this.IrcUser.EqualsIgnoreCase( other.IrcUser ) &&
                ( this.MessageType == other.MessageType );
        }

        public override int GetHashCode()
        {
            return
                this.Protocol.GetHashCode() +
                StringComparer.InvariantCultureIgnoreCase.GetHashCode( this.Server ) +
                StringComparer.InvariantCultureIgnoreCase.GetHashCode( this.Channel ) +
                StringComparer.InvariantCultureIgnoreCase.GetHashCode( this.IrcUser ) +
                this.MessageType.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( $"{nameof( Protocol )}: {this.Protocol}" );
            builder.AppendLine( $"{nameof( Server )}: {this.Server ?? "[null]"}" );
            builder.AppendLine( $"{nameof( Channel )}: {this.Channel ?? "[null]"}" );
            builder.AppendLine( $"{nameof( IrcUser )}: {this.IrcUser ?? "[null]"}" );
            builder.AppendLine( $"{nameof( MessageType )}: {this.MessageType}" );

            return builder.ToString();
        }
    }
}
