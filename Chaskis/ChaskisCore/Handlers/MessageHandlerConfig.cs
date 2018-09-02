//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public class MessageHandlerConfig
    {
        // ---------------- Constructor ----------------

        public MessageHandlerConfig()
        {
            this.RegexOptions = RegexOptions.None;
            this.CoolDown = 0;
            this.ResponseOption = ResponseOptions.ChannelAndPms;
            this.RespondToSelf = false;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The regex to search for in order to fire the action.
        /// For example, if you want !bot help to trigger the action, pass in "!bot\s+help"
        /// 
        /// This DOES get Liquified via <see cref="Parsing.LiquefyStringWithIrcConfig(string, string, string, string)'"/>
        /// 
        /// Required.
        /// </summary>
        public string LineRegex { get; set; }

        /// <summary>
        /// What regex options to use with <see cref="LineRegex"/>.
        /// Defaulted to <see cref="RegexOptions.None"/>
        /// </summary>
        public RegexOptions RegexOptions { get; set; }

        /// <summary>
        /// The action that gets triggered when the line regex matches.
        /// 
        /// Required.
        /// </summary>
        public MessageHandlerAction LineAction { get; set; }

        /// <summary>
        /// How long to wait in seconds between firing events. 0 for no cool down.
        /// This cool down is on a per-channel basis if the bot is in multiple channels.
        /// 
        /// Defaulted to 0.
        /// </summary>
        public int CoolDown { get; set; }

        /// <summary>
        /// Whether or not this bot will respond to private messages or not.
        /// 
        /// Defaulted to <see cref="ResponseOptions.ChannelAndPms"/>
        /// </summary>
        public ResponseOptions ResponseOption { get; set; }

        /// <summary>
        /// Whether or not the action will be triggered if the person
        /// who sent the message was this bot.
        /// 
        /// Defaulted to false.
        /// </summary>
        public bool RespondToSelf { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();
            errorString.AppendLine( "Errors when validating " + nameof( MessageHandlerConfig ) );

            if( string.IsNullOrEmpty( this.LineRegex ) )
            {
                success = false;
                errorString.AppendLine( "\t- " + nameof( this.LineRegex ) + " can not be null or empty." );
            }

            if( this.LineAction == null )
            {
                success = false;
                errorString.AppendLine( "\t- " + nameof( this.LineAction ) + " can not be null." );
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }

        public MessageHandlerConfig Clone()
        {
            return (MessageHandlerConfig)this.MemberwiseClone();
        }
    }
}
