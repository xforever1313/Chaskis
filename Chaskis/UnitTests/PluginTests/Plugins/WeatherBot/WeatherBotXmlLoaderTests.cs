//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using Chaskis.Plugins.WeatherBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.WeatherBot
{
    [TestFixture]
    public class WeatherBotXmlLoaderTests
    {
        // -------- Fields --------

        private const string defaultZip = "12345";

        /// <summary>
        /// Location of the weather bot test files.
        /// </summary>
        private static readonly string testFilesLocation = Path.Combine(
            TestHelpers.PluginTestsDir,
            "Plugins",
            "WeatherBot",
            "TestFiles"
        );

        // -------- Tests --------

        /// <summary>
        /// Tests the loading of a weather report.
        /// </summary>
        [Test]
        public void TestLoadWeatherReport()
        {
            string xml = ReadFile( Path.Combine( testFilesLocation, "SampleReport.xml" ) );
            WeatherReport report = XmlLoader.ParseWeatherReport( xml, defaultZip );

            Assert.AreEqual( defaultZip, report.ZipCode );
            Assert.AreEqual( "97.0", report.HighTemp );
            Assert.AreEqual( "78.0", report.LowTemp );
            Assert.AreEqual( "76.0", report.CurrentTemp );
            Assert.AreEqual( "75.0", report.ApparentTemp );
            Assert.AreEqual( "11.0", report.ChanceOfPrecipitation );
            Assert.AreEqual( "Mostly Clear", report.CurrentConditions );
        }

        /// <summary>
        /// Tests to make sure we handle a missing weather report from NOAA.
        /// </summary>
        [Test]
        public void TestBadWeatherReport()
        {
            string xml = ReadFile( Path.Combine( testFilesLocation, "BadReport.xml" ) );
            QueryException error = Assert.Throws<QueryException>( () =>
                XmlLoader.ParseWeatherReport( xml, defaultZip )
            );

            Assert.AreEqual( QueryErrors.MissingForecast, error.ErrorCode );
        }

        /// <summary>
        /// Tests to make sure we can parse NOAA's zip code response.
        /// </summary>
        [Test]
        public void TestLatLon()
        {
            string xml = ReadFile( Path.Combine( testFilesLocation, "SampleZipCodeResponse.xml" ) );
            Tuple<string, string> coor = XmlLoader.ParseLatitudeLongitude( xml, defaultZip );

            Assert.AreEqual( "39.1687", coor.Item1 );
            Assert.AreEqual( "-71.6158", coor.Item2 );
        }

        /// <summary>
        /// Tests to make sure we can handle a SOAP error from NOAA properly.
        /// </summary>
        [Test]
        public void TestZipErrorResponse()
        {
            string xml = ReadFile( Path.Combine( testFilesLocation, "SampleZipError.xml" ) );

            {
                QueryException error = Assert.Throws<QueryException>( () =>
                    XmlLoader.ParseLatitudeLongitude( xml, defaultZip )
                );

                Assert.AreEqual( QueryErrors.InvalidZip, error.ErrorCode );
            }

            {
                QueryException error = Assert.Throws<QueryException>( () =>
                    XmlLoader.ParseWeatherReport( xml, defaultZip )
                );

                Assert.AreEqual( QueryErrors.InvalidZip, error.ErrorCode );
            }
        }

        /// <summary>
        /// Tests to make sure we can handle a generic SOAP error from NOAA properly.
        /// </summary>
        [Test]
        public void TestGenericErrorResponse()
        {
            string xml = ReadFile( Path.Combine( testFilesLocation, "SampleError.xml" ) );

            {
                QueryException error = Assert.Throws<QueryException>( () =>
                    XmlLoader.ParseLatitudeLongitude( xml, defaultZip )
                );

                Assert.AreEqual( QueryErrors.ServerError, error.ErrorCode );
            }

            {
                QueryException error = Assert.Throws<QueryException>( () =>
                    XmlLoader.ParseWeatherReport( xml, defaultZip )
                );

                Assert.AreEqual( QueryErrors.ServerError, error.ErrorCode );
            }
        }

        /// <summary>
        /// Tests to make sure we can handle when NOAA returns a bad zip code.
        /// </summary>
        [Test]
        public void TestBadZipCode()
        {
            string xml = ReadFile( Path.Combine( testFilesLocation, "InvalidZipCode.xml" ) );
            QueryException error = Assert.Throws<QueryException>( () =>
                XmlLoader.ParseLatitudeLongitude( xml, defaultZip )
            );

            Assert.AreEqual( QueryErrors.InvalidZip, error.ErrorCode );
        }

        /// <summary>
        /// Tests to make sure we can handle NOAA returning a bad Lat/Lon
        /// string.
        /// </summary>
        [Test]
        public void TestBadLatLonResponse()
        {
            string xml = ReadFile( Path.Combine( testFilesLocation, "InvalidLatLon.xml" ) );

            QueryException error = Assert.Throws<QueryException>(
                () => XmlLoader.ParseLatitudeLongitude( xml, defaultZip )
            );

            Assert.AreEqual( QueryErrors.InvalidLatLon, error.ErrorCode );
        }

        // -------- Test Helpers --------

        /// <summary>
        /// Reads the entire file and returns the string.
        /// </summary>
        /// <param name="fileLoaction">The file loation.</param>
        private string ReadFile( string fileLoaction )
        {
            string fileContents;
            using( FileStream infile = new FileStream( fileLoaction, FileMode.Open, FileAccess.Read ) )
            {
                using( StreamReader reader = new StreamReader( infile ) )
                {
                    fileContents = reader.ReadToEnd();
                }
            }
            return fileContents;
        }
    }
}