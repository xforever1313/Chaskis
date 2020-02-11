//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Chaskis.RegressionTests.TestCore
{
    public class ChaskisFixtureConfig
    {
        // ---------------- Constructor ----------------

        public ChaskisFixtureConfig()
        {
            this.Environment = null;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The environment to use during testing.
        /// Set to null to use the default environment.
        /// </summary>
        public string Environment { get; set; }

        public ushort Port { get; set; }
    }
}
