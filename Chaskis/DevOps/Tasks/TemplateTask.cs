
//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.Frosting;
using DevOps.Template;

namespace DevOps.Tasks
{
    [TaskName( "template" )]
    public class TemplateTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            FilesToTemplate templateFiles = new FilesToTemplate( context.Paths );
            Templatizer templatizer = new Templatizer( context.TemplateConstants, templateFiles );
            templatizer.Template();
        }
    }
}
