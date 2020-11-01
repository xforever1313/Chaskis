//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Xml.Linq;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Core
{
    /// <summary>
    /// Args that are passed into <see cref="SendJoinEventHandler"/> when
    /// the bot attempts to join a channel.
    /// </summary>
    public sealed class SendJoinEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_sendjoin_event";

        // ---------------- Constructor ----------------

        internal SendJoinEventArgs() :
            base()
        {
            this.Channel = null;
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; internal set; }

        /// <summary>
        /// The channel the bot attempted to join.
        /// </summary>
        public string Channel { get; internal set; }
        protected override string XmlElementName => XmlRootName;

        protected override IEnumerable<XElement> ChildToXml()
        {
            return new List<XElement>
            {
                new XElement( "channel", this.Channel ),
            };
        }
    }

    /// <summary>
    /// Extensions to <see cref="SendJoinEventArgs"/>
    /// </summary>
    internal static class SendJoinEventArgsExtensions
    {
        public static SendJoinEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendJoinEventArgs args = new SendJoinEventArgs
            {
                Writer = writer
            };

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            foreach( XElement child in root.Elements() )
            {
                if( "channel".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    args.Channel = child.Value;
                }
            }

            if( args.Channel == null )
            {
                throw new ValidationException(
                    $"Could not find all required properties when creating {nameof( SendJoinEventArgs )}"
                );
            }

            return args;
        }
    }
}
