
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using GenericIrcBot;

namespace TestBot
{
    class MainClass
    {
        public static void Main( string[] args )
        {
            IrcBot bot = new IrcBot();
            try
            {
                bot.Derp();
            }
            catch(Exception e)
            {
                Console.WriteLine( e.Message );
            }
        }
    }
}
