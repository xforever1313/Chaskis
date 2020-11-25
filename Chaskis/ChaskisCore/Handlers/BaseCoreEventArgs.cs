//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Core
{
    /// <inheritdoc/>
    public abstract class BaseCoreEventArgs : ICoreEventArgs
    {
        // ---------------- Constructor ----------------

        protected BaseCoreEventArgs()
        {
        }

        // ---------------- Properties ----------------

        /// <inheritdoc/>
        public string Server { get; internal set; }

        /// <inheritdoc/>
        public ChaskisEventProtocol Protocol { get; internal set; }

        protected abstract string XmlElementName { get; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Helper function for parsing an XML string and validating it is correct.
        /// </summary>
        internal static XElement ParseXml<TArgs>( TArgs args, string xmlString )
            where TArgs : BaseCoreEventArgs
        {
            XElement root = XElement.Parse( xmlString );

            if( args.XmlElementName.EqualsIgnoreCase( root.Name.LocalName ) == false )
            {
                throw new ValidationException(
                    $"Invalid XML root name: {args.XmlElementName} for {typeof( TArgs ).Name}"
                );
            }

            return root;
        }

        /// <summary>
        /// Helper function for parsing XML for elements that are common for ALL
        /// connection events.
        /// </summary>
        internal static void ParseBaseXml( BaseCoreEventArgs args, XElement rootNode )
        {
            string server = null;
            ChaskisEventProtocol? protocol = null;
            foreach( XElement child in rootNode.Elements() )
            {
                if( "server".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    server = child.Value;
                }
                else if( "protocol".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    if( Enum.TryParse( child.Value, out ChaskisEventProtocol foundProtocol ) )
                    {
                        protocol = foundProtocol;
                    }
                }
            }

            if( ( server == null ) || ( protocol == null ) )
            {
                throw new ValidationException(
                    $"Could not find all required properties when creating {nameof( BaseCoreEventArgs )}"
                );
            }

            args.Server = server;
            args.Protocol = protocol.Value;
        }

        public string ToXml()
        {
            List<XElement> elements = new List<XElement>();
            elements.Add( new XElement( "server", this.Server ) );
            elements.Add( new XElement( "protocol", this.Protocol ) );

            IEnumerable<XElement> childElements = ChildToXml();
            if( childElements != null )
            {
                elements.AddRange( childElements );
            }

            XElement root = new XElement(
                this.XmlElementName,
                elements.ToArray()
            );

            return root.ToString( SaveOptions.DisableFormatting );
        }

        /// <summary>
        /// Override this to add child class XElements to the XML document.
        /// Each item in the list becomes a child under the root node.
        /// </summary>
        protected virtual IEnumerable<XElement> ChildToXml()
        {
            return null;
        }
    }
}
