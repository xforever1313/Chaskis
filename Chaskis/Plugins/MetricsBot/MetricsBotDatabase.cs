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
    internal sealed class MetricsBotDatabase : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly LiteDatabase dbConnection;
        private readonly ILiteCollection<MessageInfo> messageInfo;
        private readonly BsonMapper mapper;

        private readonly object cacheLock;
        private Dictionary<MessageInfoKey, long> messageInfoCache;

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

            this.mapper = mapper ?? BsonMapper.Global;

            this.messageInfoCache = new Dictionary<MessageInfoKey, long>();

            this.cacheLock = new object();
            this.UpdateCacheFromDatabase();
        }

        // ---------------- Properties ----------------

        // ---------------- Functions ----------------

        public void AddNewMessage( MessageInfoKey key )
        {
            lock( this.cacheLock )
            {
                if( this.messageInfoCache.ContainsKey( key ) == false )
                {
                    this.messageInfoCache[key] = 1;
                }
                else
                {
                    ++this.messageInfoCache[key];
                }
            }
        }

        public void WriteCacheToDatabase()
        {
            Dictionary<MessageInfoKey, long> copy;
            lock( this.cacheLock )
            {
                // We *probably* don't need to make a deep-copy of MessageInfoKey
                // since they are immutable.
                copy = new Dictionary<MessageInfoKey, long>( this.messageInfoCache );
            }

            foreach( KeyValuePair<MessageInfoKey, long> cache in copy )
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
        }

        public void UpdateCacheFromDatabase()
        {
            Dictionary<MessageInfoKey, long> newCache = new Dictionary<MessageInfoKey, long>();
            foreach( MessageInfo info in this.messageInfo.FindAll() )
            {
                newCache[info.Id] = info.Count;
            }

            Dictionary<MessageInfoKey, long> oldCache;
            lock( this.cacheLock )
            {
                // Swap references so there is as little time time locked as possible.
                oldCache = this.messageInfoCache;
                this.messageInfoCache = newCache;
            }

            oldCache.Clear();
        }

        public MessageInfo GetInfo( MessageInfoKey key )
        {
            lock( this.cacheLock )
            {
                if( this.messageInfoCache.ContainsKey( key ) )
                {
                    return new MessageInfo( key, this.messageInfoCache[key] );
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
