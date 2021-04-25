//
//          Copyright Seth Hendrick 2016-2021.
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
    public abstract class BaseSendMessageEventArgs : BaseCoreEventArgs
    {
        // ---------------- Constructor ----------------

        protected BaseSendMessageEventArgs() :
            base()
        {
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; internal set; }

        /// <summary>
        /// Where the message was sent to.
        /// </summary>
        public string ChannelOrUser { get; internal set; }

        public string Message { get; internal set; }

        protected sealed override IEnumerable<XElement> ChildToXml()
        {
            return new List<XElement>
            {
                new XElement( "channel", this.ChannelOrUser ),
                new XElement( "message", this.Message )
            };
        }
    }

    internal static class BaseSendMessageEventArgsExtensions
    {
        public static void FromXml( BaseSendMessageEventArgs args, string xmlString, IIrcWriter writer )
        {
            args.Writer = writer;

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            foreach( XElement child in root.Elements() )
            {
                if( "channel".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    args.ChannelOrUser = child.Value;
                }
                else if( "message".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    args.Message = child.Value;
                }
            }

            if( ( args.ChannelOrUser == null ) || ( args.Message == null ) )
            {
                throw new ValidationException(
                    $"Could not find all required properties when creating {nameof( BaseSendMessageEventArgs )}"
                );
            }
        }
    }
}
