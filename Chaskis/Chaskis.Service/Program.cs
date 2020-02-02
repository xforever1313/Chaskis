//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chaskis.Service
{
    public class Program
    {
        public static void Main( string[] args )
        {
            CreateHostBuilder( args ).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder( string[] args )
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder( args );

            if( Environment.OSVersion.Platform == PlatformID.Win32NT )
            {
                hostBuilder = hostBuilder.UseWindowsService();
            }
            else
            {
                hostBuilder = hostBuilder.UseSystemd();
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
