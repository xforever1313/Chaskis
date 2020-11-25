//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Threading.Tasks;

namespace Chaskis.Plugins.WeatherBot
{
    /// <summary>
    /// Interface to query a weather report.
    /// </summary>
    public interface IWeatherQuery
    {
        /// <summary>
        /// How long we must wait between queries to the service in seconds.
        /// </summary>
        int Cooldown { get; }

        /// <summary>
        /// Queries for the weather in a background thread.
        /// </summary>
        /// <param name="zip">The ZIP code to get weather information from.</param>
        /// <returns>The weather report.</returns>
        Task<WeatherReport> QueryForWeather( string zip );
    }
}