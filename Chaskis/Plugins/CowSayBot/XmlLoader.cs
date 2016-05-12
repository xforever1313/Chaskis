
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Xml;

namespace Chaskis.Plugins.CowSayBot
{
    public static class XmlLoader
    {
        // -------- Fields --------

        /// <summary>
        /// What the root node of the xml config should be.
        /// </summary>
        private const string cowSayConfigRootNodeName = "cowsaybotconfig";

        // -------- Constructor --------

        /// <summary>
        /// Parses the given XML file to create a CowSayBotConfig and returns that.
        /// </summary>
        /// <param name="xmlFilePath">Path to the XML file.</param>
        /// <exception cref="XmlException">If the XML is not correct</exception>
        /// <exception cref="FormatException">If the XML can not convert the string to a number</exception>
        /// <exception cref="InvalidOperationException">If the configuration object is not valid.</exception>
        /// <returns>A cowsay bot config based on the given XML.</returns>
        public static CowSayBotConfig LoadCowSayBotConfig( string xmlFilePath )
        {
            if ( File.Exists( xmlFilePath ) == false )
            {
                throw new FileNotFoundException( "Could not find cowsay bot config Config file " + xmlFilePath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( xmlFilePath );

            XmlElement rootNode = doc.DocumentElement;
            if ( rootNode.Name != cowSayConfigRootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + cowSayConfigRootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            CowSayBotConfig config = new CowSayBotConfig();

            foreach ( XmlNode childNode in rootNode.ChildNodes )
            {
                switch ( childNode.Name )
                {
                    case "command":
                        config.ListenRegex = childNode.InnerText;
                        break;

                    case "path":
                        config.ExeCommand = childNode.InnerText;
                        break;

                    case "cowsaycooldown":
                        config.CoolDownTimeSeconds = uint.Parse( childNode.InnerText );
                        break;

                    case "cowfiles":
                        foreach ( XmlNode cowFileNode in childNode.ChildNodes )
                        {
                            if ( cowFileNode.Name == "cowfile" )
                            {
                                string name = string.Empty;
                                string command = string.Empty;

                                foreach ( XmlAttribute attribute in cowFileNode.Attributes )
                                {
                                    switch ( attribute.Name )
                                    {
                                        case "name":
                                            name = attribute.Value;
                                            break;

                                        case "command":
                                            command = attribute.Value;
                                            break;
                                    }
                                }

                                config.CowFileInfoList.CommandList[command] = name;
                            }
                        }
                        break;
                }
            }

            config.Validate();

            return config;
        }
    }
}
