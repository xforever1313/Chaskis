//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Net.Http;

namespace Chaskis.UnitTests.PluginTests
{
    public class PluginTestHelpers
    {
        static PluginTestHelpers()
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add( "User-Agent", "Chaskis IRC Bot Unit Tests" );
        }

        // ---------------- Properties ----------------

        public static HttpClient HttpClient { get; private set; }
    }
}
