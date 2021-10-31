//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
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
        public static readonly string UrlRegex =
            @"(?<url>(http://|https://|ftp://)([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?)";

        private static readonly Regex urlRegex = new Regex( UrlRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled );

        /// <summary>
        /// Max size is 1MB.
        /// </summary>
        private const int maxFileSize = 1 * 1000 * 1000;

        private readonly GenericLogger logger;

        private static readonly string userAgent = $"Chaskis IRC Bot - {nameof( UrlBot )} Plugin";

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public UrlReader( GenericLogger logger )
        {
            this.logger = logger;
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
        public Task<UrlResponse> AsyncGetDescription( string url )
        {
            return Task.Run(
                async delegate
                {
                    UrlResponse response = new UrlResponse();

                    IFlurlRequest baseRequest = url
                        .WithHeader( "User-Agent", userAgent )
                        // Only grab html pages.
                        .WithHeader( "Accept", "text/html,application/xhtml" )
                        .WithAutoRedirect( true )
                        .WithTimeout( TimeSpan.FromSeconds( 15 ) );

                    try
                    {
                        IFlurlResponse getResponse = await baseRequest.GetAsync();

                        HttpResponseMessage responseMessage = getResponse.ResponseMessage;
                        if( responseMessage.IsSuccessStatusCode == false )
                        {
                            this.logger.WriteLine(
                                $"Got status code {responseMessage.StatusCode} from '{url}' - ignoring."
                            );
                            return response;
                        }

                        string mediaType = responseMessage?.Content?.Headers?.ContentType?.MediaType;
                        if( "text/html" != mediaType )
                        {
                            this.logger.WriteLine(
                                $"Got non-html media type {mediaType ?? "[null]"} for url '{url}' - ignoring."
                            );

                            return response;
                        }

                        StringBuilder builder = new StringBuilder();

                        // We only care about the head section of the HTML, not the body.
                        // Only read through the head section.
                        using( Stream stream = await getResponse.GetStreamAsync() )
                        {
                            using( StreamReader reader = new StreamReader( stream ) )
                            {
                                string line = reader.ReadLine();
                                while( line != null )
                                {
                                    builder.AppendLine( line );
                                    if( line.Contains( "</head>" ) )
                                    {
                                        line = null;
                                    }
                                    else
                                    {
                                        line = reader.ReadLine();
                                    }
                                }
                            }
                        }
                        builder.AppendLine( "</html>" );

                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml( builder.ToString() );

                        HtmlNode node = doc.DocumentNode.SelectSingleNode( "//title" );
                        if( node != null )
                        {
                            // Issue #15: Ensure we decode characters such as &lt; and &gt;
                            response.Title = WebUtility.HtmlDecode( node.InnerText );
                        }

                    }
                    catch( Exception e )
                    {
                        this.logger.ErrorWriteLine( "Error when getting response from {0}{1}{2}", url, Environment.NewLine, e.ToString() );
                    }

                    return response;
                }
            );
        }
    }
}