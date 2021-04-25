//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Diagnostics;

namespace Chaskis.Core
{
    public abstract class BaseIrcHandler : IIrcHandler
    {
        // ---------------- Fields ----------------

        // ---------------- Constructor ----------------

        protected BaseIrcHandler()
        {
            this.KeepHandling = true;

            // Skip the constructors.
            this.CreationStack = new StackTrace( 2, true );
        }

        // ---------------- Properties ----------------

        public StackTrace CreationStack { get; private set; }

        public bool KeepHandling { get; set; }

        // ---------------- Functions ----------------

        public abstract void HandleEvent( HandlerArgs args );
    }
}
