//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using LiteDB;

namespace Chaskis.Plugins.MetricsBot
{
    /// <summary>
    /// Connection to the database so data can persist upon restart.
    /// </summary>
    /// <remarks>
    /// The database design is made such that the "primary keys"
    /// are the server, channel, etc that sent the message,
    /// and the value is the count.  Another strategy could have been to
    /// just save each message to the database, and count the number
    /// of rows instead.  But, if a channel is really active, this would
    /// cause the database to get stupidly big, and there is a cost to counting rows in
    /// a big database.
    /// So instead, we just track the counts of each user so our database size
    /// is only as big as, really, the number of channels and users in those channels
    /// the bot is in.
    /// </remarks>
    internal sealed class MetricsBotDatabase : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly LiteDatabase dbConnection;
        private readonly ILiteCollection<MessageInfo> messageInfo;
        private readonly ILiteCollection<DayOfWeekInfo> dayOfWeekInfo;
        private readonly ILiteCollection<HourOfDayInfo> hourOfDayInfo;

        private readonly BsonMapper mapper;

        private readonly object cacheLock;
        private MetricsBotCache cache;

        // ---------------- Constructor ----------------

        public MetricsBotDatabase( string databaseLocation, BsonMapper mapper = null )
        {
            ConnectionString connectionString = new ConnectionString
            {
                Filename = databaseLocation,
                Connection = ConnectionType.Shared
            };

            this.dbConnection = new LiteDatabase( connectionString );
            this.messageInfo = this.dbConnection.GetCollection<MessageInfo>();
            this.dayOfWeekInfo = this.dbConnection.GetCollection<DayOfWeekInfo>();
            this.hourOfDayInfo = this.dbConnection.GetCollection<HourOfDayInfo>();

            this.mapper = mapper ?? BsonMapper.Global;

            this.cacheLock = new object();
            this.UpdateCacheFromDatabase();
        }

        // ---------------- Properties ----------------

        /// <summary/>
        /// Returns the current cache reference.
        /// This should be called from the same thread
        /// that calls <see cref="AddNewMessage"/>
        /// <summary>
        public IReadOnlyCache CurrentCache
        {
            get
            {
                lock( this.cacheLock )
                {
                    return this.cache;
                }
            }
        }

        // ---------------- Functions ----------------

        public void AddNewMessage( MessageInfoKey key )
        {
            lock( this.cacheLock )
            {
                this.cache.AddNewMessage( key );
            }
        }

        public void WriteCacheToDatabase()
        {
            MetricsBotCache copy;
            lock( this.cacheLock )
            {
                copy = this.cache.Clone();
            }

            foreach( KeyValuePair<MessageInfoKey, long> cache in copy.MessageCounts )
            {
                MessageInfo dbValue = this.messageInfo.FindById(
                    this.mapper.ToDocument( cache.Key )
                );

                if( dbValue == null )
                {
                    dbValue = new MessageInfo( cache.Key, cache.Value );
                    this.messageInfo.Insert( dbValue );
                }
                else
                {
                    if( dbValue.Count != cache.Value )
                    {
                        dbValue.Count = cache.Value;
                        this.messageInfo.Update( dbValue );
                    }
                }
            }

            foreach( KeyValuePair<DayOfWeekInfoKey, long> cache in copy.DayOfWeekCounts )
            {
                DayOfWeekInfo dbValue = this.dayOfWeekInfo.FindById(
                    this.mapper.ToDocument( cache.Key )
                );

                if( dbValue == null )
                {
                    dbValue = new DayOfWeekInfo( cache.Key, cache.Value );
                    this.dayOfWeekInfo.Insert( dbValue );
                }
                else
                {
                    if( dbValue.Count != cache.Value )
                    {
                        dbValue.Count = cache.Value;
                        this.dayOfWeekInfo.Update( dbValue );
                    }
                }
            }

            foreach( KeyValuePair<HourOfDayInfoKey, long> cache in copy.HourOfDayCounts )
            {
                HourOfDayInfo dbValue = this.hourOfDayInfo.FindById(
                    this.mapper.ToDocument( cache.Key )
                );

                if( dbValue == null )
                {
                    dbValue = new HourOfDayInfo( cache.Key, cache.Value );
                    this.hourOfDayInfo.Insert( dbValue );
                }
                else
                {
                    if( dbValue.Count != cache.Value )
                    {
                        dbValue.Count = cache.Value;
                        this.hourOfDayInfo.Update( dbValue );
                    }
                }
            }
        }

        public void UpdateCacheFromDatabase()
        {
            MetricsBotCache newCache = new MetricsBotCache(
                this.messageInfo,
                this.dayOfWeekInfo,
                this.hourOfDayInfo
            );

            lock( this.cacheLock )
            {
                // Swap references so there is as little time time locked as possible.
                this.cache = newCache;
            }
        }

        public MessageInfo GetInfo( MessageInfoKey key )
        {
            lock( this.cacheLock )
            {
                if( this.cache.MessageCounts.ContainsKey( key ) )
                {
                    return new MessageInfo( key, this.cache.MessageCounts[key] );
                }
                else
                {
                    return null;
                }
            }
        }

        public void Dispose()
        {
            this.dbConnection?.Dispose();
        }
    }
}
