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
    /// <summary>
    /// This class represents an event that can be sent
    /// to all plugins, without the need of plugins
    /// to be aware of each other.
    /// </summary>
    public class InterPluginEvent
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// String for a broadcast event.
        /// </summary>
        public static readonly string BroadcastEventStr = "BCAST";

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// Only classes inside of ChaskisCore can create instances of this class directly.
        /// All plugins must use <see cref="InterPluginEventFactory"/>
        /// </summary>
        /// <param name="sourcePlugin">The plugin that generated this event.</param>
        /// <param name="destinationPlugin">
        /// The plugin this event wishes to talk to.
        /// 
        /// Null or empty string for a "broadcast" (all plugins may listen).
        /// </param>
        internal InterPluginEvent(
            string sourcePlugin,
            string destinationPlugin,
            IReadOnlyDictionary<string, string> args,
            IReadOnlyDictionary<string, string> passThroughArgs = null
        )
        {
            this.SourcePlugin = sourcePlugin.ToUpper();
            if( string.IsNullOrWhiteSpace( destinationPlugin ) )
            {
                this.DestinationPlugin = string.Empty;
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
        public IReadOnlyDictionary<string, string> Args { get; private set; }

        /// <summary>
        /// Arguments that are passed through directly to the response.
        /// </summary>
        public IReadOnlyDictionary<string, string> PassThroughArgs { get; private set; }

        public override string ToString()
        {
            return this.ToXml();
        }
    }

    internal static class InterPluginEventExtensions
    {
        internal const string XmlRootName = "interplugin_event";

        internal static InterPluginEvent FromXml( string xmlString )
        {
            string source = string.Empty;
            string destination = string.Empty;
            Dictionary<string, string> args = new Dictionary<string, string>();
            Dictionary<string, string> passThrough = new Dictionary<string, string>();

            XElement root = XElement.Parse( xmlString );

            if( XmlRootName.EqualsIgnoreCase( root.Name.LocalName ) == false )
            {
                throw new ValidationException(
                    $"Invalid XML root name: {XmlRootName} for {nameof( InterPluginEvent )}"
                );
            }

            foreach( XAttribute attr in root.Attributes() )
            {
                if( "source".EqualsIgnoreCase( attr.Name.LocalName ) )
                {
                    source = attr.Value;
                }
                else if( "destination".EqualsIgnoreCase( attr.Name.LocalName ) )
                {
                    destination = attr.Value;
                }
            }

            foreach( XElement child in root.Elements() )
            {
                if( "args".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    foreach( XElement arg in child.Elements() )
                    {
                        args[arg.Name.LocalName] = arg.Value;
                    }
                }
                else if( "passargs".EqualsIgnoreCase( child.Name.LocalName ) )
                {
                    foreach( XElement arg in child.Elements() )
                    {
                        passThrough[arg.Name.LocalName] = arg.Value;
                    }
                }
            }

            return new InterPluginEvent(
                source,
                destination,
                args,
                passThrough
            );
        }

        internal static string ToXml( this InterPluginEvent e )
        {
            XElement root = new XElement( XmlRootName );

            {
                XElement args = new XElement( "args" );
                foreach( KeyValuePair<string, string> arg in e.Args )
                {
                    args.Add( new XElement( arg.Key, arg.Value ) );
                }

                root.Add( args );
            }
            
            // No need to waste time or bytes if there are not pass through args.
            if( e.PassThroughArgs != null )
            {
                XElement passThrough = new XElement( "passargs" );
                foreach( KeyValuePair<string, string> arg in e.PassThroughArgs )
                {
                    passThrough.Add( new XElement( arg.Key, arg.Value ) );
                }
                root.Add( passThrough );
            }

            root.Add(
                new XAttribute( "source", e.SourcePlugin )
            );

            root.Add(
                new XAttribute( "destination", e.DestinationPlugin )
            );

            return root.ToString( SaveOptions.DisableFormatting );
        }
    }
}
