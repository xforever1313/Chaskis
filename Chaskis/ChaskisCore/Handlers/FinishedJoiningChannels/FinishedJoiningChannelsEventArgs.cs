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
    /// Args that are passed into <see cref="FinishedJoiningChannelsEventHandler"/>.
    /// </summary>
    public class FinishedJoiningChannelsEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_finishedjoiningchannels_event";

        // ---------------- Constructor ----------------

        internal FinishedJoiningChannelsEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; internal set; }

        protected override string XmlElementName => XmlRootName;
    }

    /// <summary>
    /// Extensions to <see cref="FinishedJoiningChannelsEventArgs"/>
    /// </summary>
    internal static class FinishedJoiningChannelsEventArgsExtensions
    {
        public static FinishedJoiningChannelsEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            FinishedJoiningChannelsEventArgs args = new FinishedJoiningChannelsEventArgs
            {
                Writer = writer
            };

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            return args;
        }
    }
}
