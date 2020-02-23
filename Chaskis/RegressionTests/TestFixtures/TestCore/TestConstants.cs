//
//          Copyright Seth Hendrick 2017-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaskis.RegressionTests.TestCore
{
    public static class TestConstants
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Default timeout: 8 seconds.
        /// </summary>
        internal const int DefaultTimeout = 8 * 1000;

        /// <summary>
        /// Username that has admin capability to the bot.
        /// </summary>
        internal const string AdminUserName = "adminuser";

        /// <summary>
        /// Username that does NOT have admin capability to the bot.
        /// </summary>
        internal const string NormalUser = "nonadminuser";

        /// <summary>
        /// Name of the bot.
        /// </summary>
        internal const string BotName = "chaskisbot";

        /// <summary>
        /// Name of the first channel the bot is in.
        /// </summary>
        internal const string Channel1 = "#chaskistest";

        /// <summary>
        /// Name of the second channel the bot is in.
        /// </summary>
        internal const string Channel2 = "#chaskistest2";

        /// <summary>
        /// Name of the regression test plugin.
        /// </summary>
        internal const string RegressionTestPluginName = "chaskistest";

        /// <summary>
        /// List of all of the channels the bot is in.
        /// </summary>
        internal static readonly IReadOnlyList<string> JoinedChannels;

        // ---------------- Constructors ----------------

        static TestConstants()
        {
            JoinedChannels = new List<string>
            {
                Channel1,
                Channel2
            }.AsReadOnly();
        }
    }
}
