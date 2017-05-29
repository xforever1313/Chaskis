//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Tests.Plugins.RssBot
{
    public static class RssBotTestHelpers
    {
        public const string TestUrl1 = "https://www.shendrick.net/atom.xml";

        public static readonly TimeSpan Interval1 = TimeSpan.FromMinutes( 60 );

        public const string TestUrl2 = "http://thenaterhood.com/feed.xml";

        public static readonly TimeSpan Interval2 = TimeSpan.FromMinutes( 30 );
    }
}
