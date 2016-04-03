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
