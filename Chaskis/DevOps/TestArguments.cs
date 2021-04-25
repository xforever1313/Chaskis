//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.ArgumentBinder;

namespace DevOps
{
    public class TestArguments
    {
        // ---------------- Fields ----------------

        public static readonly string CoverageFilter = "+[*]Chaskis*";

        // ---------------- Constructor ----------------

        public TestArguments()
        {
            this.RunWithCodeCoverage = false;
        }

        // ---------------- Properties ----------------

        [BooleanArgument(
            "code_coverage",
            Description = "Should we run this with code coverage?",
            DefaultValue = false
        )]
        public bool RunWithCodeCoverage { get; set; }
    }
}
