//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Cake.Core;
using Cake.FileHelpers;

namespace DevOps.Template
{
    public class Templatizer
    {
        // ---------------- Fields ----------------

        private readonly TemplateConstants constants;

        private readonly FilesToTemplate files;

        private readonly ICakeContext context;

        // ---------------- Constructor ----------------

        public Templatizer( TemplateConstants constants, FilesToTemplate filesToTemplate )
        {
            this.constants = constants;
            this.files = filesToTemplate;
            this.context = this.constants.CakeContext;
        }

        // ---------------- Functions ----------------

        public void Template()
        {
            foreach( FileToTemplate file in this.files.TemplateFiles )
            {
                string contents;
                if( file.Defines.Count == 0 )
                {
                    // If we have no defines, just grab the whole file.
                    contents = this.DoTemplate( this.context.FileReadText( file.Input ) );
                }
                else
                {
                    // Otherwise, we need to be smart.   If something is defined,
                    // include the text between the #IF and the #ENDIF.  Otherwise, move on.
                    // This is bascially a terrible version of the C PreProcessor since WIX doesn't
                    // like extra attributes.
                    // This crappy version doesn't currently support nested if statements.
                    List<Regex> ifRegexes = new List<Regex>();
                    foreach( string define in file.Defines )
                    {
                        ifRegexes.Add( new Regex( @"\s*#IF\s+" + define, RegexOptions.IgnoreCase ) );
                    }

                    Regex badRegex = new Regex( @"\s*#IF\s+\w+", RegexOptions.IgnoreCase );
                    Regex endRegex = new Regex( @"\s*#ENDIF", RegexOptions.IgnoreCase );

                    bool addLine = true;
                    StringBuilder builder = new StringBuilder();
                    foreach( string line in this.context.FileReadLines( file.Input ) )
                    {
                        bool foundIfRegex = false;
                        foreach( Regex ifRegex in ifRegexes )
                        {
                            if( ifRegex.IsMatch( line ) )
                            {
                                foundIfRegex = true;
                                break;
                            }
                        }

                        if( foundIfRegex )
                        {
                            addLine = true;
                            continue;
                        }
                        else if( badRegex.IsMatch( line ) )
                        {
                            addLine = false;
                            continue;
                        }
                        else if( endRegex.IsMatch( line ) )
                        {
                            addLine = true;
                            continue;
                        }
                        else if( addLine )
                        {
                            builder.AppendLine( line );
                        }
                    }
                    contents = this.DoTemplate( builder.ToString() );
                }

                if( file.LineEnding != null )
                {
                    contents = Regex.Replace( contents, Environment.NewLine, file.LineEnding );
                }

                this.context.FileWriteText( file.Output, contents );
            }
        }

        private string DoTemplate( string contents )
        {
            contents = Regex.Replace( contents, @"{%FullName%}", this.constants.FullName );
            contents = Regex.Replace( contents, @"{%ChaskisMainVersion%}", this.constants.ChaskisVersion );
            contents = Regex.Replace( contents, @"{%ChaskisCoreVersion%}", this.constants.ChaskisCoreVersion );
            contents = Regex.Replace( contents, @"{%License%}", this.constants.License );
            contents = Regex.Replace( contents, @"{%RegressionTestPluginVersion%}", this.constants.RegressionTestPluginVersion );
            contents = Regex.Replace( contents, @"{%RegressionTestPluginName%}", this.constants.RegressionTestPluginName );
            contents = Regex.Replace( contents, @"{%DefaultPluginName%}", this.constants.DefaultPluginName );
            contents = Regex.Replace( contents, @"{%CoreTags%}", this.constants.CoreTags );
            contents = Regex.Replace( contents, @"{%MainTags%}", this.constants.CliTags );
            contents = Regex.Replace( contents, @"{%Author%}", this.constants.Author );
            contents = Regex.Replace( contents, @"{%AuthorEmail%}", this.constants.AuthorEmail );
            contents = Regex.Replace( contents, @"{%ProjectUrl%}", this.constants.ProjectUrl );
            contents = Regex.Replace( contents, @"{%LicenseUrl%}", this.constants.LicenseUrl );
            contents = Regex.Replace( contents, @"{%WikiUrl%}", this.constants.WikiUrl );
            contents = Regex.Replace( contents, @"{%IssueTrackerUrl%}", this.constants.IssueTrackerUrl );
            contents = Regex.Replace( contents, @"{%CopyRight%}", this.constants.CopyRight );
            contents = Regex.Replace( contents, @"{%Description%}", this.constants.Description );
            contents = Regex.Replace( contents, @"{%ReleaseNotes%}", this.constants.ReleaseNotes );
            contents = Regex.Replace( contents, @"{%Summary%}", this.constants.Summary );
            contents = Regex.Replace( contents, @"{%IconUrl%}", this.constants.IconUrl );
            contents = Regex.Replace( contents, @"{%RunTime%}", this.constants.RunTime );

            return contents;
        }
    }
}
