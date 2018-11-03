//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using NetRunner.ExternalLibrary;
using SethCS.Basic;

namespace Chaskis.RegressionTests
{
    public class TestHttpClient : BaseTestContainer
    {
        // ----------------- Fields -----------------

        /// <summary>
        /// Static since we only want one client per process.
        /// </summary>
        private static readonly HttpClient client;

        private readonly GenericLogger httpClientLogger;

        // ----------------- Constructor -----------------

        public TestHttpClient()
        {
            this.httpClientLogger = Logger.GetLogFromContext( "http_client" );
        }

        static TestHttpClient()
        {
            client = new HttpClient();
        }

        // ----------------- Functions -----------------

        public bool SendPostRequestToWithQueryExpect( string url, string queryString, HttpStatusCode statusCode )
        {
            NameValueCollection query = HttpUtility.ParseQueryString( queryString );

            Dictionary<string, string> args = new Dictionary<string, string>();
            foreach( string key in query.AllKeys )
            {
                args[key] = query[key];
            }
            FormUrlEncodedContent content = new FormUrlEncodedContent( args );

            this.httpClientLogger.WriteLine( "Setting POST request to '{0}' with query '{1}'", url.ToString(), queryString );

            HttpResponseMessage message = client.PostAsync( url, content ).Result;

            string responseMsg = message.Content.ReadAsStringAsync().Result;

            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine( "Response from '" + url.ToString() + "':" );
            logMessage.AppendLine( "\t- Code: " + message.StatusCode + "; expected: " + statusCode );
            logMessage.AppendLine( "\t- Response: " + responseMsg );

            this.httpClientLogger.WriteLine( logMessage.ToString() );

            return message.StatusCode == statusCode;
        }
    }
}
