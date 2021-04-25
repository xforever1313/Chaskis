//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using SethCS.Exceptions;

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
            this.ListenRegex = @"!{%saycmd%} (?<msg>.+)";
            this.ExeCommand = "/usr/bin/cowsay";
            this.CowFileInfoList = new CowFileInfo();
            this.CoolDownTimeSeconds = 5;
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
        public CowFileInfo CowFileInfoList { get; private set; }

        /// <summary>
        /// How long to wait between running cowsay again in seconds.  Prevents flooding the IRC
        /// channel.This means that if someone runs !cowsay and someone else runs !cowsay before
        /// this time has passed, cowsay will run the first time, but not the second time.
        /// Defaulted to 5 seconds.
        /// </summary>
        public uint CoolDownTimeSeconds { get; set; }

        // -------- Functions --------

        /// <summary>
        /// Ensures this object is valid.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when this does not validate.</exception>
        public void Validate()
        {
            string errors = "Can not validate this CowSayBotConfig object:" + Environment.NewLine;
            bool success = true;

            if( string.IsNullOrEmpty( this.ListenRegex ) || string.IsNullOrWhiteSpace( this.ListenRegex ) )
            {
                errors += nameof( this.ListenRegex ) + " can not be empty or null" + Environment.NewLine;
                success = false;
            }
            if( string.IsNullOrEmpty( this.ExeCommand ) || string.IsNullOrWhiteSpace( this.ExeCommand ) )
            {
                errors += nameof( this.ExeCommand ) + " can not be empty or null" + Environment.NewLine;
                success = false;
            }
            else if( File.Exists( this.ExeCommand ) == false )
            {
                errors += nameof( this.ExeCommand ) + " does not exist!" + Environment.NewLine;
                success = false;
            }

            if( this.CowFileInfoList == null )
            {
                errors += nameof( this.CowFileInfoList ) + " can not be null" + Environment.NewLine;
                success = false;
            }
            else
            {
                try
                {
                    this.CowFileInfoList.Validate();
                }
                catch( Exception err )
                {
                    success = false;
                    errors += err.Message + Environment.NewLine;
                }
            }

            if( success == false )
            {
                throw new ValidationException( errors );
            }
        }

        /// <summary>
        /// Deep-copies this class.
        /// </summary>
        /// <returns>A deep copy of this object.</returns>
        public CowSayBotConfig Clone()
        {
            CowSayBotConfig clone = (CowSayBotConfig)this.MemberwiseClone();
            clone.CowFileInfoList = this.CowFileInfoList.Clone();
            return clone;
        }
    }
}