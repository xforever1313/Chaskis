//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chaskis.Plugins.WeatherBot
{
    /// <summary>
    /// This class is what reports the weather to the IRC channel.
    /// </summary>
    public class WeatherReporter
    {
        // -------- Fields --------

        /// <summary>
        /// What queries the weather.
        /// </summary>
        private IWeatherQuery weatherQuery;

        /// <summary>
        /// NOAA requests that we do not query the SOAP service more than an hour
        /// at a time.  We will cache our results for an hour in this dictionary.
        /// Key is the zip code, value is the weather report.
        /// </summary>
        private Dictionary<string, WeatherReport> reportCache;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="weatherQuery">The means to query the weather.</param>
        public WeatherReporter( IWeatherQuery weatherQuery )
        {
            this.weatherQuery = weatherQuery;
            this.reportCache = new Dictionary<string, WeatherReport>();
        }

        // -------- Functions --------

        /// <summary>
        /// Queries the weather service in the background and returns
        /// what should be send over IRC.
        /// </summary>
        /// <param name="zip">Zip code.</param>
        /// <returns>
        /// The string that should go out over IRC.
        /// Can be report or an error message.
        /// </returns>
        public async Task<string> QueryWeather( string zip )
        {
            string returnString = string.Empty;
            try
            {
                if( reportCache.ContainsKey( zip ) )
                {
                    // If our cache reports a null, its an invalid zip code.  Print error message.
                    if( reportCache[zip] == null )
                    {
                        returnString = "Error with one or more zip codes: Error: Zip code \"" + zip + "\" is not a valid US zip code";
                    }
                    // Otherwise, if the value in our cache is old, get updated info.
                    else if( ( DateTime.UtcNow - reportCache[zip].ConstructionTime ).TotalSeconds >= this.weatherQuery.Cooldown )
                    {
                        // Get the weather report in the background.
                        WeatherReport report = await this.weatherQuery.QueryForWeather( zip );
                        returnString = report.ToString();
                        reportCache[zip] = report;
                    }
                    // Otherwise, our cache is still new, do not query NOAA, just return our cache.
                    else
                    {
                        returnString = reportCache[zip].ToString();
                    }
                }
                else
                {
                    // Get the weather report in the background.
                    WeatherReport report = await this.weatherQuery.QueryForWeather( zip );
                    returnString = report.ToString();
                    reportCache[zip] = report;
                }
            }
            catch( QueryException err )
            {
                // Only fill our cache if an invalid zip.  Its possible we timed out
                // or the service is just having a tough time at the moment.  We don't
                // want to fill the cache with a zip code that may be okay in the future,
                // just not now.
                if( err.ErrorCode == QueryErrors.InvalidZip )
                {
                    reportCache[zip] = null; // Invalid zip, no sense wasting our time querying the SOAP service next time around.
                }

                returnString = err.Message;
            }
            catch( Exception err )
            {
                returnString = err.Message;
            }

            return returnString;
        }
    }
}