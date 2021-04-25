//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed into <see cref="SendActionEventHandler"/>
    /// </summary>
    public sealed class SendActionEventArgs : BaseSendMessageEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_sendaction_event";

        // ---------------- Constructor ----------------

        public SendActionEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class SendActionEventArgsExtensions
    {
        public static SendActionEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendActionEventArgs args = new SendActionEventArgs();

            BaseSendMessageEventArgsExtensions.FromXml( args, xmlString, writer );

            return args;
        }
    }
}
