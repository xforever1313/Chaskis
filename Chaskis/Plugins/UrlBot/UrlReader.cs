//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SethCS.Basic;

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
        /// Max size is 1MB.
        /// </summary>
        private const int maxFileSize = 1 * 1000 * 1000;

        const string userAgent = "Chakis IRC Bot URL Plugin";

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
        public Task<UrlResponse> GetDescription( string url )
        {
            return Task<UrlResponse>.Run(
                delegate()
                {
                    UrlResponse response = new UrlResponse();

                    try
                    {
                        string webResponse;
                        using( WebClient client = new WebClient() )
                        {
                            client.Headers.Add( "user-agent", userAgent );
                            client.DownloadProgressChanged += delegate ( object sender, DownloadProgressChangedEventArgs e )
                            {
                                if( e.TotalBytesToReceive >= maxFileSize )
                                {
                                    client.CancelAsync();
                                    StaticLogger.WriteLine( "UrlReader> Request Cancelled due to size of {0}", e.TotalBytesToReceive );
                                }
                            };

                            Task<string> str = client.DownloadStringTaskAsync( url );

                            str.Wait();

                            webResponse = str.Result;
                        }

                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml( webResponse );

                        HtmlNode node = doc.DocumentNode.SelectSingleNode( "//title" );
                        if( node != null )
                        {
                            // Issue #15: Ensure we decode characters such as &lt; and &gt;
                            response.Title = WebUtility.HtmlDecode( node.InnerText );
                        }
                    }
                    catch( Exception e )
                    {
                        StaticLogger.ErrorWriteLine( "UrlReader> Error when getting response from {0}{1}{2}", url, Environment.NewLine, e.ToString() );
                    }

                    return response;
                }
            );
        }
    }
}