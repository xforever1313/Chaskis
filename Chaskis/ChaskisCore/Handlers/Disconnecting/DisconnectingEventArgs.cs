//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Xml.Linq;

namespace Chaskis.Core
{
    /// <summary>
    /// Args that are passed into <see cref="DisconnectingEventHandler"/> when
    /// the bot is about to disconnect from the server.
    /// </summary>
    /// <remarks>
    /// No <see cref="IIrcWriter"/> is passed in since there is no guarentee that
    /// when this event gets processed we are connected still.
    /// </remarks>
    public class DisconnectingEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_disconnecting_event";


        // ---------------- Constructor ----------------

        internal DisconnectingEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class DisconnectingEventArgsExtensions
    {
        public static DisconnectingEventArgs FromXml( string xmlString )
        {
            DisconnectingEventArgs args = new DisconnectingEventArgs();

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            return args;
        }
    }
}
