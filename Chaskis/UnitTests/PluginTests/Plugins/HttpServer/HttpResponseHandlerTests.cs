//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chaskis.Core;
using Chaskis.Plugins.HttpServer;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.HttpServer
{
    [TestFixture]
    public class HttpResponseHandlerTests
    {
        // ---------------- Fields ----------------

        private Mock<IIrcWriter> mockWriter;

        private HttpResponseHandler uut;

        private const string channel = "#mychannel";
        private const string remoteUser = "someuser";
        private const string method = "POST";
        private const string message = "Hello, World!";

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
            this.mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.uut = new HttpResponseHandler( this.mockWriter.Object );
            this.uut.IsIrcConnected = true;
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that if we are not a POST request, we get an error.
        /// </summary>
        [Test]
        public void InvalidMethodTest()
        {
            List<string> methods = new List<string>
            {
                "GET",
                "HEAD",
                "PUT",
                "DELETE",
                "CONNECT",
                "OPTIONS",
                "TRACE"
            };

            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel
            };
            foreach( string method in methods )
            {
                HttpResponseInfo info = this.uut.HandleResposne( "/part", method, queryString );
                Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
                Assert.AreEqual( ErrorMessage.InvalidMethod, info.Error );
            }
        }

        /// <summary>
        /// Ensures that if we pass in an invalid format, we get an error.
        /// </summary>
        [Test]
        public void InvalidFormatTest()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel,
                ["format"] = "json"
            };

            HttpResponseInfo info = this.uut.HandleResposne( "/part", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.InvalidFormat, info.Error );
        }

        /// <summary>
        /// Ensures that if we are not connected, we get a server error.
        /// </summary>
        [Test]
        public void NotConnectedTest()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel
            };

            this.uut.IsIrcConnected = false;

            HttpResponseInfo info = this.uut.HandleResposne( "/part", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ServerError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.NotConnectedToIrc, info.Error );
        }

        // -------- PRIVMSG --------

        [Test]
        public void PrivmsgSuccessTest()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel,
                ["message"] = message
            };

            this.mockWriter.Setup(
                m => m.SendMessage( message, channel )
            );

            HttpResponseInfo info = this.uut.HandleResposne( "/privmsg", method, queryString );
            Assert.AreEqual( HttpResponseStatus.Ok, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.None, info.Error );

            this.mockWriter.VerifyAll();
        }

        [Test]
        public void PrivmsgMissingChannelTest()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["message"] = message
            };

            HttpResponseInfo info = this.uut.HandleResposne( "/privmsg", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.PrivMsgMissingParameters, info.Error );
        }

        [Test]
        public void PrivmsgMissingMessageTest()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel
            };

            HttpResponseInfo info = this.uut.HandleResposne( "/privmsg", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.PrivMsgMissingParameters, info.Error );
        }

        // -------- KICK --------

        [Test]
        public void KickWithoutReasonSuccess()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel,
                ["user"] = remoteUser
            };

            this.mockWriter.Setup(
                m => m.SendKick( remoteUser, channel, null )
            );

            HttpResponseInfo info = this.uut.HandleResposne( "/kick", method, queryString );
            Assert.AreEqual( HttpResponseStatus.Ok, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.None, info.Error );

            this.mockWriter.VerifyAll();
        }

        [Test]
        public void KickWithReasonSuccess()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel,
                ["message"] = message,
                ["user"] = remoteUser
            };

            this.mockWriter.Setup(
                m => m.SendKick( remoteUser, channel, message )
            );

            HttpResponseInfo info = this.uut.HandleResposne( "/kick", method, queryString );
            Assert.AreEqual( HttpResponseStatus.Ok, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.None, info.Error );

            this.mockWriter.VerifyAll();
        }

        [Test]
        public void KickMissingChannel()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["user"] = remoteUser
            };

            HttpResponseInfo info = this.uut.HandleResposne( "/kick", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.KickMsgMissingParameters, info.Error );
        }

        [Test]
        public void KickMissingUser()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel
            };

            HttpResponseInfo info = this.uut.HandleResposne( "/kick", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.KickMsgMissingParameters, info.Error );
        }

        // -------- BCAST --------

        [Test]
        public void BcastSuccess()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["message"] = message
            };

            this.mockWriter.Setup(
                m => m.SendBroadcastMessage( message )
            );

            HttpResponseInfo info = this.uut.HandleResposne( "/bcast", method, queryString );
            Assert.AreEqual( HttpResponseStatus.Ok, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.None, info.Error );

            this.mockWriter.VerifyAll();
        }

        [Test]
        public void BcastMissingMessage()
        {
            NameValueCollection queryString = new NameValueCollection
            {
            };

            HttpResponseInfo info = this.uut.HandleResposne( "/bcast", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.BcastMissingParameters, info.Error );
        }

        // -------- PART --------

        [Test]
        public void PartWithoutReasonSuccess()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel
            };

            this.mockWriter.Setup(
                m => m.SendPart( null, channel )
            );

            HttpResponseInfo info = this.uut.HandleResposne( "/part", method, queryString );
            Assert.AreEqual( HttpResponseStatus.Ok, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.None, info.Error );

            this.mockWriter.VerifyAll();
        }

        [Test]
        public void PartWithReasonSuccess()
        {
            NameValueCollection queryString = new NameValueCollection
            {
                ["channel"] = channel,
                ["message"] = message
            };

            this.mockWriter.Setup(
                m => m.SendPart( message, channel )
            );

            HttpResponseInfo info = this.uut.HandleResposne( "/part", method, queryString );
            Assert.AreEqual( HttpResponseStatus.Ok, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.None, info.Error );

            this.mockWriter.VerifyAll();
        }

        [Test]
        public void PartMissingChannel()
        {
            NameValueCollection queryString = new NameValueCollection
            {
            };

            HttpResponseInfo info = this.uut.HandleResposne( "/part", method, queryString );
            Assert.AreEqual( HttpResponseStatus.ClientError, info.ResponseStatus );
            Assert.AreEqual( ErrorMessage.PartMissingParameters, info.Error );
        }
    }
}
