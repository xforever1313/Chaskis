//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Reflection;
using Cake.Frosting;
using Seth.CakeLib;

namespace DevOps
{
    class Program
    {
        static int Main( string[] args )
        {
            string exeDir = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            string repoRoot = Path.Combine(
                exeDir, // app
                "..", // Debug
                "..", // Bin
                "..", // DevOps
                "..", // Src
                ".."  // Root
            );

            return new CakeHost()
                .UseContext<ChaskisContext>()
                .SetToolPath( ".cake" )
                .InstallTool( new Uri( "nuget:?package=OpenCover&version=4.7.922" ) )
                .InstallTool( new Uri( "nuget:?package=ReportGenerator&version=4.8.8" ) )
                .AddAssembly( SethCakeLib.GetAssembly() )
                .UseWorkingDirectory( repoRoot )
                .Run( args );
        }
    }
}
