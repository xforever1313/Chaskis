//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Core
{
    /// <summary>
    /// Base class for all private message configurations.
    /// 
    /// Put any child-specific options in the child classes.  This simply contains
    /// things that are common for all configurations.
    /// </summary>
    /// <typeparam name="TChildType">The child class's type.</typeparam>
    /// <typeparam name="TLineActionType">The type for the line action.</typeparam>
    public abstract class BasePrivateMessageConfig<TChildType, TLineActionType, TLineActionArgs> : IPrivateMessageConfig 
        where TLineActionType : Delegate
        where TLineActionArgs : IPrivateMessageHandlerArgs
    {
        // ---------------- Fields ----------------

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.  Sets all properties to defaults
        /// specified in their comments.  To override the default,
        /// simply do so in the child class's constructor.
        /// </summary>
        protected BasePrivateMessageConfig()
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
        public TLineActionType LineAction { get; set; }

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
            List<string> errors = new List<string>();

            if ( string.IsNullOrEmpty( this.LineRegex ) )
            {
                errors.Add( nameof( this.LineRegex ) + " can not be null or empty." );
            }

            if ( this.LineAction == null )
            {
                errors.Add( nameof( this.LineAction ) + " can not be null." );
            }

            if ( this.CoolDown < 0 )
            {
                errors.Add( nameof( this.CoolDown ) + " can not be less than 0." );
            }

            IEnumerable<string> childErrors = this.ValidateChild();
            if ( childErrors != null )
            {
                errors.AddRange( childErrors );
            }

            if ( errors.IsEmpty() == false )
            {
                throw new ListedValidationException( "Errors when validating private message config", errors );
            }
        }

        public abstract TChildType Clone();

        /// <summary>
        /// Validate the child's properties, if any.
        /// </summary>
        /// <returns>
        /// A list of strings that are wrong with the child node.
        /// Return null or an empty list if nothing is wrong.
        /// </returns>
        protected abstract IEnumerable<string> ValidateChild();
    }
}
