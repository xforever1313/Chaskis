//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.ArgumentBinder;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "make_distro" )]
    [TaskDescription( "Runs the Chaskis CLI installer and puts a disto in the specified location (using arguemnt 'output')" )]
    public class MakeDistroTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            DistroCreatorConfig config = context.CreateFromArguments<DistroCreatorConfig>();
            config.Target = "Release";

            DistroCreator creator = new DistroCreator( context, config );
            creator.CreateDistro();
        }
    }
}
