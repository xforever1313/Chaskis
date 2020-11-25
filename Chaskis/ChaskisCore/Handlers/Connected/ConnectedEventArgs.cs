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
    /// Args that are passed into <see cref="ConnectedEventHandler"/> when
    /// the bot connects to a server.
    /// </summary>
    public class ConnectedEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_connect_event";

        // ---------------- Constructor ----------------

        internal ConnectedEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; internal set; }

        protected override string XmlElementName => XmlRootName;
    }

    /// <summary>
    /// Extensions to <see cref="ConnectedEventArgs"/>
    /// </summary>
    internal static class ConnectedEventArgsExtensions
    {
        public static ConnectedEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            ConnectedEventArgs args = new ConnectedEventArgs
            {
                Writer = writer
            };

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            return args;
        }
    }
}
