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
    internal class HourOfDayInfoKey : IEquatable<HourOfDayInfoKey>
    {
        // ----------------- Constructor -----------------

        public HourOfDayInfoKey()
        {
        }

        public HourOfDayInfoKey(
            Protocol protocol,
            string server,
            string channel,
            int hourOfDay
        )
        {
            this.Protocol = protocol;
            this.Server = server;
            this.Channel = channel;
            this.HourOfDay = hourOfDay;
        }

        // ----------------- Properties -----------------

        public Protocol Protocol { get; private set; }

        public string Server { get; private set; }

        public string Channel { get; private set; }

        public int HourOfDay { get; private set; }

        // ----------------- Functions -----------------

        public override bool Equals( object obj )
        {
            return base.Equals( obj );
        }

        public bool Equals( HourOfDayInfoKey other )
        {
            if( other == null )
            {
                return false;
            }

            return
                ( this.Protocol == other.Protocol ) &&
                this.Server.EqualsIgnoreCase( other.Server ) &&
                this.Channel.EqualsIgnoreCase( other.Channel ) &&
                ( this.HourOfDay == other.HourOfDay );
        }

        public override int GetHashCode()
        {
            return
                this.Protocol.GetHashCode() +
                StringComparer.InvariantCultureIgnoreCase.GetHashCode( this.Server ) +
                StringComparer.InvariantCultureIgnoreCase.GetHashCode( this.Channel ) +
                this.HourOfDay.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( $"{nameof( Protocol )}: {this.Protocol}" );
            builder.AppendLine( $"{nameof( Server )}: {this.Server ?? "[null]"}" );
            builder.AppendLine( $"{nameof( Channel )}: {this.Channel ?? "[null]"}" );
            builder.AppendLine( $"{nameof( HourOfDay )}: {this.HourOfDay}" );

            return builder.ToString();
        }
    }
}
