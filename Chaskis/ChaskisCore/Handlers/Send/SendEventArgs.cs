//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Core
{
    /// <summary>
    /// Args that are passed into <see cref="SendEventHandler"/> when
    /// *any* command is sent to the server.
    /// </summary>
    public sealed class SendEventArgs : BaseCoreEventArgs
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "chaskis_send_event";

        // ---------------- Constructor ----------------

        internal SendEventArgs() :
            base()
        {
            this.Command = null;
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; internal set; }

        /// <summary>
        /// The IRC command sent to the server.
        /// </summary>
        public string Command { get; internal set; }

        protected override string XmlElementName => XmlRootName;

        protected override IEnumerable<XElement> ChildToXml()
        {
            // It is possible for a character to not be a valid XML
            // character since we can send bytes with some commands (such as CTCP Ping).
            // So, we need to escape these characters, while still making them
            // readable in the log.  We will do that by
            // replacing them with [hexValue] in the XML, then
            // putting them back on the otherside.
            bool escaped = false;
            StringBuilder builder = new StringBuilder();
            foreach( char c in this.Command )
            {
                if(
                    ( XmlConvert.IsXmlChar( c ) == false ) ||
                    ( c == '[' ) ||
                    ( c == ']' ) 
                )
                {
                    escaped = true;
                    builder.Append( $"[{Convert.ToInt32( c ):X}]" );
                }
                else
                {
                    builder.Append( c );
                }
            }

            XElement commandElement = new XElement( "command", builder );
            commandElement.Add( new XAttribute( "escaped", escaped ) );
            return new List<XElement>
            {
                commandElement
            };
        }
    }

    /// <summary>
    /// Extensions to <see cref="SendEventArgs"/>
    /// </summary>
    internal static class SendEventArgsExtensions
    {
        public static SendEventArgs FromXml( string xmlString, IIrcWriter writer )
        {
            SendEventArgs args = new SendEventArgs
            {
                Writer = writer
            };

            XElement root = BaseCoreEventArgs.ParseXml( args, xmlString );
            BaseCoreEventArgs.ParseBaseXml( args, root );

            foreach( XElement child in root.Elements() )
            {
                if( "command".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    // Assume true, we don't want things to crash if this
                    // was somehow false.
                    bool escaped = true;
                    foreach( XAttribute attr in child.Attributes() )
                    {
                        if( "escaped".EqualsIgnoreCase( attr.Name.LocalName ) )
                        {
                            if( bool.TryParse( attr.Value, out escaped ) == false )
                            {
                                escaped = false;
                            }
                        }
                    }

                    // If we did not escape, just assign the value,
                    // no need to over-complciate things.
                    if( escaped == false )
                    {
                        args.Command = child.Value;
                    }
                    else
                    {
                        args.Command = ParseCommand( child.Value );
                    }
                }
            }

            if( args.Command == null )
            {
                throw new ValidationException(
                    $"Could not find all required properties when creating {nameof( SendEventArgs )}"
                );
            }

            return args;
        }

        private static string ParseCommand( string originalCommand )
        {
            StringBuilder builder = new StringBuilder();

            bool parse = false;
            StringBuilder parser = new StringBuilder();
            foreach( char c in originalCommand )
            {
                if( c == '[' )
                {
                    parse = true;
                }
                else if( c == ']' )
                {
                    parse = false;
                    builder.Append( Convert.ToChar( int.Parse( parser.ToString(), NumberStyles.HexNumber ) ) );
                    parser.Clear();
                }
                else if( parse )
                {
                    parser.Append( c );
                }
                else
                {
                    builder.Append( c );
                }
            }

            return builder.ToString();
        }
    }
}
