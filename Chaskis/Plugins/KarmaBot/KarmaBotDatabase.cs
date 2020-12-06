//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;

namespace Chaskis.Plugins.KarmaBot
{
    /// <summary>
    /// Handles talking to the database.
    /// </summary>
    public class KarmaBotDatabase : IDisposable
    {
        // -------- Fields --------

        /// <summary>
        /// The db connection.
        /// </summary>
        private readonly LiteDatabase dbConnection;

        private readonly LiteCollection<IrcUser> users;

        /// <summary>
        /// Cache for irc users so we don't need to consistently query the database
        /// when someone just wants karma for something.
        /// </summary>
        private readonly Dictionary<string, IrcUser> ircUserCache;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="databaseLocation">The database location.</param>
        public KarmaBotDatabase( string databaseLocation )
        {
            this.dbConnection = new LiteDatabase( databaseLocation );
            this.users = this.dbConnection.GetCollection<IrcUser>();

            this.ircUserCache = new Dictionary<string, IrcUser>();
        }

        // -------- Functions --------

        /// <summary>
        /// Increases the karma of the given user by 1.
        /// </summary>
        /// <param name="userName">The user name to increase the karma of.</param>
        /// <returns>The new number of karma the user has after incrementing.</returns>
        public async Task<int> IncreaseKarma( string userName )
        {
            userName = userName.ToLower();

            IrcUser user = null;
            if( ( this.ircUserCache.ContainsKey( userName ) == false ) || ( this.ircUserCache[userName].Id == -1 ) )
            {
                // If our cache does not have the user, or the cache has an invalid ID for the user,
                // we need to query it so we have the latest information.
                user = await QueryUserAsync( userName );
            }
            // Otherwise, if our cache has the user, grab it.
            else if( this.ircUserCache.ContainsKey( userName ) )
            {
                user = this.ircUserCache[userName];
            }

            // If the user is null, it means it doesn't exist in the database yet; create a new one
            if( user == null )
            {
                user = new IrcUser
                {
                    KarmaCount = 0,
                    UserName = userName
                };
            }

            // Decrease the karma of the user, but only if it doesn't cause an underflow, otherwise, leave it alone.
            if( user.KarmaCount != int.MaxValue )
            {
                ++user.KarmaCount;
            }

            // Save the user to the database, and by extension, the cache.
            await SaveUserAsync( user );
            this.ircUserCache[userName] = user;

            return user.KarmaCount;
        }

        /// <summary>
        /// Decreases the karma of the given user by 1.
        /// </summary>
        /// <param name="userName">The user name to decrease the karma of.</param>
        /// <returns>The new number of karma the user has after decreasing.</returns>
        public async Task<int> DecreaseKarma( string userName )
        {
            userName = userName.ToLower();

            IrcUser user = null;
            if( ( this.ircUserCache.ContainsKey( userName ) == false ) || ( this.ircUserCache[userName].Id == -1 ) )
            {
                // If our cache does not have the user, or the cache has an invalid ID for the user,
                // we need to query it so we have the latest information.
                user = await QueryUserAsync( userName );
            }
            // Otherwise, if our cache has the user, grab it.
            else if( this.ircUserCache.ContainsKey( userName ) )
            {
                user = this.ircUserCache[userName];
            }

            // If the user is null, it means it doesn't exist in the database yet; create a new one
            if( user == null )
            {
                user = new IrcUser
                {
                    KarmaCount = 0,
                    UserName = userName
                };
            }

            // Decrease the karma of the user, but only if it doesn't cause an underflow, otherwise, leave it alone.
            if( user.KarmaCount != int.MinValue )
            {
                --user.KarmaCount;
            }

            // Save the user to the database, and by extension, the cache.
            await SaveUserAsync( user );
            this.ircUserCache[userName] = user;

            return user.KarmaCount;
        }

        /// <summary>
        /// Queries the database (or the cache if its in there)
        /// for the karma of the given user name.
        /// </summary>
        /// <param name="userName">The user name to get the karma of.</param>
        /// <returns>The number of karma the user has, 0 if the user does not exist.</returns>
        public async Task<int> QueryKarma( string userName )
        {
            userName = userName.ToLower();

            // If the user is in our cache, return the information.
            if( this.ircUserCache.ContainsKey( userName ) )
            {
                return this.ircUserCache[userName].KarmaCount;
            }
            else
            {
                IrcUser user = await QueryUserAsync( userName );
                if( user == null )
                {
                    // If no karma exists for this user, return 0.  But first, add it to the cache so
                    // we don't waste time querying things.
                    user = new IrcUser
                    {
                        UserName = userName,
                        KarmaCount = 0,
                        Id = -1 // Not in database, its database ID is -1.
                    };
                }

                // Add user to karma cache.
                this.ircUserCache[userName] = user;

                return user.KarmaCount;
            }
        }

        /// <summary>
        /// Queries the database for the given user.
        /// Returns null if the user does not exist in the database.
        /// </summary>
        /// <param name="userName">The user name to get the karma of.</param>
        /// <returns>The user from the database.  Null if the user does not exist.</returns>
        private IrcUser QueryUser( string userName )
        {
            lock( this.users )
            {
                IrcUser user = this.users.FindOne( u => u.UserName.Equals( userName ) );
                if( user != null )
                {
                    // Unsure if the clone is needed, but without it, bad things happen.
                    user = user.Clone();
                }

                return user;
            }
        }

        /// <summary>
        /// Queries the database for the given user in a background thread.
        /// Returns null if the user does not exist in the database.
        /// </summary>
        /// <param name="userName">The user name to get the karma of.</param>
        /// <returns>The user from the database.  Null if the user does not exist.</returns>
        private Task<IrcUser> QueryUserAsync( string userName )
        {
            return Task.Run(
                delegate ()
                {
                    return this.QueryUser( userName );
                }
            );
        }

        /// <summary>
        /// Saves the given user to the database in a background thread.
        /// </summary>
        /// <param name="userToSave">The user to save.</param>
        /// <returns>The task so this can be run in the background.</returns>
        private Task SaveUserAsync( IrcUser userToSave )
        {
            return Task.Run(
                delegate ()
                {
                    lock( this.users )
                    {
                        if( this.users.Exists( u => u.UserName.Equals( userToSave.UserName ) ) )
                        {
                            this.users.Update( userToSave );
                        }
                        else
                        {
                            this.users.Insert( userToSave );
                        }
                    }
                }
            );
        }

        /// <summary>
        /// Closes the database and cleans up this class.
        /// </summary>
        public void Dispose()
        {
            this.dbConnection.Dispose();
        }
    }
}