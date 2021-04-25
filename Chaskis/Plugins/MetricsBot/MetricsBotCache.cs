//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiteDB;

namespace Chaskis.Plugins.MetricsBot
{
    /// <summary>
    /// Cache so we don't have to waste time going out to the filesystem
    /// whenever someone queries information.
    /// 
    /// This way, we don't block the receive thread for as little as possible.
    /// </summary>
    internal class MetricsBotCache : IReadOnlyCache
    {
        // ---------------- Fields ----------------

        private readonly Dictionary<MessageInfoKey, long> messageCounts;
        private readonly Dictionary<DayOfWeekInfoKey, long> dayOfWeekCounts;
        private readonly Dictionary<HourOfDayInfoKey, long> hourOfDayCounts;

        // ---------------- Constructor ----------------

        public MetricsBotCache()
        {
        }

        private MetricsBotCache( MetricsBotCache copy )
        {
            this.messageCounts = new Dictionary<MessageInfoKey, long>( copy.messageCounts );
            this.dayOfWeekCounts = new Dictionary<DayOfWeekInfoKey, long>( copy.dayOfWeekCounts );
            this.hourOfDayCounts = new Dictionary<HourOfDayInfoKey, long>( copy.hourOfDayCounts );

            this.SetupReadonlyDictionaries();
        }

        public MetricsBotCache(
            ILiteCollection<MessageInfo> messageInfo,
            ILiteCollection<DayOfWeekInfo> dayOfWeekInfo,
            ILiteCollection<HourOfDayInfo> hourOfDayInfo
        )
        {
            {
                var allMessageInfos = messageInfo.FindAll();
                this.messageCounts = new Dictionary<MessageInfoKey, long>( allMessageInfos.Count() );
                foreach( MessageInfo info in allMessageInfos )
                {
                    this.messageCounts[info.Id] = info.Count;
                }
            }

            {
                var allDayOfWeekInfos = dayOfWeekInfo.FindAll();
                this.dayOfWeekCounts = new Dictionary<DayOfWeekInfoKey, long>( allDayOfWeekInfos.Count() );
                foreach( DayOfWeekInfo info in allDayOfWeekInfos )
                {
                    this.dayOfWeekCounts[info.Id] = info.Count;
                }
            }

            {
                var allHourOfDayInfos = hourOfDayInfo.FindAll();
                this.hourOfDayCounts= new Dictionary<HourOfDayInfoKey, long>( hourOfDayInfo.Count() );
                foreach( HourOfDayInfo info in allHourOfDayInfos )
                {
                    this.hourOfDayCounts[info.Id] = info.Count;
                }
            }

            SetupReadonlyDictionaries();
        }

        private void SetupReadonlyDictionaries()
        {
            this.MessageCounts = new ReadOnlyDictionary<MessageInfoKey, long>( this.messageCounts );
            this.DayOfWeekCounts = new ReadOnlyDictionary<DayOfWeekInfoKey, long>( this.dayOfWeekCounts );
            this.HourOfDayCounts = new ReadOnlyDictionary<HourOfDayInfoKey, long>( this.hourOfDayCounts );
        }

        // ---------------- Properties ----------------

        public IReadOnlyDictionary<MessageInfoKey, long> MessageCounts { get; private set; }

        public IReadOnlyDictionary<DayOfWeekInfoKey, long> DayOfWeekCounts { get; private set; }

        public IReadOnlyDictionary<HourOfDayInfoKey, long> HourOfDayCounts { get; private set; }

        // ---------------- Functions ----------------

        public void AddNewMessage( MessageInfoKey messageKey )
        {
            DateTime timeStamp = DateTime.UtcNow;

            // Our time stamps are nothing more than
            // a subset of the message info key, might
            // as well add those as well while we are here.
            DayOfWeekInfoKey dowKey = new DayOfWeekInfoKey(
                messageKey.Protocol,
                messageKey.Server,
                messageKey.Channel,
                timeStamp.DayOfWeek
            );

            HourOfDayInfoKey hodKey = new HourOfDayInfoKey(
                messageKey.Protocol,
                messageKey.Server,
                messageKey.Channel,
                timeStamp.Hour
            );

            if( this.messageCounts.ContainsKey( messageKey ) == false )
            {
                this.messageCounts[messageKey] = 1;
            }
            else
            {
                ++this.messageCounts[messageKey];
            }

            if( this.dayOfWeekCounts.ContainsKey( dowKey ) == false )
            {
                this.dayOfWeekCounts[dowKey] = 1;
            }
            else
            {
                ++this.dayOfWeekCounts[dowKey];
            }

            if( this.hourOfDayCounts.ContainsKey( hodKey ) == false )
            {
                this.hourOfDayCounts[hodKey] = 1;
            }
            else
            {
                ++this.hourOfDayCounts[hodKey];
            }
        }

        public MetricsBotCache Clone()
        {
            return new MetricsBotCache( this );
        }
    }
}
