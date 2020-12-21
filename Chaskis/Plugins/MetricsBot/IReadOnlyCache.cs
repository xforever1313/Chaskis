//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chaskis.Plugins.MetricsBot
{
    internal interface IReadOnlyCache
    {
        // ---------------- Properties ----------------

        IReadOnlyDictionary<MessageInfoKey, long> MessageCounts { get; }

        IReadOnlyDictionary<DayOfWeekInfoKey, long> DayOfWeekCounts { get; }

        IReadOnlyDictionary<HourOfDayInfoKey, long> HourOfDayCounts { get; }
    }

    internal static class IReadOnlyCacheExtensions
    {
        public static long GetCountOfAllMessageTypes( this IReadOnlyCache cache )
        {
            return cache.MessageCounts.Values.Sum();
        }
    }
}