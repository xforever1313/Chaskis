//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed into <see cref="SendCtcpPongEventHandler"/>
    /// </summary>
    public sealed class SendCtcpPongEventArgs : BaseSendMessageEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_sendctcppong_event";

        // ---------------- Constructor ----------------

        public SendCtcpPongEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        protected override string XmlElementName => XmlRootName;
    }

    internal static class SendCtcpPongEventArgsExtensions
    {
        public static SendCtcpPongEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendCtcpPongEventArgs args = new SendCtcpPongEventArgs();

            BaseSendMessageEventArgsExtensions.FromXml( args, xmlString, writer );

            return args;
        }
    }
}
