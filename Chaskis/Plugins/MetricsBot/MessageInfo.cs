//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text;

namespace Chaskis.Plugins.MetricsBot
{
    internal sealed class MessageInfo
    {
        // ---------------- Constructor ----------------

        public MessageInfo() :
            this( null, 0 )
        {
        }

        public MessageInfo( MessageInfoKey key, long count )
        {
            this.Id = key;
            this.Count = count;
        }

        // ---------------- Properties ----------------

        public MessageInfoKey Id { get; set; }

        public long Count { get; set; }

        // ---------------- Functions ----------------

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( this.Id.ToString() );
            builder.AppendLine( $"{nameof( Count )}: {this.Count}" );

            return builder.ToString();
        }
    }
}
