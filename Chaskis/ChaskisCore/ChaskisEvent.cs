//
//          Copyright Seth Hendrick 2017-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ChaskisCore
{
    /// <summary>
    /// This class represents an event that can be sent
    /// to all plugins, without the need of plugins
    /// to be aware of each other.
    /// </summary>
    public class ChaskisEvent
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// String for a broadcast event.
        /// </summary>
        public const string BroadcastEventStr = "BCAST";

        public const string XmlElementName = "chaskis_event";

        // ---------------- Constructor ----------------

        internal static ChaskisEvent FromXml( string xmlString )
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml( xmlString );

            ChaskisEvent e = new ChaskisEvent();

            XmlNode rootNode = doc.DocumentElement;
            if( rootNode.Name != XmlElementName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + XmlElementName + "\".  Got: " + rootNode.Name
                );
            }

            // Get Attributes
            foreach( XmlAttribute attr in rootNode.Attributes )
            {
                switch( attr.Name )
                {
                    case "source_type":
                        ChaskisEventSource sourceType;
                        if( Enum.TryParse( attr.Value, out sourceType ) )
                        {
                            e.SourceType = sourceType;
                        }
                        break;

                    case "source_plugin":
                        e.SourcePlugin = attr.Value;
                        break;

                    case "dest_plugin":
                        e.DestinationPlugin = attr.Value;
                        break;
                }
            }

            foreach( XmlNode node in rootNode.ChildNodes )
            {
                switch( node.Name )
                {
                    case "args":
                        foreach( XmlNode argNode in node.ChildNodes )
                        {
                            e.Args[argNode.Name] = argNode.InnerText;
                        }
                        break;

                    case "passthrough_args":
                        foreach( XmlNode argNode in node.ChildNodes )
                        {
                            e.PassThroughArgs[argNode.Name] = argNode.InnerText;
                        }
                        break;
                }
            }

            return e;
        }

        private ChaskisEvent()
        {
            this.SourceType = ChaskisEventSource.CORE;
            this.SourcePlugin = string.Empty;
            this.DestinationPlugin = string.Empty;
            this.Args = new Dictionary<string, string>();
            this.PassThroughArgs = new Dictionary<string, string>();
        }

        /// <summary>
        /// Constructor.
        /// Only classes inside of ChaskisCore can create instances of this class directly.
        /// All plugins must use <see cref="ChaskisEventFactory"/>
        /// </summary>
        /// <param name="sourcePlugin">The plugin that generated this event.</param>
        /// <param name="destinationPlugin">
        /// The plugin this event wishes to talk to.
        /// 
        /// Null for a "broadcast" (all plugins may listen).
        /// </param>
        internal ChaskisEvent(
            ChaskisEventSource sourceType,
            string sourcePlugin,
            string destinationPlugin,
            IDictionary<string, string> args,
            IDictionary<string, string> passThroughArgs = null
        )
        {
            this.SourceType = sourceType;
            this.SourcePlugin = sourcePlugin.ToUpper();
            if( destinationPlugin == null )
            {
                this.DestinationPlugin = BroadcastEventStr;
            }
            else
            {
                this.DestinationPlugin = destinationPlugin.ToUpper();
            }
            this.Args = args;
            this.PassThroughArgs = passThroughArgs;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Did this event come from the CORE or from a Plugin?
        /// </summary>
        public ChaskisEventSource SourceType { get; private set; }

        /// <summary>
        /// Which plugin created the event.
        /// </summary>
        public string SourcePlugin { get; private set; }

        /// <summary>
        /// Which plugin are we directing the event to?
        /// Empty string for a broadcast for all plugins.
        /// </summary>
        public string DestinationPlugin { get; private set; }

        /// <summary>
        /// Required arguments for the event.
        /// </summary>
        public IDictionary<string, string> Args { get; private set; }

        /// <summary>
        /// Arguments that are passed through directly to the response.
        /// </summary>
        public IDictionary<string, string> PassThroughArgs { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Creates the event string, which is <see cref="ToXml"/> in string format.
        /// </summary>
        public override string ToString()
        {
            using( StringWriter stringWriter = new StringWriter() )
            {
                XmlDocument doc = this.ToXml();
                using( XmlTextWriter writer = new XmlTextWriter( stringWriter ) )
                {
                    writer.Formatting = Formatting.None; // All one line.
                    doc.Save( writer );
                }

                return stringWriter.ToString();
            }
        }

        public XmlDocument ToXml()
        {
            XmlDocument doc = new XmlDocument();

            XmlNode rootNode = doc.DocumentElement;

            XmlNode chaskisNode = doc.CreateElement( XmlElementName );

            // Add Attributes to the chaskis node
            {
                {
                    XmlAttribute sourceType = doc.CreateAttribute( "source_type" );
                    sourceType.Value = this.SourceType.ToString();
                    chaskisNode.Attributes.Append( sourceType );
                }

                {
                    XmlAttribute sourcePlugin = doc.CreateAttribute( "source_plugin" );
                    sourcePlugin.Value = this.SourcePlugin;
                    chaskisNode.Attributes.Append( sourcePlugin );
                }

                {
                    XmlAttribute destinationPlugin = doc.CreateAttribute( "dest_plugin" );
                    destinationPlugin.Value = this.DestinationPlugin;
                    chaskisNode.Attributes.Append( destinationPlugin );
                }
            }

            // Add arguments
            {
                XmlElement argsNode = doc.CreateElement( "args" );

                if( this.Args != null )
                {
                    foreach( KeyValuePair<string, string> args in this.Args )
                    {
                        XmlElement argNode = doc.CreateElement( args.Key );
                        argNode.InnerText = args.Value.ToString();
                        argsNode.AppendChild( argNode );
                    }
                }

                chaskisNode.AppendChild( argsNode );
            }

            // Add pass-through args
            {
                XmlElement passThroughArgs = doc.CreateElement( "passthrough_args" );

                if( this.PassThroughArgs != null )
                {
                    foreach( KeyValuePair<string, string> args in this.PassThroughArgs )
                    {
                        XmlElement argNode = doc.CreateElement( args.Key );
                        argNode.InnerText = args.Value.ToString();
                        passThroughArgs.AppendChild( argNode );
                    }
                }

                chaskisNode.AppendChild( passThroughArgs );
            }

            doc.InsertBefore( chaskisNode, rootNode );

            return doc;
        }
    }
}
