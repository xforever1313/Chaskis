//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using SethCS.Extensions;

namespace Chaskis.Plugins.MetricsBot
{
    internal sealed class MessageInfoKey : IEquatable<MessageInfoKey>
    {
        // ----------------- Constructor -----------------

        public MessageInfoKey()
        {
        }

        // ----------------- Properties -----------------

        public Protocol Protocol { get; set; }

        public string Server { get; set; }

        public string Channel { get; set; }

        public string IrcUser { get; set; }

        public MessageType MessageType { get; set; }

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
                ( this.Server?.GetHashCode() ?? 0 ) +
                ( this.Channel?.GetHashCode() ?? 0 ) +
                ( this.IrcUser?.GetHashCode() ?? 0 ) +
                this.MessageType.GetHashCode();
        }

        public MessageInfoKey Clone()
        {
            return (MessageInfoKey)this.MemberwiseClone();
        }
    }

    internal sealed class MessageInfo
    {
        public MessageInfo() :
            this( null, 0 )
        {
        }

        public MessageInfo( MessageInfoKey key, long count )
        {
            this.Id = key;
            this.Count = count;
        }

        public MessageInfoKey Id { get; set; }

        public long Count { get; set; }
    }
}
