//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SethCS.Basic;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Plugins.MeetBot
{
    public class MeetBotConfig
    {
        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.  Sets everything to default settings.
        /// </summary>
        public MeetBotConfig( string meetbotRoot )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( meetbotRoot, nameof( meetbotRoot ) );
            this.MeetBotRoot = meetbotRoot;

            this.CommandConfigPath = null;
            this.EnableBackups = true;
            this.Generators = new List<GeneratorConfig>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Absolute path to the meet bot plugin root directory.
        /// </summary>
        public string MeetBotRoot { get; private set; }

        /// <summary>
        /// Path to the command file class.
        /// 
        /// Defaulted to null.  If set to null, the commands compiled in shall be used.
        /// </summary>
        public string CommandConfigPath { get; set; }

        /// <summary>
        /// If set to true, if at least one generator fails, an XML generator will be used
        /// to save a backup of the meeting notes.
        /// 
        /// Defaulted to true.
        /// </summary>
        public bool EnableBackups { get; set; }

        /// <summary>
        /// List of meeting note generator configurations.
        /// If none are specified, no meeting notes will be generated.
        /// </summary>
        public IList<GeneratorConfig> Generators { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Gets the command config path
        /// with any variables subsituted.
        /// </summary>
        /// <returns>Null means only use the default.  Otherwise, the path to the file.</returns>
        public string GetCommandConfigPath()
        {
            if( this.CommandConfigPath == null )
            {
                return null;
            }

            return Regexes.MeetBotRootConfigVariable.Replace( this.CommandConfigPath, this.MeetBotRoot );
        }

        /// <summary>
        /// Validates all configuration.  If validation fails, a <see cref="ListedValidationException"/>
        /// is thrown.
        /// </summary>
        public void Validate()
        {
            List<string> errors = new List<string>();

            foreach( GeneratorConfig config in this.Generators )
            {
                if( config == null )
                {
                    errors.Add( $"{nameof( GeneratorConfig )} can not be null" );
                }
                else 
                {
                    errors.AddRange( config.TryValidate() );
                }
            }

            if( errors.IsEmpty() == false )
            {
                throw new ListedValidationException(
                    $"Errors when validating {nameof( MeetBotConfig )}",
                    errors
                );
            }
        }
    }

    public static class MeetBotConfigExtensions
    {
        // ---------------- Fields ----------------

        internal const string XmlElementName = "meetbotconfig";

        private const string commandConfigElementName = "commandconfig";
        private const string enableBackupsElementName = "enablebackups";
        private const string generatorsElementName = "generators";

        // ---------------- Functions ----------------

        public static void FromXml( this MeetBotConfig config, XElement configElement, GenericLogger logger = null )
        {
            if( XmlElementName.Equals( configElement.Name.LocalName ) == false )
            {
                throw new ArgumentException(
                    $"Invalid XML element name.  Expected: {XmlElementName}, Got: {configElement.Name.LocalName}"
                );
            }

            foreach( XElement childElement in configElement.Elements() )
            {
                if( commandConfigElementName.Equals( childElement.Name.LocalName ) )
                {
                    config.CommandConfigPath = childElement.Value.Trim();
                }
                else if( enableBackupsElementName.Equals( childElement.Name.LocalName ) )
                {
                    config.EnableBackups = bool.Parse( childElement.Value.Trim() );
                }
                // Parse Generators:
                else if( generatorsElementName.Equals( childElement.Name.LocalName ) )
                {
                    foreach( XElement generatorElement in childElement.Elements() )
                    {
                        if( GeneratorConfigExtensions.XmlElementName.Equals( generatorElement.Name.LocalName ) )
                        {
                            GeneratorConfig generatorConfig = new GeneratorConfig( config.MeetBotRoot );
                            generatorConfig.FromXml( generatorElement, logger );
                            config.Generators.Add( generatorConfig );
                        }
                        else
                        {
                            logger?.WarningWriteLine(
                                $"Unknown XML element '{generatorElement.Name.LocalName} under '{childElement.Name.LocalName}'"
                            );
                        }
                    }
                }
                else
                {
                    logger?.WarningWriteLine(
                        $"Unknown XML element '{childElement.Name.LocalName} under '{configElement.Name.LocalName}'"
                    );
                }
            }
        }
    }
}
