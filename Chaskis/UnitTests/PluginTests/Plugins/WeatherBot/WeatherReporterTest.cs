//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading.Tasks;
using Chaskis.Plugins.WeatherBot;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.WeatherBot
{
    [TestFixture]
    public class WeatherReporterTest
    {
        // -------- Fields ---------

        /// <summary>
        /// The means to mock a weather query.
        /// </summary>
        private Mock<IWeatherQuery> weatherQuery;

        /// <summary>
        /// The current report created for the test.
        /// </summary>
        private WeatherReport currentReport;

        /// <summary>
        /// Unit under test.
        /// </summary>
        private WeatherReporter uut;

        /// <summary>
        /// Zip code to use.
        /// </summary>
        private const string zip = "12345";

        /// <summary>
        /// Invalid Zip code exception.
        /// </summary>
        private QueryException invalidZipException;

        // -------- Setup / Teardown --------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.invalidZipException = new QueryException(
                QueryErrors.InvalidZip,
                "Error with one or more zip codes: Error: Zip code \"" + zip + "\" is not a valid US zip code"
            );
        }

        [SetUp]
        public void TestSetup()
        {
            this.weatherQuery = new Mock<IWeatherQuery>( MockBehavior.Strict );
            this.uut = new WeatherReporter( this.weatherQuery.Object );
            this.currentReport = new WeatherReport();
        }

        // -------- Tests --------

        /// <summary>
        /// Tests that a valid zip code twice results in using the cached value.
        /// </summary>
        [Test]
        public void ValidZipCacheTest()
        {
            this.currentReport.ApparentTemp = "10";

            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 ); // 1 hour query cooldown.

                // Should expect a query.
                this.weatherQuery.Setup( w => w.QueryForWeather( It.Is<string>( s => s == zip ) ) ).Returns(
                    Task.Run( delegate () { return this.currentReport; } )
                );

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( this.currentReport.ToString(), response.Result );
            }

            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 );

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( this.currentReport.ToString(), response.Result );
            }

            this.currentReport.ApparentTemp = "20";

            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 0 ); // Should always query.

                // Should expect a query.
                this.weatherQuery.Setup( w => w.QueryForWeather( It.Is<string>( s => s == zip ) ) ).Returns(
                    Task.Run( delegate () { return this.currentReport; } )
                );

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( this.currentReport.ToString(), response.Result );
            }
        }

        /// <summary>
        /// Tests that a invalid zip code twice results in using the cached value.
        /// </summary>
        [Test]
        public void InvalidZipCacheTest()
        {
            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 ); // 1 hour query cooldown.
                this.weatherQuery.Setup( w => w.QueryForWeather( It.Is<string>( s => s == zip ) ) ).Throws( this.invalidZipException );

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                // Expect invalid zip message.
                Assert.AreEqual( this.invalidZipException.Message, response.Result );
            }

            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 ); // Should not query and use cache.

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                // Should use cached value, which is an error.
                Assert.AreEqual( this.invalidZipException.Message, response.Result );
            }

            {
                // Should not query.  No need to waste the querier's time.
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 0 );

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( this.invalidZipException.Message, response.Result );
            }
        }

        /// <summary>
        /// Tests to make sure we handle an Non-zip code based
        /// query exception properly (should not fill cache).
        /// </summary>
        [Test]
        public void NonZipQueryException()
        {
            QueryException err = new QueryException( QueryErrors.InvalidLatLon, "ERROR" );
            
            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 ); // 1 hour query cooldown.
                this.weatherQuery.Setup( w => w.QueryForWeather( It.Is<string>( s => s == zip ) ) ).Throws( err );

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( err.Message, response.Result );
            }

            {
                // Non-zip code error should not fill cache, we should
                // still query.
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 );
                this.weatherQuery.Setup( w => w.QueryForWeather( It.Is<string>( s => s == zip ) ) ).Throws( err );

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( err.Message, response.Result );
            }
        }

        /// <summary>
        /// Tests to make sure we handle an Non-zip code based
        /// query exception properly (should not fill cache).        /// </summary>
        [Test]
        public void UnknownException()
        {
            Exception err = new Exception( "ERROR" );

            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 );
                this.weatherQuery.Setup( w => w.QueryForWeather( It.Is<string>( s => s == zip ) ) ).Throws( err );

                // No value in cache, expect query.

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( err.Message, response.Result );
            }

            {
                this.weatherQuery.Setup( w => w.Cooldown ).Returns( 1 * 60 * 60 );
                this.weatherQuery.Setup( w => w.QueryForWeather( It.Is<string>( s => s == zip ) ) ).Throws( err );

                // Non-zip code error should not fill cache, we should
                // still query.

                Task<string> response = this.uut.QueryWeather( zip );
                response.Wait();

                Assert.AreEqual( err.Message, response.Result );
            }
        }
    }
}