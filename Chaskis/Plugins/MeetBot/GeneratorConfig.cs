//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using SethCS.Basic;
using SethCS.Extensions;

namespace Chaskis.Plugins.MeetBot
{
    /// <summary>
    /// Configuration on how to generate meeting notes file.
    /// </summary>
    public class GeneratorConfig
    {
        // ---------------- Fields ----------------

        private readonly string meetbotRoot;

        private readonly string defaultTemplatesPath;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor; sets everything to default settings.
        /// </summary>
        /// <param name="meetbotRoot">Path to the meetbot plugin directory.</param>
        /// <param name="type">The generator type.</param>
        public GeneratorConfig( string meetbotRoot )
        {
            this.meetbotRoot = meetbotRoot;
            this.defaultTemplatesPath = Path.Combine( this.meetbotRoot, "templates" );

            this.Type = MeetingNotesGeneratorType.Unknown;
            this.Channels = new List<string>();
            this.Output = Path.Combine(
                this.meetbotRoot,
                "notes",
                "{%channel%}" // Will be substituted later for the channel name.
            );
            this.FileName = "{%meetingtopic%}-{%timestamp%}.{%generatortype%}";
            this.TimeStampUseUtc = true;
            this.TimeStampCulture = CultureInfo.InvariantCulture;
            this.TimeStampFormat = "yyyy-MM-dd_HH-mm-ss-ffff";
        }

        // ---------------- Properties ----------------

        public MeetingNotesGeneratorType Type { get; set; }

        /// <summary>
        /// The list of channels this generator is used in.
        /// If this list is empty, it is used in all channels unless
        /// another generator "claims" that channel.
        /// </summary>
        public IList<string> Channels { get; private set; }

        /// <summary>
        /// Path to the razor template to use.
        /// Not every generator requires this (e.g. XML).
        /// 
        /// Each generator type has its own default.
        /// If set to null, the default will be used.
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        /// Where to save the meeting notes.
        /// 
        /// Defaulted to {%meetbotroot%}/notes/{%channel%}
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// What to name the filename.
        /// 
        /// Defaulted to {%meetingtopic%}-{%timestamp%}.{%generatortype%}
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Should the timestamp of the filename use UTC?  Defaulted to true.
        /// </summary>
        public bool TimeStampUseUtc { get; set; }

        /// <summary>
        /// The timestamp's culture (what <see cref="DateTime.ToString(string, IFormatProvider)"/> returns
        /// may depend on this).
        /// 
        /// Defaulted to <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public CultureInfo TimeStampCulture { get; set; }

        /// <summary>
        /// Value passed into <see cref="DateTime.ToString(string, IFormatProvider)"/> when determing the timestamp.
        /// Defaulted to "yyyy-MM-dd_HH-mm-ss-ffff".
        /// </summary>
        public string TimeStampFormat { get; set; }

        /// <summary>
        /// The action to perform by calling a subprocess.  Leave null or empty
        /// to perform no action.
        /// </summary>
        public string PostSaveAction { get; set; }

