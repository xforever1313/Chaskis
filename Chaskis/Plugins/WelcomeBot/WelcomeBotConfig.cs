//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;

namespace WelcomeBot
{
    public class WelcomeBotConfig : IEquatable<WelcomeBotConfig>
    {
        // ---------------- Constructor ----------------

        public WelcomeBotConfig()
        {
            this.EnableJoinMessages = true;
            this.EnablePartMessages = true;
            this.EnableKickMessages = true;
            this.KarmaBotIntegration = false;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Does the bot report to the channel when a user joins it?
        /// 
        /// Defaulted to true.
        /// </summary>
        public bool EnableJoinMessages { get; set; }

        /// <summary>
        /// Does the bot report to a channel when a user leaves (parts) it?
        /// 
        /// Defaulted to true.
        /// </summary>
        public bool EnablePartMessages { get; set; }

        /// <summary>
        /// Does the bot report to a channel when a user is kicked from it?
        /// 
        /// Defaulted to true.
        /// </summary>
        public bool EnableKickMessages { get; set; }

        /// <summary>
        /// Should WelcomeBot integrate with KarmaBot?
        /// Karmabot must be loaded, otherwise this option is ignored.
        /// 
        /// If this is set to true, the user's karma will be displayed when they join the channel if
        /// <see cref="EnableJoinMessages"/> is set to true.
        /// 
        /// Defaulted to false.
        /// </summary>
        public bool KarmaBotIntegration { get; set; }

        // ---------------- Functions ----------------

        public override bool Equals( object obj )
        {
            return this.Equals( obj as WelcomeBotConfig );
        }

        public bool Equals( WelcomeBotConfig other )
        {
            if( other == null )
            {
                return false;
            }

            return
                ( this.EnableJoinMessages == other.EnableJoinMessages ) &&
                ( this.EnableKickMessages == other.EnableKickMessages ) &&
                ( this.EnablePartMessages == other.EnablePartMessages ) &&
                ( this.KarmaBotIntegration == other.KarmaBotIntegration );
        }

        public override int GetHashCode()
        {
            return
                this.EnableJoinMessages.GetHashCode() +
                this.EnableKickMessages.GetHashCode() +
                this.EnablePartMessages.GetHashCode() +
                this.KarmaBotIntegration.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine( "Welcome Bot Config:" );
            builder.AppendLine( "\t- " + nameof( this.EnableJoinMessages ) + " " + this.EnableJoinMessages );
            builder.AppendLine( "\t- " + nameof( this.EnablePartMessages ) + " " + this.EnablePartMessages );
            builder.AppendLine( "\t- " + nameof( this.EnableKickMessages ) + " " + this.EnableKickMessages );

            return builder.ToString();
        }

        public void Validate()
        {
            // Nothing to validate...
        }

        public WelcomeBotConfig Clone()
        {
            return (WelcomeBotConfig)this.MemberwiseClone();
        }
    }
}
