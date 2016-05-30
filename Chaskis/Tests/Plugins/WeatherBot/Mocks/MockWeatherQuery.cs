using System;
using System.Threading.Tasks;
using Chaskis.Plugins.WeatherBot;

namespace Tests.Plugins.WeatherBot.Mocks
{
    public class MockWeatherQuery : IWeatherQuery
    {
        // -------- Constructor -------

        /// <summary>
        /// Constructor.
        /// </summary>
        public MockWeatherQuery()
        {
            ResetStates();
        }

        // -------- Properties --------

        /// <summary>
        /// How long we must wait between queries to the service in seconds.
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        /// The report to return for QueryForWeather.
        /// </summary>
        public WeatherReport ReportToReturn { get; set; }

        /// <summary>
        /// The exception to throw during query.  Set to null to
        /// throw no exception.  This WILL throw if not set to null.
        /// </summary>
        public Exception ExceptionToThrowDuringQuery { get; set; }

        /// <summary>
        /// Whether or not this class we queried or not.
        /// </summary>
        public bool WasQueried { get; private set; }

        /// <summary>
        /// Queries for the weather in a background thread.
        /// </summary>
        /// <param name="zip">The ZIP code to get weather information from.</param>
        /// <returns>The weather report.</returns>
        public Task<WeatherReport> QueryForWeather( string zip )
        {
            return Task.Run(
                delegate()
                {
                    this.WasQueried = true;

                    if ( this.ExceptionToThrowDuringQuery != null )
                    {
                        throw this.ExceptionToThrowDuringQuery;
                    }

                    return this.ReportToReturn;
                }
            );
        }

        /// <summary>
        /// Reset the state of this class.
        /// </summary>
        public void ResetStates()
        {
            this.Cooldown = 1;
            this.ReportToReturn = new WeatherReport();
            this.ExceptionToThrowDuringQuery = null;
            this.WasQueried = false;
        }
    }
}
