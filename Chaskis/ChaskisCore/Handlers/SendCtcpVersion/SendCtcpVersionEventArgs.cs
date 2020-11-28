//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed into <see cref="SendCtcpVersionEventHandler"/>
    /// </summary>
    public sealed class SendCtcpVersionEventArgs : BaseSendMessageEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_sendctcpversion_event";

        // ---------------- Constructor ----------------

        public SendCtcpVersionEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class SendCtcpVersionEventArgsExtensions
    {
        public static SendCtcpVersionEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendCtcpVersionEventArgs args = new SendCtcpVersionEventArgs();

            BaseSendMessageEventArgsExtensions.FromXml( args, xmlString, writer );

            return args;
        }
    }
}
