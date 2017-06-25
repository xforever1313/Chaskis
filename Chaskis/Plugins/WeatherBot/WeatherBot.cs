//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChaskisCore;

namespace Chaskis.Plugins.WeatherBot
{
    // Weather bot uses the NOAA XML SOAP service located here: http://graphical.weather.gov/xml/
    // They ask the following:
    // "The NDFD is updated no more than hourly.
    // We request developers using this SOAP service for local
    // applications only make a request for a specific point no more
    // than once an hour. The database is currently updated by 45 minutes after the hour."
    // Therfore, we must cache any requests for an hour before going out to the SOAP service.
    //
    // Since we use NOAA, this means we can only get the weather in the US.
    //
    // When a user triggers the bot, we need to take the ZIP code and first get the lat/lon of that zip code.
    // We can do that using NOAA's "Grid Points For A Zip Code" engine by making a get request to:
    // http://graphical.weather.gov/xml/SOAP_server/ndfdXMLclient.php?whichClient=LatLonListZipCode&listZipCodeList=XXXXX&Submit=Submit
    // where XXXXX is the zip code.
    //
    // We then need to take the lat/long coordinates returned and pass it to NOAA's "NDFD Data Encoded in GML for Single Time" client.
    // This returns XML that contains the information we need.  We do this by making a get request to:
    // http://graphical.weather.gov/xml/SOAP_server/ndfdXMLclient.php?whichClient=GmlLatLonList&gmlListLatLon=XX.XXXX%2C-YY.YYYY&featureType=Forecast_Gml2Point&product=glance&Unit=e&maxt=maxt&mint=mint&pop12=pop12&wx=wx&appt=appt&temp=temp&Submit=Submit
    // where XX.XXXX is the Latitude, YY.YYYY is the longitude.  By specifying the above URL, we get the following information in the form of an XML tag:
    // | Get Request | What it returns             | XML Tag                    |
    // | ----------- | :-------------------------: | :-----------------------:  |
    // | Unit=e      | Units are emprical          | None                       |
    // | maxt=maxt   | The high temp               | <app:maximumTemperature>   |
    // | mint=mint   | The low temp                | <app:minimumTemperature>   |
    // | temp=temp   | The current temp            | <app:temperature>          |
    // | appt=appt   | The apparent temp           | <app:apparentTemperature>  | (What it feels like to us. More info: http://graphical.weather.gov/definitions/defineApparentT.html )
    // | pop12=pop12 | The chance of precipitation | <app:probOfPrecip12hourly> |
    // | wx=wx       | Description of the weather  | <app:weatherPhrase>        | (E.g. "Mostly Cloudy")
    //
    // All of the XML tags live in the node <app:Forecast_Gml2Point>, which lives in the node <gml:featureMember>,
    // which lives in the root node <app:NdfdForecastCollection>.
    //
    // See the samples in Tests/Plugins/WeatherBot/TestFiles/SampleReport.xml for a sample XML we get from the SOAP service.
    //
    // When a user in the IRC channel types !weather XXXXX, we will respond with:
    // Weather for XXXXX: Currently: Mostly Cloudy, 100F (feels like 115F). High: 110F. Low: 50F.Chance of Precipitation: 13%.
    //
    // We will also have a 15 second cooldown.  We don't want to spam NOAA's API.

    [ChaskisPlugin( "weatherbot" )]
    public class WeatherBot : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "1.0.0";

        /// <summary>
        /// The command to trigger the weather bot.
        /// </summary>
        private const string weatherCommand = @"!weather\s+(?<zipCode>\d{5})";

        /// <summary>
        /// List of handlers.
        /// </summary>
        private List<IIrcHandler> handlers;

        /// <summary>
        /// The cooldown for the bot.
        /// </summary>
        private const int cooldown = 15;

        /// <summary>
        /// Queries for the weather report.
        /// </summary>
        private WeatherReporter reporter;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public WeatherBot()
        {
            this.handlers = new List<IIrcHandler>();
            this.reporter = new WeatherReporter( new NOAAWeatherQuery() );
        }

        /// <summary>
        /// Returns the source code location of this plugin.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/WeatherBot";
            }
        }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        /// <summary>
        /// About this plugin.
        /// </summary>
        public string About
        {
            get
            {
                return "I print the weather of the given US zip code.";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Inits this plugin.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            MessageHandler weatherHandler = new MessageHandler(
                weatherCommand,
                HandleWeatherCommand,
                cooldown
            );

            this.handlers.Add( weatherHandler );
        }

        /// <summary>
        /// Handle help command.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            string message = string.Format(
                "Usage: '{0} zipCode'.  Note I have a cooldown of {1} seconds and only work with US zip codes.",
                "!weather",
                cooldown
            );

            writer.SendMessage(
                message,
                response.Channel
            );
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        /// <summary>
        /// Tears down this plugin.
        /// </summary>
        public void Dispose()
        {
        }

        // ---- Handlers ----

        /// <summary>
        /// Handles the help command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private static void HandleHelpCommand( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage(
                "Valid commands: XXXXX (US Zip Code), help, about, sourcecode.  Each command has a " + cooldown + " second cooldown.",
                response.Channel
            );
        }

        /// <summary>
        /// Handles the weather command by doing a GET request to NOAA's API
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private async void HandleWeatherCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;
            string zip = match.Groups["zipCode"].Value;
            string strToWrite = await this.reporter.QueryWeather( zip );
            writer.SendMessage( strToWrite, response.Channel );
        }
    }
}