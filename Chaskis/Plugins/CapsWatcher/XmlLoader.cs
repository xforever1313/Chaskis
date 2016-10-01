//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Xml;

namespace Chaskis.Plugins.CapsWatcher
{
    public static class XmlLoader
    {
        // -------- Fields --------

        /// <summary>
        /// What the root node of the xml config should be.
        /// </summary>
        private const string capsWatcherConfigRootNodeName = "capswatcherconfig";

        // -------- Functions --------

        /// <summary>
        /// Parses the given XML file to create a CowSayBotConfig and returns that.
        /// </summary>
        /// <param name="xmlFilePath">Path to the XML file.</param>
        /// <exception cref="XmlException">If the XML is not correct</exception>
        /// <exception cref="FormatException">If the XML can not convert the string to a number</exception>
        /// <exception cref="InvalidOperationException">If the configuration object is not valid.</exception>
        /// <returns>A cowsay bot config based on the given XML.</returns>
        public static CapsWatcherConfig LoadCapsWatcherConfig( string xmlFilePath )
        {
            if( File.Exists( xmlFilePath ) == false )
            {
                throw new FileNotFoundException( "Could not find caps watcher bot config Config file " + xmlFilePath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( xmlFilePath );

            XmlElement rootNode = doc.DocumentElement;
            if( rootNode.Name != capsWatcherConfigRootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + capsWatcherConfigRootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            CapsWatcherConfig config = new CapsWatcherConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "message":
                        config.Messages.Add( childNode.InnerText );
                        break;
                }
            }

            config.Validate();

            return config;
        }
    }
}