//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chaskis.Plugins.IsItDownBot
{
    /// <summary>
    /// The status of the given website.
    /// </summary>
    public enum WebsiteStatus
    {
        /// <summary>
        /// The website is online, and was online the last
        /// time we checked.
        /// </summary>
        StillOnline,

        /// <summary>
        /// The website was offline, but came online.
        /// </summary>
        CameOnline,

        /// <summary>
        /// The website was online, but it went offline.
        /// </summary>
        WentOffline,

        /// <summary>
        /// The website is offline, and was offline the last time we checked.
        /// </summary>
        StillOffline
    }

    /// <summary>
    /// Checks to see if a website is down or not.
    /// </summary>
    public class IsItDownChecker
    {
        // ---------------- Fields ----------------

        private readonly HttpClient httpClient;

        /// <summary>
        /// These are websites that are triggered automatically.
        /// We cache these values so we don't blow up the IRC chat if the status
        /// doesn't change.
        /// </summary>
        private readonly Dictionary<string, WebsiteStatus> cachedWebsites;

        // ---------------- Constructor ----------------

        public IsItDownChecker( HttpClient httpClient, IList<string> cachedUrls )
        {
            this.httpClient = httpClient;
            this.cachedWebsites = new Dictionary<string, WebsiteStatus>();

            foreach( string s in cachedUrls )
            {
                // Assume the website is online to start with.
                this.cachedWebsites.Add( s, WebsiteStatus.StillOnline );
            }
        }

        // ---------------- Properties ----------------

        // ---------------- Functions ----------------

        /// <summary>
        /// Gets the status of the given cached website.
        /// </summary>
        public WebsiteStatus GetCachedStatus( string url )
        {
            lock( this.cachedWebsites )
            {
                return this.cachedWebsites[url];
            }
        }

        /// <summary>
        /// Checks the given URL to see if the website is online.
        /// </summary>
        /// <param name="url">
        /// The url to check.  Note, if the url is part of our cached websites list,
        /// the cached URL's status will be updated.  If it is not part of the list,
        /// the cache is untouched.
        /// </param>
        public Tuple<WebsiteStatus, HttpStatusCode> CheckSite( string url )
        {
            HttpRequestMessage headRequest = new HttpRequestMessage( HttpMethod.Head, url );
            Task<HttpResponseMessage> headResponse = this.httpClient.SendAsync( headRequest );
            headResponse.Wait();

            bool online = headResponse.Result.IsSuccessStatusCode;
            HttpStatusCode code = headResponse.Result.StatusCode;

            WebsiteStatus status;
            lock( this.cachedWebsites )
            {
                if( this.cachedWebsites.ContainsKey( url ) )
                {
                    switch( this.cachedWebsites[url] )
                    {
                        case WebsiteStatus.StillOnline:
                            if( online )
                            {
                                // If our last state was "StillOnline",
                                // and we are still online, our return status
                                // is that we are still online, as nothing changed.
                                status = WebsiteStatus.StillOnline;
                            }
                            else
                            {
                                // If we went offline, we went from online -> offline.
                                // our return status should be the same.  Our cached
                                // status must also change.
                                status = WebsiteStatus.WentOffline;
                            }
                            break;

                        case WebsiteStatus.StillOffline:
                            if( online )
                            {
                                // If our last state was "StillOffline",
                                // but we went online, our return status is we went
                                // online.
                                status = WebsiteStatus.CameOnline;
                            }
                            else
                            {
                                // Otherwise, our status hasn't changed, we're still offline.
                                status = WebsiteStatus.StillOffline;
                            }
                            break;

                        case WebsiteStatus.CameOnline:
                            if( online )
                            {
                                // If our last status was "CameOnline",
                                // and we are still online, our new status is "StillOnline".
                                status = WebsiteStatus.StillOnline;
                            }
                            else
                            {
                                // Otherwise, we went offline.
                                status = WebsiteStatus.WentOffline;
                            }
                            break;

                        case WebsiteStatus.WentOffline:
                            if( online )
                            {
                                // If our last status was "WentOffline",
                                // and we went online, our new status is "CameOnline".
                                status = WebsiteStatus.CameOnline;
                            }
                            else
                            {
                                // Otherwise, our status must changed to "StillOffline".
                                status = WebsiteStatus.StillOffline;
                            }
                            break;

                        default:
                            // This should never happen.
                            throw new InvalidOperationException( "Found invalid " + nameof( WebsiteStatus ) + ": " + this.cachedWebsites[url] );
                    }

                    this.cachedWebsites[url] = status;
                }
                else
                {
                    status = online ? WebsiteStatus.StillOnline : WebsiteStatus.StillOffline;
                }
            }

            return new Tuple<WebsiteStatus, HttpStatusCode>( status, code );
        }

        public Task<Tuple<WebsiteStatus, HttpStatusCode>> AsyncCheckSite( string url )
        {
            return Task.Run( () => this.CheckSite( url ) );
        }
    }
}
