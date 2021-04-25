//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Chaskis.Plugins.WeatherBot
{
    public enum QueryErrors
    {
        /// <summary>
        /// When the forecast tag is missing from the query.
        /// </summary>
        MissingForecast,

        /// <summary>
        /// Error from the query's service.
        /// </summary>
        ServerError,

        /// <summary>
        /// Error when there's something from with the lat/long
        /// tag when getting the coordinates of a zip code.
        /// </summary>
        MissingLatLon,

        /// <summary>
        /// Invalid lat/long string when getting the coordinates
        /// of a zip code.
        /// </summary>
        InvalidLatLon,

        /// <summary>
        /// Invalid ZIP code.
        /// </summary>
        InvalidZip
    }

    /// <summary>
    /// Errors that occur when talking to the weather query's service.
    /// </summary>
    public class QueryException : ApplicationException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="error">The error that triggered the exception.</param>
        /// <param name="message">The message of what triggered the exception.</param>
        public QueryException( QueryErrors error, string message ) :
            base( message )
        {
            this.ErrorCode = error;
        }

        /// <summary>
        /// The error that triggered the exception.
        /// </summary>
        public QueryErrors ErrorCode { get; private set; }
    }
}