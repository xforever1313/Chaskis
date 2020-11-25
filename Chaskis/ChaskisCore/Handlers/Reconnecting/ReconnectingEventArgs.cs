//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Xml.Linq;

namespace Chaskis.Core
{
    /// <summary>
    /// Args that are passed into <see cref="ReconnectingEventHandler"/> when
    /// the bot is going to ATTEMPT to reconnect to the server.
    /// </summary>
    /// <remarks>
    /// No <see cref="IIrcWriter"/> is passed in since there is no IRC connection
    /// to write to.
    /// </remarks>
    public class ReconnectingEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_reconnecting_event";


        // ---------------- Constructor ----------------

        internal ReconnectingEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class ReconnectingEventArgsExtensions
    {
        public static ReconnectingEventArgs FromXml( string xmlString )
        {
            ReconnectingEventArgs args = new ReconnectingEventArgs();

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            return args;
        }
    }
}
