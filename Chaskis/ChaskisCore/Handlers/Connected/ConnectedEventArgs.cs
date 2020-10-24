//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Xml.Linq;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Core
{
    /// <summary>
    /// Args that are passed into <see cref="ConnectedEventHandler"/> when
    /// the bot connects to a server.
    /// </summary>
    public class ConnectedEventArgs : BaseConnectionEventArgs
    {
        // ---------------- Constructor ----------------

        public ConnectedEventArgs( string server, ChaskisEventProtocol protocol, IIrcWriter writer ) :
            base( server, protocol )
        {
            this.Writer = writer;
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; private set; }
    }

    /// <summary>
    /// Extensions to <see cref="ConnectedEventArgs"/>
    /// </summary>
    internal static class ConnectedEventArgsExtensions
    {
        internal const string XmlElementName = "chaskis_connect_event";

        public static ConnectedEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            XElement root = XElement.Parse( xmlString );
            string server = null;
            ChaskisEventProtocol? protocol = null;

            if( XmlElementName.EqualsIgnoreCase( root.Name.LocalName ) == false )
            {
                throw new ValidationException(
                    $"Invalid XML root name: {XmlElementName} for {nameof( ConnectedEventArgs )}"
                );
            }

            foreach( XElement child in root.Elements() )
            {
                if( "server".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    server = child.Value;
                }
                else if( "protocol".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    if( Enum.TryParse<ChaskisEventProtocol>( child.Value, out ChaskisEventProtocol foundProtocol ) )
                    {
                        protocol = foundProtocol;
                    }
                }
            }

            if( ( server != null ) && ( protocol != null ) )
            {
                return new ConnectedEventArgs( server, protocol.Value, writer );
            }
            else
            {
                throw new ValidationException(
                    $"Could not find all required properties when creating {nameof( ConnectedEventArgs )}"
                );
            }
        }

        public static string ToXml( this ConnectedEventArgs args )
        {
            XElement root = new XElement(
                XmlElementName,
                new XElement( "server", args.Server ),
                new XElement( "protocol", args.Protocol.ToString() )
            );

            return root.ToString( SaveOptions.DisableFormatting );
        }
    }
}
