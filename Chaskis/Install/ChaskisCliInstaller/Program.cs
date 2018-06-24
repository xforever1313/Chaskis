//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaskis.ChaskisCliInstaller
{
    /// <summary>
    /// This program installs chaskis.
    /// </summary>
    class Program
    {
        static int Main( string[] args )
        {
            if( args.Length != 6 )
            {
                PrintUsage();
                return 0;
            }

            try
            {
                Installer installer = new Installer( args[0], args[1], args[2], args[3], args[4], args[5] );
                installer.Start();
            }
            catch( Exception e )
            {
                Console.WriteLine( e.ToString() );
                return e.GetHashCode();
            }

            return 0;
        }

        static void PrintUsage()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( "This CLI program is used to install Chaskis." );
            builder.AppendLine();
            builder.AppendLine( "Usage: ChaskisCliInstaller.exe slnDir rootDir wixXmlFile debug|release exeruntime pluginruntime" );
            builder.AppendLine();
            builder.AppendLine( "slnDir - Where Chaskis.sln lives" );
            builder.AppendLine( "rootDir - Where the install root will be (e.g. /usr/lib/).  The Chaskis folder gets created in here." );
            builder.AppendLine( "wixXmlFile - The Wix XML file to use." );
            builder.AppendLine( "debug|release - Which target we are using." );
            builder.AppendLine( "exeruntime - Which dotnet runtime we are using for executables." );
            builder.AppendLine( "pluginruntime - Which dotnet runtime or standard we are using for plugins" );

            Console.WriteLine( builder.ToString() );
        }
    }
}
