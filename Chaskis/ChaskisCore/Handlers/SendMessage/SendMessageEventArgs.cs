//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed into <see cref="SendMessageEventHandler"/>
    /// </summary>
    public sealed class SendMessageEventArgs : BaseSendMessageEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_sendmessage_event";

        // ---------------- Constructor ----------------

        public SendMessageEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class SendMessageEventArgsExtensions
    {
        public static SendMessageEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendMessageEventArgs args = new SendMessageEventArgs();

            BaseSendMessageEventArgsExtensions.FromXml( args, xmlString, writer );

            return args;
        }
    }
}
