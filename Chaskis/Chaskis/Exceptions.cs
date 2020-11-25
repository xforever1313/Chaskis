//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Chaskis
{
    /// <summary>
    /// Exception that gets thrown if we can't load a plugin.
    /// </summary>
    public class PluginLoadException : Exception
    {
        public PluginLoadException( string msg ) : 
            base( msg )
        {
        }
    }
}
