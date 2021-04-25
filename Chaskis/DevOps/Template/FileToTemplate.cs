//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Cake.Core.IO;

namespace DevOps.Template
{
    public class FileToTemplate
    {
        // ---------------- Constructor ----------------

        public FileToTemplate( FilePath input, FilePath output ) :
            this( input, output, null )
        {
        }

        public FileToTemplate( FilePath input, FilePath output, IReadOnlyList<string> defines )
        {
            this.Input = input;
            this.Output = output;

            if( defines == null )
            {
                this.Defines = new List<string>();
            }
            else
            {
                this.Defines = defines;
            }

            this.LineEnding = null;
        }

        // ---------------- Properties ----------------

        public FilePath Input { get; private set; }

        public FilePath Output { get; private set; }

        public IReadOnlyList<string> Defines { get; private set; }

        /// <summary>
        /// After templating the file, all line endings
        /// with this string.
        ///
        /// Leave null to not replace.
        /// </summary>
        public string LineEnding { get; set; }
    }
}
