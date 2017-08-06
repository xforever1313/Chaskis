//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using ChaskisCore;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ChaskisEventTest
    {
        // ---------------- Fields ----------------

        private ChaskisEventFactory factory;

        private string pluginName;
        private string targetPluginName;

        private string argStr;
        private IReadOnlyList<string> expectedArgs;

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.factory = TestHelpers.CreateEventFactory();
            this.pluginName = TestHelpers.FactoryPluginNames[0];
            this.targetPluginName = TestHelpers.FactoryPluginNames[1];


            argStr = "ARG1 ARG2 ARG3";
            this.expectedArgs = this.argStr.Split( ' ' );
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that if we try to create multiple
        /// ChaskisEventFactories, we get an exception.
        /// </summary>
        [Test]
        public void DoubleInstanceFailure()
        {
            Assert.Throws<InvalidOperationException>(
                () => ChaskisEventFactory.CreateInstance( TestHelpers.FactoryPluginNames )
            );
        }

        /// <summary>
        /// Ensures that a targeted plugin event results in the correct string.
        /// </summary>
        [Test]
        public void CreateTargetedPluginEvent()
        {
            ChaskisEvent e = this.factory.EventCreators[pluginName].CreateTargetedEvent(
                this.targetPluginName,
                this.expectedArgs
            );

            Assert.AreEqual(
                "CHASKIS PLUGIN " + this.pluginName.ToUpper() + " " + this.targetPluginName.ToUpper() + " ARG1 ARG2 ARG3",
                e.ToString()
            );
        }

        /// <summary>
        /// Ensures that a targeted plugin event results in the correct string.
        /// </summary>
        [Test]
        public void CreateTargetedPluginEventNoArgs()
        {
            ChaskisEvent e = this.factory.EventCreators[pluginName].CreateTargetedEvent(
                this.targetPluginName,
                new List<string>()
            );

            Assert.AreEqual(
                "CHASKIS PLUGIN " + this.pluginName.ToUpper() + " " + this.targetPluginName.ToUpper(),
                e.ToString()
            );
        }

        /// <summary>
        /// Ensures a BCAST event results in the correct string.
        /// </summary>
        [Test]
        public void CreateBCastEvent()
        {
            ChaskisEvent e = this.factory.EventCreators[pluginName].CreateBcastEvent(
                this.expectedArgs
            );

            Assert.AreEqual(
                "CHASKIS PLUGIN " + this.pluginName.ToUpper()+ " " + ChaskisEvent.BroadcastEventStr + " ARG1 ARG2 ARG3",
                e.ToString()
            );
        }

        /// <summary>
        /// Ensures a BCAST event with no args results in the correct string.
        /// </summary>
        [Test]
        public void CreateBCastEventNoArgs()
        {
            ChaskisEvent e = this.factory.EventCreators[pluginName].CreateBcastEvent(
                new List<string>()
            );

            Assert.AreEqual(
                "CHASKIS PLUGIN " + this.pluginName.ToUpper() + " " + ChaskisEvent.BroadcastEventStr,
                e.ToString()
            );
        }
    }
}
