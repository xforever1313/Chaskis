
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaskis.Plugins.WeatherBot
{
    public enum NOAAErrors
    {
        /// <summary>
        /// When the forecast tag is missing from the XML.
        /// </summary>
        MissingForecast,

        /// <summary>
        /// Error from NOAA's SOAP service.
        /// </summary>
        SOAPError,

        /// <summary>
        /// Error when there's something from with the lat/long
        /// tag when getting the coordinates of a zip code.
        /// </summary>
        MissingLatLon,

        /// <summary>
        /// Invalid lat/long string when getting the coordinates
        /// of a zip code.
        /// </summary>
        InvalidLatLon
    }

    /// <summary>
    /// Errors that occur when talking to NOAA's SOAP service.
    /// </summary>
    public class NOAAException : ApplicationException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="error">The error that triggered the exception.</param>
        /// <param name="message">The message of what triggered the exception.</param>
        public NOAAException( NOAAErrors error, string message ) :
            base( message )
        {
            this.ErrorCode = error;
        }

        /// <summary>
        /// The error that triggered the exception.
        /// </summary>
        public NOAAErrors ErrorCode { get; private set; }
    }
}
