//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Xml;

namespace Chaskis.Plugins.ServerDiagnostics
{
    /// <summary>
    /// Loads XML for the server diagnostics plugin.
    /// </summary>
    public static class XmlLoader
    {
        // -------- Fields --------

        /// <summary>
        /// What the root node of the xml config should be.
        /// </summary>
        private const string rootNodeName = "serverdiagnostics";

        // -------- Functions --------

        /// <summary>
        /// Loads the server diagnostics config from the
        /// given XML file.
        /// </summary>
        /// <param name="xmlFilePath">The path to the xml file.</param>
        /// <exception cref="XmlException">If the XML is not correct</exception>
        /// <returns>The server diagnostics config from the XML file.</returns>
        public static ServerDiagnosticsConfig LoadConfig( string xmlFilePath )
        {
            if( File.Exists( xmlFilePath ) == false )
            {
                throw new FileNotFoundException( "Could not find server diagnostics plugin config file " + xmlFilePath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( xmlFilePath );

            XmlElement rootNode = doc.DocumentElement;
            if( rootNode.Name != rootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + rootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            ServerDiagnosticsConfig config = new ServerDiagnosticsConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "utimecmd":
                        config.UpTimeCmd = childNode.InnerText;
                        break;

                    case "osverscmd":
                        config.OsVersionCmd = childNode.InnerText;
                        break;

                    case "proccountcmd":
                        config.ProcessorCountCmd = childNode.InnerText;
                        break;

                    case "timecmd":
                        config.TimeCmd = childNode.InnerText;
                        break;
                }
            }

            return config;
        }
    }
}