//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;
using SethCS.Extensions;

namespace Chaskis.Plugins.MetricsBot
{
    internal sealed class DayOfWeekInfoKey : IEquatable<DayOfWeekInfoKey>
    {
        // ----------------- Constructor -----------------

        public DayOfWeekInfoKey()
        {
        }

        public DayOfWeekInfoKey(
            Protocol protocol,
            string server,
            string channel,
            DayOfWeek dayOfWeek
        )
        {
            this.Protocol = protocol;
            this.Server = server;
            this.Channel = channel;
            this.DayOfWeek = dayOfWeek;
        }

        // ----------------- Properties -----------------

        public Protocol Protocol { get; private set; }

        public string Server { get; private set; }

        public string Channel { get; private set; }

        public DayOfWeek DayOfWeek { get; private set; }

        // ----------------- Functions -----------------

        public override bool Equals( object obj )
        {
            return base.Equals( obj );
        }

        public bool Equals( DayOfWeekInfoKey other )
        {
            if( other == null )
            {
                return false;
            }

            return
                ( this.Protocol == other.Protocol ) &&
                this.Server.EqualsIgnoreCase( other.Server ) &&
                this.Channel.EqualsIgnoreCase( other.Channel ) &&
                ( this.DayOfWeek == other.DayOfWeek );
        }

        public override int GetHashCode()
        {
            return
                this.Protocol.GetHashCode() +
                StringComparer.InvariantCultureIgnoreCase.GetHashCode( this.Server ) +
                StringComparer.InvariantCultureIgnoreCase.GetHashCode( this.Channel ) +
                this.DayOfWeek.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( $"{nameof( Protocol )}: {this.Protocol}" );
            builder.AppendLine( $"{nameof( Server )}: {this.Server ?? "[null]"}" );
            builder.AppendLine( $"{nameof( Channel )}: {this.Channel ?? "[null]"}" );
            builder.AppendLine( $"{nameof( DayOfWeek )}: {this.DayOfWeek}" );

            return builder.ToString();
        }
    }
}
