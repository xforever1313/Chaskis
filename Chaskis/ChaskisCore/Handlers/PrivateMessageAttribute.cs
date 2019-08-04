//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    /// <summary>
    /// This attribute is used to flag on any handler classes that have 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class PrivateMessageAttribute : Attribute
    {
        // ---------------- Constructor ----------------

        public PrivateMessageAttribute( string messageRegex )
        {
            this.MessageRegex = new Regex(
                messageRegex,
                RegexOptions.ExplicitCapture | RegexOptions.Compiled
            );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Regex used to check to see if the attribute is of a certain type.
        /// </summary>
        /// <example>
        /// For the action private message type, this would be 0x01ACTION (?<message>.+)0x01.
        /// If we get a private message that matches this regex, <see cref="MessageHandler"/> will not fire,
        /// but ActionHandler will.
        /// </example>
        public Regex MessageRegex { get; private set; }
    }
}
