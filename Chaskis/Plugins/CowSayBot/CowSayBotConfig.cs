
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;

namespace Chaskis.Plugins.CowSayBot
{
    /// <summary>
    /// Configuration object for cowsay bot.
    /// </summary>
    public class CowSayBotConfig
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public CowSayBotConfig()
        {
            this.ListenRegex = @"^!{%saycmd%} (?<msg>.+)";
            this.ExeCommand = "/usr/bin/cowsay";
            this.CowFileInfoList = new List<CowFileInfo>();
        }

        // -------- Properties --------

        /// <summary>
        /// The regex to listen for to trigger cowsay bot to respond.
        /// {%channel%} is replaced with the current channel name,
        /// {%nick%} is replaced with the bot's nick name
        /// {%saycmd%} is replaced with all the valid say commands that trigger cowsay
        /// bot to respond (e.g. coway, tuxsay, etc).
        /// 
        /// Defaulted to ^!{%saycmd%} (?<msg>.+).  msg MUST be a group, or this will not validate.
        /// </summary>
        public string ListenRegex { get; set; }

        /// <summary>
        /// The executable command to launch cowsay.
        /// Defaulted to /usr/bin/cowsay.
        /// </summary>
        public string ExeCommand { get; set; }

        /// <summary>
        /// List of cowfiles to look at.  Can not be empty for this thing to validate.
        /// Also each CowFileInfo must validate as well.
        /// </summary>
        public IList<CowFileInfo> CowFileInfoList { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Ensures this object is valid.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this does not validate.</exception>
        public void Validate()
        {
            string errors = "Can not validate this CowSayBotConfig object:" + Environment.NewLine;
            bool success = true;

            if ( string.IsNullOrEmpty( this.ListenRegex ) || string.IsNullOrWhiteSpace( this.ListenRegex ) )
            {
                errors += nameof( this.ListenRegex ) + " can not be empty or null" + Environment.NewLine;
                success = false;
            }
            if ( string.IsNullOrEmpty( this.ExeCommand ) || string.IsNullOrWhiteSpace( this.ExeCommand ) )
            {
                errors += nameof( this.ExeCommand ) + " can not be empty or null" + Environment.NewLine;
                success = false;
            }
            else if ( File.Exists( this.ExeCommand ) == false )
            {
                errors += nameof( this.ExeCommand ) + " does not exist!" + Environment.NewLine;
                success = false;
            }

            if ( ( this.CowFileInfoList == null ) || ( this.CowFileInfoList.Count == 0 ) )
            {
                errors += nameof( this.CowFileInfoList ) + " can not be empty or null" + Environment.NewLine;
                success = false;
            }
            else
            {
                foreach ( CowFileInfo info in this.CowFileInfoList )
                {
                    try
                    {
                        info.Validate();
                    }
                    catch ( InvalidOperationException err )
                    {
                        errors += Environment.NewLine + err.Message + Environment.NewLine;
                        success = false;
                    }
                }
            }

            if ( success == false )
            {
                throw new InvalidOperationException( errors );
            }
        }

        /// <summary>
        /// Deep-copies this class.
        /// </summary>
        /// <returns>A deep copy of this object.</returns>
        public CowSayBotConfig Clone()
        {
            CowSayBotConfig clone = ( CowSayBotConfig ) this.MemberwiseClone();
            clone.CowFileInfoList = new List<CowFileInfo>( this.CowFileInfoList );
            return clone;
        }
    }
}
