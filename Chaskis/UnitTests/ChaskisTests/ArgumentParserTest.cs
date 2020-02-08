//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Cli;
using NUnit.Framework;

namespace Chaskis.UnitTests.ChaskisTests
{
    [TestFixture]
    public class ArgumentParserTest
    {
        // -------- Fields --------

        /// <summary>
        /// The test root directory.
        /// </summary>
        private static readonly string rootDir = Cli.Chaskis.DefaultRootDirectory;

        // -------- Tests --------

        /// <summary>
        /// Ensures the default are sane.
        /// </summary>
        [Test]
        public void DefaultTest()
        {
            string[] args = { };

            ArgumentParser uut = new ArgumentParser( args, rootDir );
            Assert.AreEqual( rootDir, uut.ChaskisRoot );
            Assert.IsFalse( uut.FailOnPluginFailure );
            Assert.IsFalse( uut.PrintHelp );
            Assert.IsFalse( uut.PrintVersion );
            Assert.IsFalse( uut.BootStrap );
            Assert.IsTrue( uut.IsValid );
        }

        /// <summary>
        /// Ensures help is set to true.
        /// </summary>
        [Test]
        public void HelpTest()
        {
            foreach( string s in new string[] { "--help", "-h", "/?" } )
            {
                string[] args = { s };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( rootDir, uut.ChaskisRoot );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsTrue( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsFalse( uut.BootStrap );
                Assert.IsTrue( uut.IsValid );
            }
        }

        /// <summary>
        /// Ensures version is set to true.
        /// </summary>
        [Test]
        public void VersionTest()
        {
            string[] args = { "--version" };

            ArgumentParser uut = new ArgumentParser( args, rootDir );
            Assert.AreEqual( rootDir, uut.ChaskisRoot );
            Assert.IsFalse( uut.FailOnPluginFailure );
            Assert.IsFalse( uut.PrintHelp );
            Assert.IsTrue( uut.PrintVersion );
            Assert.IsFalse( uut.BootStrap );
            Assert.IsTrue( uut.IsValid );
        }

        /// <summary>
        /// Ensures the plugin failure argument is working.
        /// </summary>
        [Test]
        public void FailOnPluginFailureTest()
        {
            {
                string[] args = { "--failOnBadPlugin=yes" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( rootDir, uut.ChaskisRoot );
                Assert.IsTrue( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsTrue( uut.IsValid );
            }
            {
                string[] args = { "--failOnBadPlugin=no" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( rootDir, uut.ChaskisRoot );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsTrue( uut.IsValid );
            }
            {
                string[] args = { "--failOnBadPlugin=derp" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( rootDir, uut.ChaskisRoot );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsFalse( uut.IsValid );
            }
        }

        /// <summary>
        /// Using something with non-default root.
        /// </summary>
        [Test]
        public void DifferentRootTest()
        {
            const string differentRoot = "/usr/lib/derp";
            string[] args = { "--chaskisroot=" + differentRoot };

            ArgumentParser uut = new ArgumentParser( args, differentRoot );
            Assert.AreEqual( differentRoot, uut.ChaskisRoot );
            Assert.IsFalse( uut.FailOnPluginFailure );
            Assert.IsFalse( uut.PrintHelp );
            Assert.IsFalse( uut.PrintVersion );
            Assert.IsFalse( uut.BootStrap );
            Assert.IsTrue( uut.IsValid );
        }

        /// <summary>
        /// Ensures bootstrap arg is used correctly.
        /// </summary>
        [Test]
        public void BootstrapTest()
        {
            string[] args = { "--bootstrap" };

            ArgumentParser uut = new ArgumentParser( args, rootDir );
            Assert.AreEqual( rootDir, uut.ChaskisRoot );
            Assert.IsFalse( uut.FailOnPluginFailure );
            Assert.IsFalse( uut.PrintHelp );
            Assert.IsFalse( uut.PrintVersion );
            Assert.IsTrue( uut.BootStrap );
            Assert.IsTrue( uut.IsValid );
        }
    }
}