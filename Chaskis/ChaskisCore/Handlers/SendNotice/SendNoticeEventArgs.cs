//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed into <see cref="SendNoticeEventHandler"/>
    /// </summary>
    public sealed class SendNoticeEventArgs : BaseSendMessageEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_sendnotice_event";

        // ---------------- Constructor ----------------

        public SendNoticeEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class SendNoticeEventArgsExtensions
    {
        public static SendNoticeEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendNoticeEventArgs args = new SendNoticeEventArgs();

            BaseSendMessageEventArgsExtensions.FromXml( args, xmlString, writer );

            return args;
        }
    }
}
