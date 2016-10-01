//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using Chaskis.Plugins.WeatherBot;
using NUnit.Framework;
using Tests.Plugins.WeatherBot.Mocks;

namespace Tests.Plugins.WeatherBot
{
    [TestFixture]
    public class WeatherReporterTest
    {
        // -------- Fields ---------

        /// <summary>
        /// The means to mock a weather query.
        /// </summary>
        private MockWeatherQuery weatherQuery;

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

        [TestFixtureSetUp]
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
            this.weatherQuery = new MockWeatherQuery();
            this.uut = new WeatherReporter( this.weatherQuery );
            this.currentReport = new WeatherReport();
        }

        // -------- Tests --------

        /// <summary>
        /// Tests that a valid zip code twice results in using the cached value.
        /// </summary>
        [Test]
        public async void ValidZipCacheTest()
        {
            this.currentReport.ApparentTemp = "10";
            this.weatherQuery.ReportToReturn = this.currentReport;
            this.weatherQuery.Cooldown = 1 * 60 * 60; // One hour.

            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( this.currentReport.ToString(), response );

                // No value in cache, expect query.
                Assert.IsTrue( this.weatherQuery.WasQueried );
            }

            this.weatherQuery.ResetStates();
            this.weatherQuery.ReportToReturn = this.currentReport;
            this.weatherQuery.Cooldown = 1 * 60 * 60;

            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( this.currentReport.ToString(), response );

                // Should use cached value.
                Assert.IsFalse( this.weatherQuery.WasQueried );
            }

            this.currentReport.ApparentTemp = "20";
            this.weatherQuery.ResetStates();
            this.weatherQuery.ReportToReturn = this.currentReport;
            this.weatherQuery.Cooldown = 0; // Should always query.

            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( this.currentReport.ToString(), response );

                // Cache timed out, should query.
                Assert.IsTrue( this.weatherQuery.WasQueried );
            }
        }

        /// <summary>
        /// Tests that a invalid zip code twice results in using the cached value.
        /// </summary>
        [Test]
        public async void InvalidZipCacheTest()
        {
            this.weatherQuery.ExceptionToThrowDuringQuery = this.invalidZipException;
            this.weatherQuery.Cooldown = 1 * 60 * 60; // One hour.

            {
                string response = await this.uut.QueryWeather( zip );
                // Expect invalid zip message.
                Assert.AreEqual( this.invalidZipException.Message, response );

                // No value in cache, expect query.
                Assert.IsTrue( this.weatherQuery.WasQueried );
            }

            this.weatherQuery.ResetStates();
            this.weatherQuery.ExceptionToThrowDuringQuery = this.invalidZipException;
            this.weatherQuery.Cooldown = 1 * 60 * 60;

            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( this.invalidZipException.Message, response );

                // Should use cached value, which is an error.
                Assert.IsFalse( this.weatherQuery.WasQueried );
            }

            this.weatherQuery.ResetStates();
            this.weatherQuery.ExceptionToThrowDuringQuery = this.invalidZipException;
            this.weatherQuery.Cooldown = 0; // Still should not query.

            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( this.invalidZipException.Message, response );
                // Should not query.  No need to waste the querier's time.
                Assert.IsFalse( this.weatherQuery.WasQueried );
            }
        }

        /// <summary>
        /// Tests to make sure we handle an Non-zip code based
        /// query exception properly (should not fill cache).
        /// </summary>
        [Test]
        public async void NonZipQueryException()
        {
            QueryException err = new QueryException( QueryErrors.InvalidLatLon, "ERROR" );
            this.weatherQuery.ExceptionToThrowDuringQuery = err;
            this.weatherQuery.Cooldown = 1 * 60 * 60; // One hour.

            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( err.Message, response );

                // No value in cache, expect query.
                Assert.IsTrue( this.weatherQuery.WasQueried );
            }

            this.weatherQuery.ResetStates();
            this.weatherQuery.ExceptionToThrowDuringQuery = err;
            this.weatherQuery.Cooldown = 1 * 60 * 60; // One hour.
            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( err.Message, response );

                // Non-zip code error should not fill cache, we should
                // still query.
                Assert.IsTrue( this.weatherQuery.WasQueried );
            }
        }

        /// <summary>
        /// Tests to make sure we handle an Non-zip code based
        /// query exception properly (should not fill cache).        /// </summary>
        [Test]
        public async void UnknownException()
        {
            Exception err = new Exception( "ERROR" );
            this.weatherQuery.ExceptionToThrowDuringQuery = err;
            this.weatherQuery.Cooldown = 1 * 60 * 60; // One hour.

            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( err.Message, response );

                // No value in cache, expect query.
                Assert.IsTrue( this.weatherQuery.WasQueried );
            }

            this.weatherQuery.ResetStates();
            this.weatherQuery.ExceptionToThrowDuringQuery = err;
            this.weatherQuery.Cooldown = 1 * 60 * 60; // One hour.
            {
                string response = await this.uut.QueryWeather( zip );
                Assert.AreEqual( err.Message, response );

                // Non-zip code error should not fill cache, we should
                // still query.
                Assert.IsTrue( this.weatherQuery.WasQueried );
            }
        }
    }
}