        /// <summary>
        /// The message to send to the channel when the notes are saved.
        /// 
        /// Leave null or empty to send the default message.
        /// </summary>
        public string PostSaveMessage { get; set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Gets the default template if <see cref="TemplatePath"/> is null or empty,
        /// otherwise returns <see cref="TemplatePath"/>.
        /// </summary>
        public string GetTemplatePath()
        {
            if( string.IsNullOrWhiteSpace( this.TemplatePath ) )
            {
                if( this.Type == MeetingNotesGeneratorType.html )
                {
                    return Path.Combine( this.defaultTemplatesPath, "default.cshtml" );
                }
                else if( this.Type == MeetingNotesGeneratorType.txt )
                {
                    return Path.Combine( this.defaultTemplatesPath, "default.cstxt" );
                }
                else if( this.Type == MeetingNotesGeneratorType.Unknown )
                {
                    throw new InvalidOperationException(
                        $"{nameof( MeetingNotesGeneratorType.Unknown )} does not have a template!"
                    );
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return this.TemplatePath;
            }
        }

        /// <summary>
        /// Gets the desired filename based on the meeting info.
        /// </summary>
        public string GetFileName( IMeetingInfo meetingInfo )
        {
            return ReplaceWithMeetingInfo(
                this.FileName,
                meetingInfo
            ).NormalizeWhiteSpace( '_' );
        }

        /// <summary>
        /// Gets the absolute path to where the notes need to be written to.
        /// </summary>
        public string GetFullOutputPath( IMeetingInfo meetingInfo )
        {
            return Path.Combine(
                this.meetbotRoot,
                GetFileName( meetingInfo )
            );
        }

        /// <summary>
        /// Gets the post save action the bot needs to run as a subprocess.
        /// 
        /// Returns null if no action was specified.
        /// </summary>
        public string GetPostSaveAction( IMeetingInfo meetingInfo )
        {
            if( string.IsNullOrWhiteSpace( this.PostSaveAction ) )
            {
                return null;
            }

            return ReplaceAll( this.PostSaveAction, meetingInfo );
        }

        /// <summary>
        /// Gets the post save message the bot will send to the channel.
        /// 
        /// Returns null to use specifying to use a default message.
        /// </summary>
        public string GetPostSaveMessage( IMeetingInfo meetingInfo )
        {
            if( string.IsNullOrWhiteSpace( this.PostSaveMessage ) )
            {
                return null;
            }

            return ReplaceAll( this.PostSaveMessage, meetingInfo );
        }

        private string ReplaceWithMeetingInfo( string input, IMeetingInfo meetingInfo )
        {
            input = Regexes.ChannelConfigVariable.Replace( input, meetingInfo.Channel );
            input = Regexes.MeetingTopicConfigVariable.Replace( input, meetingInfo.MeetingTopic );
            input = Regexes.TimeStampConfigVariable.Replace(
                input,
                meetingInfo.StartTime.ToString( this.TimeStampFormat, this.TimeStampCulture )
            );
            input = Regexes.GeneratorTypeConfigVariable.Replace( input, this.Type.ToString() );

            return input;
        }

        private string ReplaceAll( string input, IMeetingInfo meetingInfo )
        {
            input = ReplaceWithMeetingInfo( input, meetingInfo );

            input = Regexes.FileNameConfigVariable.Replace( input, this.GetFileName( meetingInfo ) );
            input = Regexes.MeetBotRootConfigVariable.Replace( input, this.meetbotRoot );
            input = Regexes.FullFilePathConfigVariable.Replace( input, this.GetFullOutputPath( meetingInfo ) );

            return input;
        }

        /// <summary>
        /// Ensures this configuraiton is correct.
        /// </summary>
        /// <returns>
        /// List of validation errors with this generator.  Empty list for no errors.
        /// </returns>
        public IList<string> TryValidate()
        {
            List<string> errors = new List<string>();

            if( this.Type == MeetingNotesGeneratorType.Unknown )
            {
                errors.Add( $"{nameof( MeetingNotesGeneratorType )} can not be {MeetingNotesGeneratorType.Unknown}" );
            }

            foreach( string channel in Channels )
            {
                if( string.IsNullOrWhiteSpace( channel ) )
                {
                    errors.Add( $"Generator's channel can not be null, whitespace, or empty" );
                }
            }

            // XML does not require a template, all others do.
            if( ( this.Type != MeetingNotesGeneratorType.xml ) && ( this.Type != MeetingNotesGeneratorType.Unknown ) )
            {
                if( string.IsNullOrWhiteSpace( this.GetTemplatePath() ) )
                {
                    errors.Add( $"{nameof(TemplatePath)} can not be null, whitespace, or empty when the type is not {MeetingNotesGeneratorType.xml}" );
                }
                else if( File.Exists( this.TemplatePath ) == false )
                {
                    errors.Add( $"Can not find template at '{this.TemplatePath}" );
                }
            }

            if( string.IsNullOrWhiteSpace( this.Output ) )
            {
                errors.Add( $"{nameof( this.Output )} can not be null, whitespace, or empty" );
            }

            if( string.IsNullOrWhiteSpace( this.FileName ) )
            {
                errors.Add( $"{nameof( this.Output )} can not be null, whitespace, or empty" );
            }

            if( this.TimeStampCulture == null )
            {
                errors.Add( $"{nameof( this.TimeStampCulture )} can not be null" );
            }

            if( string.IsNullOrWhiteSpace( this.TimeStampFormat ) )
            {
                errors.Add( $"{nameof( this.TimeStampFormat )} can not be null, whitespace, or empty" );
            }
            else
            {
                try
                {
                    DateTime.Now.ToString( this.TimeStampFormat, this.TimeStampCulture );
                }
                catch( FormatException e )
                {
                    errors.Add( $"Invalid {nameof( TimeStampFormat )}: {e.Message}" );
                }
            }

            return errors;
        }
    }

    public static class GeneratorConfigExtensions
    {
        // ---------------- Fields ----------------

        internal const string XmlElementName = "generator";

        private const string meetingNotesTypeAttrName = "type";

        private const string channelElementName = "channel";
        private const string outputElementName = "output";
        private const string templateElementName = "templatepath";
        private const string fileNameElementName = "filename";
        private const string timestampElementName = "timestamp";
        private const string timestampUtcAttrName = "utc";
        private const string timestampCultureAttrName = "culture";
        private const string fileNameNameElementName = "name";
        private const string postSaveActionElementName = "postsaveaction";
        private const string postSaveMsgElementName = "postsavemsg";

        // ---------------- Functions ----------------

        public static void FromXml( this GeneratorConfig config, XElement configElement, GenericLogger logger = null )
        {
            if( XmlElementName.Equals( configElement.Name.LocalName ) == false )
            {
                throw new ArgumentException(
                    $"Invalid XML element name.  Expected: {XmlElementName}, Got: {configElement.Name.LocalName}"
                );
            }

            foreach( XAttribute attr in configElement.Attributes() )
            {
                if( meetingNotesTypeAttrName.Equals( attr.Name.LocalName ) )
                {
                    if( Enum.TryParse( attr.Value, out MeetingNotesGeneratorType type ) )
                    {
                        config.Type = type;
                    }
                    else
                    {
                        config.Type = MeetingNotesGeneratorType.Unknown;
                    }
                }
                else
                {
                    logger?.WarningWriteLine( $"Unknown attribute '{attr.Name}' in '{configElement.Name.LocalName}'" );
                }
            }

            foreach( XElement childElement in configElement.Elements() )
            {
                if( channelElementName.Equals( childElement.Name.LocalName ) )
                {
                    config.Channels.Add( childElement.Value.Trim() );
                }
                else if( templateElementName.Equals( childElement.Name.LocalName ) )
                {
                    config.TemplatePath = childElement.Value.Trim();
                }
                else if( outputElementName.Equals( childElement.Name.LocalName ) )
                {
                    config.Output = childElement.Value.Trim();
                }
                else if( fileNameElementName.Equals( childElement.Name.LocalName ) )
                {
                    foreach( XElement fileElement in childElement.Elements() )
                    {
                        // Parse timestamp info.
                        if( timestampElementName.Equals( fileElement.Name.LocalName ) )
                        {
                            config.TimeStampFormat = fileElement.Value.Trim();
                            foreach( XAttribute timestampAttr in fileElement.Attributes() )
                            {
                                if( timestampUtcAttrName.Equals( timestampAttr.Name.LocalName ) )
                                {
                                    config.TimeStampUseUtc = bool.Parse( timestampAttr.Value.Trim() );
                                }
                                else if( timestampCultureAttrName.Equals( timestampAttr.Name.LocalName ) )
                                {
                                    config.TimeStampCulture = new CultureInfo( timestampAttr.Value.Trim() );
                                }
                                else
                                {
                                    logger?.WarningWriteLine(
                                        $"Unknown attribute '{timestampAttr.Name}' in attribute '{timestampAttr.Name.LocalName}'"
                                    );
                                }
                            }
                        }
                        else if( fileNameNameElementName.Equals( fileElement.Name.LocalName ) )
                        {
                            config.FileName = fileElement.Value.Trim();
                        }
                    }
                }
                else if( postSaveActionElementName.Equals( childElement.Name.LocalName ) )
                {
                    config.PostSaveAction = childElement.Value.Trim();
                }
                else if( postSaveMsgElementName.Equals( childElement.Name.LocalName ) )
                {
                    config.PostSaveMessage = childElement.Value.Trim();
                }
            }
        }
    }
}
