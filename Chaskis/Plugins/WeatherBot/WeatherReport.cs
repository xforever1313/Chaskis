
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

namespace Chaskis.Plugins.WeatherBot
{
    /// <summary>
    /// Represents a weather report from NOAA.
    /// </summary>
    public class WeatherReport
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.  Sets all properties to "??".
        /// </summary>
        public WeatherReport()
        {
            this.ZipCode = "?????";
            this.HighTemp = "??";
            this.LowTemp = "??";
            this.CurrentTemp = "??";
            this.ApparentTemp = "??";
            this.CurrentConditions = "Unknown Conditions";
            this.ChanceOfPrecipitation = "??";
        }

        // -------- Properties --------
   
        /// <summary>
        /// The zip code of the weather report.
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// The High Temperature in Farenheit.
        /// </summary>
        public string HighTemp { get; set; }

        /// <summary>
        /// The Low Temperature in Farenheit.
        /// </summary>
        public string LowTemp { get; set; }

        /// <summary>
        /// The current temperature in Farenheit
        /// </summary>
        public string CurrentTemp { get; set; }

        /// <summary>
        /// The apparent temperature (What the temperature feels to us,
        /// e.g. with Wind Chill) in Farenheit.
        /// </summary>
        public string ApparentTemp { get; set; }

        /// <summary>
        /// What its currently like outside, e.g. "Mostly Cloudy".
        /// </summary>
        public string CurrentConditions { get; set; }

        /// <summary>
        /// The chance of precipitation (e.g. rain or snow).
        /// </summary>
        public string ChanceOfPrecipitation { get; set; }
        
        // -------- Functions --------

        /// <summary>
        /// Returns a string representation of this class
        /// that can be sent over IRC.
        /// </summary>
        /// <returns>Weather for 12345: Currently: Mostly Cloudy, 100F (feels like 115F). High: 110F. Low: 50F.Chance of Precipitation: 13%.</returns>
        public override string ToString()
        {
            return string.Format(
                "Weather for {0} - {1}, {2}F (feels like {3}F).  High: {4}F. Low: {5}F. Chance of Precipitation: {6}%.",
                this.ZipCode,
                this.CurrentConditions,
                this.CurrentTemp,
                this.ApparentTemp,
                this.HighTemp,
                this.LowTemp,
                this.ChanceOfPrecipitation
            );
        }
    }
}
