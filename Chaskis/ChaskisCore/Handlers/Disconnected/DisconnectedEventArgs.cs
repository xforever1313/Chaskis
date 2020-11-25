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
    /// Args that are passed into <see cref="DisconnectedEventHandler"/> when
    /// the bot has finished disconnecting from the server.
    /// </summary>
    /// <remarks>
    /// No <see cref="IIrcWriter"/> is passed in since there is no connection to write
    /// to.
    /// </remarks>
    public class DisconnectedEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_disconnected_event";


        // ---------------- Constructor ----------------

        internal DisconnectedEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class DisconnectedEventArgsExtensions
    {
        public static DisconnectedEventArgs FromXml( string xmlString )
        {
            DisconnectedEventArgs args = new DisconnectedEventArgs();

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            return args;
        }
    }
}
