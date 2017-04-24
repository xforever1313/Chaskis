//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Interop;

namespace Chaskis.Plugins.QuoteBot
{
    public class QuoteBotDatabase : IDisposable
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The SQLite connection.
        /// </summary>
        private SQLiteConnection sqlite;

        // ---------------- Constructor ----------------

        public QuoteBotDatabase( string databaseLocation )
        {
            ISQLitePlatform platform;
            if( Environment.OSVersion.Platform.Equals( PlatformID.Win32NT ) )
            {
                Console.WriteLine( "QuoteBot> Using Win32 Sqlite Platform" );
                platform = new SQLite.Net.Platform.Win32.SQLitePlatformWin32();
            }
            else
            {
                // Requires the SQLite.so (shared object) files to be installed.
                Console.WriteLine( "QuoteBot> Using Generic Sqlite Platform" );
                platform = new SQLite.Net.Platform.Generic.SQLitePlatformGeneric();
            }

            this.sqlite = new SQLiteConnection(
                platform,
                databaseLocation,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite
            );

            this.sqlite.CreateTable<Quote>();
            this.sqlite.Commit();
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Adds the given quote object to the sqlite database.
        /// Returns the generated ID.
        /// Runs in a background thread.
        /// </summary>
        /// <param name="quote">The quote to add. ID MUST BE NULL TO ADD.</param>
        /// <returns>The ID that was generated for the quote.</returns>
        public Task<long> AddQuoteAsync( Quote quote )
        {
            return Task<long>.Run(
                delegate ()
                {
                    return this.AddQuote( quote );
                }
            );
        }

        /// <summary>
        /// Adds the given quote object to the sqlite database.
        /// Returns the generated ID.
        /// </summary>
        /// <param name="quote">The quote to add. ID MUST BE NULL TO ADD.</param>
        /// <returns>The ID that was generated for the quote.</returns>
        public long AddQuote( Quote quote )
        {
            if( quote.Id != null )
            {
                throw new ArgumentException( "Quote ID must be null to add to database!" );
            }

            lock( this.sqlite )
            {
                this.sqlite.InsertOrReplace( quote );

                SQLiteCommand cmd = this.sqlite.CreateCommand( "SELECT last_insert_rowid()" );
                long id = cmd.ExecuteScalar<long>();

                this.sqlite.Commit();

                return id;
            }
        }

        /// <summary>
        /// Deletes the given quote based on ID.
        /// Runs in background thread.
        /// </summary>
        /// <returns>True if the object was delete (1 row was affected).</returns>
        public Task<bool> DeleteQuoteAsync( long id )
        {
            return Task<bool>.Run(
                delegate ()
                {
                    return this.DeleteQuote( id );
                }
            );
        }

        /// <summary>
        /// Deletes the given quote based on ID.
        /// </summary>
        /// <returns>True if the object was delete (1 row was affected).</returns>
        public bool DeleteQuote( long id )
        {
            if( id < 0 )
            {
                throw new ArgumentOutOfRangeException( "ID must be positive, got " + id );
            }


            lock( this.sqlite )
            {
                // Delete returns rows affected.
                bool success = this.sqlite.Delete<Quote>( id ) == 1;
                this.sqlite.Commit();

                return success;
            }
        }

        /// <summary>
        /// Gets the quote based on the id.
        /// Runs in background thread.
        /// </summary>
        /// <exception cref="SQLiteException">If not found (per SQLite-net's documentation).</exception>
        /// <returns>The found quote.</returns>
        public Task<Quote> GetQuoteAsync( long id )
        {
            return Task<Quote>.Run(
                delegate ()
                {
                    return this.GetQuote( id );
                }
            );
        }

        /// <summary>
        /// Gets a random quote from the database in a background thread.
        /// </summary>
        /// <returns>A random quote.  Null if none are in the database.</returns>
        public Task<Quote> GetRandomQuoteAsync()
        {
            return Task<Quote>.Run(
                delegate ()
                {
                    return this.GetRandomQuote();
                }
            );
        }

        /// <summary>
        /// Gets a random quote from the database.
        /// </summary>
        /// <returns>A random quote.  Null if none are in the database.</returns>
        public Quote GetRandomQuote()
        {
            lock( this.sqlite )
            {
                return this.sqlite.FindWithQuery<Quote>( "SELECT * FROM Quote WHERE id IN (SELECT id FROM quote ORDER BY RANDOM() LIMIT 1)" );
            }
        }

        /// <summary>
        /// Gets the quote based on the id.
        /// </summary>
        /// <exception cref="InvalidOperationException">If not found.</exception>
        /// <returns>The found quote.</returns>
        public Quote GetQuote( long id )
        {
            if( id < 0 )
            {
                throw new ArgumentOutOfRangeException( "ID must be positive, got " + id );
            }

            lock( this.sqlite )
            {
                return this.sqlite.Get<Quote>( q => q.Id == id );
            }
        }

        /// <summary>
        /// Closes the database and cleans up this class.
        /// </summary>
        public void Dispose()
        {
            if( this.sqlite != null )
            {
                lock( this.sqlite )
                {
                    this.sqlite.Dispose();
                }
            }
        }
    }
}
