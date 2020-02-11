//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using SethCS.Basic;
using SethCS.Extensions;

namespace Chaskis.RegressionTests.TestCore
{
    public class Logger
    {
        private readonly Dictionary<string, GenericLogger> logs;

        private static readonly Logger instance;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        private Logger()
        {
            this.logs = new Dictionary<string, GenericLogger>();
        }

        static Logger()
        {
            instance = new Logger();
        }

        /// <summary>
        /// Gets the log of the given context (e.g. Console Out).
        /// </summary>
        public static GenericLogger GetLogFromContext( string context )
        {
            lock( instance.logs )
            {
                if( instance.logs.ContainsKey( context ) == false )
                {
                    GenericLogger logger = new GenericLogger();
                    logger.OnWriteLine += delegate ( string line )
                    {
                        Console.Write(
                            "{0}\t{1}>\t{2}",
                            DateTime.Now.ToTimeStampString(),
                            context,
                            line
                        );
                        Console.Out.Flush();
                    };
                    instance.logs[context] = logger;
                }

                return instance.logs[context];
            }
        }

        /// <summary>
        /// Gets the Console Out Log.
        /// </summary>
        public static GenericLogger GetConsoleOutLog()
        {
            return GetLogFromContext( "Console Out" );
        }
    }
}
