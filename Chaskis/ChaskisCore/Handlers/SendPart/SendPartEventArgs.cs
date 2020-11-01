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
    /// Args that are passed into <see cref="SendPartEventHandler"/> when
    /// the bot leaves a channel.
    /// </summary>
    public sealed class SendPartEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_SendPart_event";

        // ---------------- Constructor ----------------

        internal SendPartEventArgs() :
            base()
        {
            this.Channel = null;
            this.PartReason = null;
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; internal set; }

        /// <summary>
        /// The channel the bot left.
        /// </summary>
        public string Channel { get; internal set; }

        /// <summary>
        /// The reason they bot left the channel.
        /// Set to <see cref="string."/> if there was no reason.
        /// </summary>
        public string PartReason { get; internal set; }

        protected override string XmlElementName => XmlRootName;

        protected override IEnumerable<XElement> ChildToXml()
        {
            return new List<XElement>
            {
                new XElement( "channel", this.Channel ),
                new XElement( "reason", this.PartReason )
            };
        }
    }

    /// <summary>
    /// Extensions to <see cref="SendPartEventArgs"/>
    /// </summary>
    internal static class SendPartEventArgsExtensions
    {
        public static SendPartEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendPartEventArgs args = new SendPartEventArgs
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
                else if( "reason".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    args.PartReason = child.Value;
                }
            }

            if( ( args.Channel == null ) || ( args.PartReason == null ) )
            {
                throw new ValidationException(
                    $"Could not find all required properties when creating {nameof( SendPartEventArgs )}"
                );
            }

            return args;
        }
    }
}
