//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Plugins.MeetBot
{
    public static class Regexes
    {
        // ---------------- Fields ----------------

        internal static readonly Regex MeetBotRootConfigVariable = new Regex(
            "{%meetbotroot%}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static readonly Regex TimeStampConfigVariable = new Regex(
            "{%timestamp%}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static readonly Regex MeetingTopicConfigVariable = new Regex(
            "{%meetingtopic%}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static readonly Regex ChannelConfigVariable = new Regex(
            "{%channel%}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static readonly Regex GeneratorTypeConfigVariable = new Regex(
            "{%generatortype%}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static readonly Regex FileNameConfigVariable = new Regex(
            "{%filename%}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static readonly Regex FullFilePathConfigVariable = new Regex(
            "{%fullfilepath%}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );
    }
}
