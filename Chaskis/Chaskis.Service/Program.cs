//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SethCS.Extensions;

namespace Chaskis.Service
{
    public class Program
    {
        // ---------------- Properties ----------------

        /// <summary>
        /// Save off args so we can pass it into the Chaskis Main.
        /// </summary>
        public static IReadOnlyList<string> Args { get; private set; }

        // ---------------- Functions ----------------

        public static void Main( string[] args )
        {
            CreateHostBuilder( args ).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder( string[] args )
        {
            Args = args;

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder( args );

            bool isDocker = Environment.UserName.EqualsIgnoreCase( "ContainerUser" );
            if( isDocker )
            {
                Console.WriteLine( "Docker Environment Detected" );
            }
            else
            {
                if( Environment.OSVersion.Platform == PlatformID.Win32NT )
                {
                    hostBuilder = hostBuilder.UseWindowsService();
                }
                else
                {
                    hostBuilder = hostBuilder.UseSystemd();
                }
            }

            return hostBuilder.ConfigureServices(
                ( hostContext, services ) =>
                {
                    services.AddHostedService<Worker>();
                }
            );
        }
    }
}
