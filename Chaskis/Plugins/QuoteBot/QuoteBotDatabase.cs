//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;

namespace Chaskis.Plugins.QuoteBot
{
    public class QuoteBotDatabase : IDisposable
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The SQLite connection.
        /// </summary>
        private LiteDatabase dbConnection;

        private LiteCollection<Quote> quotes;

        private Random random;

        // ---------------- Constructor ----------------

        public QuoteBotDatabase( string databaseLocation )
        {
            this.dbConnection = new LiteDatabase( databaseLocation );
            this.quotes = this.dbConnection.GetCollection<Quote>();
            this.random = new Random();
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Adds the given quote object to the sqlite database.
        /// Returns the generated ID.
        /// Runs in a background thread.
        /// </summary>
        /// <param name="quote">The quote to add.</param>
        /// <returns>The ID that was generated for the quote.</returns>
        public Task<int> AddQuoteAsync( Quote quote )
        {
            return Task<int>.Run(
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
        /// <param name="quote">The quote to add.</param>
        /// <returns>The ID that was generated for the quote.</returns>
        public int AddQuote( Quote quote )
        {
            lock( this.quotes )
            {
                BsonValue id = this.quotes.Insert( quote );
                return id.AsInt32;
            }
        }

        /// <summary>
        /// Deletes the given quote based on ID.
        /// Runs in background thread.
        /// </summary>
        /// <returns>True if the object was deleted.</returns>
        public Task<bool> DeleteQuoteAsync( int id )
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
        /// <returns>True if the object was deleted.</returns>
        public bool DeleteQuote( int id )
        {
            if( id < 0 )
            {
                throw new ArgumentOutOfRangeException( "ID must be positive, got " + id );
            }

            lock( this.quotes )
            {
                // Delete returns rows affected.
                bool success = this.quotes.Delete( id );
                return success;
            }
        }

        /// <summary>
        /// Gets the quote based on the id.
        /// Runs in background thread.
        /// </summary>
        /// <returns>The found quote.  Null if none were found.</returns>
        public Task<Quote> GetQuoteAsync( int id )
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
            IEnumerable<Quote> ids;
            lock( this.quotes )
            {
                ids = this.quotes.FindAll();
            }

            if( ids.Count() == 0 )
            {
                return null;
            }

            return ids.ElementAt( this.random.Next( 0, ids.Count() ) );
        }

        /// <summary>
        /// Gets the quote based on the id.
        /// </summary>
        /// <returns>The found quote. Null if we can't find it.</returns>
        public Quote GetQuote( int id )
        {
            if( id < 0 )
            {
                throw new ArgumentOutOfRangeException( "ID must be positive, got " + id );
            }

            lock( this.quotes )
            {
                return this.quotes.FindOne( q => q.Id == id );
            }
        }

        /// <summary>
        /// Closes the database and cleans up this class.
        /// </summary>
        public void Dispose()
        {
            if( this.dbConnection != null )
            {
                this.dbConnection.Dispose();
            }
        }
    }
}
