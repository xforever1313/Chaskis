//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public static class Parsing
    {
        /// <summary>
        /// Replaces {%user%}, {%nick%}, {%channel%} with the values
        /// from the given IRC config.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <param name="user">The remote user who sent the command.  Null to ignore {%user%}.</param>
        /// <param name="nick">The nickname of the bot.  Null to ignore {%nick%}.</param>
        /// <param name="channel">The channel name.  Null to ignore {%channel%}.</param>
        public static string LiquefyStringWithIrcConfig( string str, string user = null, string nick = null, string channel = null )
        {
            ArgumentChecker.IsNotNull( str, nameof( str ) );

            StringBuilder builder = new StringBuilder( str );
            if( user != null )
            {
                builder.Replace( "{%user%}", user );
            }

            if( nick != null )
            {
                builder.Replace( "{%nick%}", nick );
            }

            if( channel != null )
            {
                builder.Replace( "{%channel%}", channel );
            }

            return builder.ToString();
        }
    }
}
