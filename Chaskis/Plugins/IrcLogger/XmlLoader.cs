//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Chaskis.Plugins.IrcLogger
{
    public static class XmlLoader
    {
        /// <summary>
        /// Root node of the XML config.
        /// </summary>
        private const string ircloggerRootNodeName = "ircloggerconfig";

        /// <summary>
        /// Loads the irc logger config from the given xml file.
        /// </summary>
        /// <param name="xmlFilePath">The file name to load.</param>
        /// <returns>An Irc Logger config object from the given xml file.</returns>
        public static IrcLoggerConfig LoadIrcLoggerConfig( string xmlFilePath )
        {
            if( File.Exists( xmlFilePath ) == false )
            {
                throw new FileNotFoundException( "Could not find irc logger plugin config file " + xmlFilePath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( xmlFilePath );

            XmlElement rootNode = doc.DocumentElement;
            if( rootNode.Name != ircloggerRootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + ircloggerRootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            IrcLoggerConfig config = new IrcLoggerConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "logfilelocation":
                        config.LogFileLocation = childNode.InnerText;
                        break;

                    case "maxnumbermessagesperlog":
                        if( string.IsNullOrEmpty( childNode.InnerText ) == false )
                        {
                            config.MaxNumberMessagesPerLog = uint.Parse( childNode.InnerText );
                        }
                        // Else, empty string results in default value.
                        break;

                    case "logname":
                        config.LogName = childNode.InnerText;
                        break;

                    case "ignores":
                        foreach( XmlNode ignoreNode in childNode.ChildNodes )
                        {
                            switch( ignoreNode.Name )
                            {
                                case "ignore":
                                    if( string.IsNullOrEmpty( ignoreNode.InnerText ) == false )
                                    {
                                        config.IgnoreRegexes.Add( new Regex( ignoreNode.InnerText, RegexOptions.Compiled ) );
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

            return config;
        }
    }
}