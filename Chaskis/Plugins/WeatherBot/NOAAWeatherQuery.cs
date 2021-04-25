//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Net;
using System.Threading.Tasks;

namespace Chaskis.Plugins.WeatherBot
{
    public class NOAAWeatherQuery : IWeatherQuery
    {
        // -------- Fields --------

        private static readonly string userAgent = "Chaskis IRC WeatherBot";

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public NOAAWeatherQuery()
        {
        }

        // -------- Properties -------

        /// <summary>
        /// NOAA requests that we don't make more than 1
        /// request per location per hour.
        /// </summary>
        public int Cooldown
        {
            get
            {
                // 1 Hour
                return 1 * 60 * 60;
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Queries NOAA in the backgroud.
        /// </summary>
        /// <param name="zip">The ZIP code to get weather information from.</param>
        /// <returns>The weather report from NOAA.</returns>
        public Task<WeatherReport> QueryForWeather( string zip )
        {
            return Task.Run(
                delegate ()
                {
                    // First, get the Lat/Long of the zip code.
                    Tuple<string, string> latLon;
                    using( WebClient client = new WebClient() )
                    {
                        client.QueryString.Add( "whichClient", "LatLonListZipCode" );
                        client.QueryString.Add( "listZipCodeList", zip );
                        client.QueryString.Add( "Submit", "Submit" );

                        // Per http://stackoverflow.com/questions/32641072/current-observation-feed-from-weather-gov-forbidden-403,
                        // Weather.gov now requires a user agent when querying their API.

                        client.Headers.Add( "user-agent", userAgent );

                        string response = client.DownloadString( "https://graphical.weather.gov/xml/SOAP_server/ndfdXMLclient.php" );
                        latLon = XmlLoader.ParseLatitudeLongitude( response, zip );
                    }

                    // Next, get the weather report.
                    WeatherReport report = null;
                    using( WebClient client = new WebClient() )
                    {
                        client.QueryString.Add( "whichClient", "GmlLatLonList" );
                        client.QueryString.Add( "gmlListLatLon", Uri.EscapeDataString( latLon.Item1 + "," + latLon.Item2 ) );
                        client.QueryString.Add( "featureType", "Forecast_Gml2Point" );
                        client.QueryString.Add( "product", "glance" );
                        client.QueryString.Add( "Unit", "e" );
                        client.QueryString.Add( "maxt", "maxt" ); // High Temp
                        client.QueryString.Add( "mint", "mint" ); // Low Temp
                        client.QueryString.Add( "pop12", "pop12" ); // Precipitation chance
                        client.QueryString.Add( "wx", "wx" ); // Current Conditations
                        client.QueryString.Add( "appt", "appt" ); // Apparent Temp
                        client.QueryString.Add( "temp", "temp" ); // Current Temp
                        client.QueryString.Add( "Submit", "Submit" );

                        client.Headers.Add( "user-agent", userAgent );

                        string response = client.DownloadString( "https://graphical.weather.gov/xml/SOAP_server/ndfdXMLclient.php" );
                        report = XmlLoader.ParseWeatherReport( response, zip );
                    }
                    return report;
                }
            );
        }
    }
}