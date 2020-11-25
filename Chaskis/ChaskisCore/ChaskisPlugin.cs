//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    /// <summary>
    /// Use this attribute on a plugin class, and we'll
    /// read it in.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class ChaskisPlugin : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pluginName">
        /// Note that the name will have whitespace replaced with '_' and set to lowercase.
        /// </param>
        public ChaskisPlugin( string pluginName )
        {
            this.PluginName = Regex.Replace( pluginName, @"\s", "_" ).ToLower();
        }

        public string PluginName { get; private set; }
    }
}
