//
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;

namespace Chaskis.Plugins.UrlBot
{
    /// <summary>
    /// This class is the class that reads the URL,
    /// parses the html for the description meta tag,
    /// and then returns the description as a string.
    /// 
    /// Maybe someday we'll add cacheing too...
    /// </summary>
    public class UrlReader
    {
        // -------- Fields --------

        /// <summary>
        /// Regex for a URL.
        /// Taken from (and slightly tweaked) from: https://weblogs.asp.net/farazshahkhan/regex-to-find-url-within-text-and-make-them-as-link
        /// </summary>
        public const string UrlRegex =
            @"(?<url>(http://|https://|ftp://|file:///)([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?)";

        private static readonly Regex urlRegex = new Regex( UrlRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled );

        /// <summary>
        /// Regex for finding the description meta tag.
        /// Yeah, Yeah, using a regex for HTML is bad, blah, blah blah.
        /// At some point we should use an HTML parser, but we'll see what happens...
        /// The less depencencies the better.
        /// </summary>
        public const string MetaDescriptionRegex = 
            @"\<\s*meta\s+name\s*=\s*""description""\s+content\s*=\s*""(?<description>[^\<\>]+)""\s*/?>";

        private static readonly Regex metaRegex = new Regex( MetaDescriptionRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline );

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public UrlReader()
        {
        }

        // -------- Properties --------

        // -------- Functions --------

        /// <summary>
        /// Tries to find the URL from the given string.
        /// </summary>
        /// <param name="stringToParse">The string to parse</param>
        /// <param name="url">The found URL as a string.</param>
        /// <returns><c>true</c>, if we found a URL, <c>false</c> otherwise.</returns>
        public static bool TryParseUrl( string stringToParse, out string url )
        {
            Match match = urlRegex.Match( stringToParse );
            if( match.Success == false )
            {
                url = string.Empty;
                return false;
            }

            url = match.Groups["url"].Value;
            return true;
        }

        /// <summary>
        /// Gets the description of the given URL in a background thread.
        /// </summary>
        /// <param name="url">URL to grab.</param>
        /// <returns>The description from the website's meta tag.</returns>
        public Task<string> GetDescription( string url )
        {
            return Task<string>.Run(
                delegate()
                {
                    try
                    {
                        string webResponse;
                        using( WebClient client = new WebClient() )
                        {
                            webResponse = client.DownloadString( url );
                        }

                        Match match = metaRegex.Match( webResponse );
                        if ( match.Success )
                        {
                            return match.Groups["description"].Value;
                        }

                        return "Can not find description tag :(";
                    }
                    catch( Exception e )
                    {
                        return "Error getting site information: " + e.Message;
                    }
                }
            );
        }
    }
}

