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
    /// Args that are passed into <see cref="WatchdogFailedEventHandler"/> when we don't
    /// get a PONG from the server, and we need to reconnect.
    /// </summary>
    /// <remarks>
    /// No <see cref="IIrcWriter"/> is passed in since there is no IRC connection
    /// to write to.
    /// </remarks>
    public class WatchdogFailedEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_watchdogfailed_event";


        // ---------------- Constructor ----------------

        internal WatchdogFailedEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class WatchdogFailedEventArgsExtensions
    {
        public static WatchdogFailedEventArgs FromXml( string xmlString )
        {
            WatchdogFailedEventArgs args = new WatchdogFailedEventArgs();

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            return args;
        }
    }
}
