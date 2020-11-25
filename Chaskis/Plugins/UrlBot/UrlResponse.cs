//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;

namespace Chaskis.Plugins.UrlBot
{
    public class UrlResponse
    {
        public static readonly int MaxTitleLength = IrcConnection.MaximumLength;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The web-page title.</param>
        public UrlResponse()
        {
            this.Title = string.Empty;
        }

        /// <summary>
        /// Title of the webpage.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A title that is shortened so we don't overflow IRC.
        /// 
        /// We'll shorten at our IRC bot's maximum length-3 characters (seems reasonable).
        /// 
        /// Also, removes new lines.
        /// </summary>
        public string TitleShortened
        {
            get
            {
                string title;
                if( this.Title.Length > MaxTitleLength )
                {
                    string shortened = this.Title.Substring( 0, MaxTitleLength - 3 ) + "...";
                    title = shortened;
                }
                else
                {
                    title = this.Title;
                }

                // Replace new lines with spaces (Don't want to send several messages)
                title = title.Replace( '\n', ' ' );
                title = title.Replace( '\r', ' ' );

                return title;
            }
        }

        /// <summary>
        /// Whether or not the webpage is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return ( string.IsNullOrEmpty( this.Title ) == false ) && ( string.IsNullOrWhiteSpace( this.Title ) == false );
            }
        }
    }
}
